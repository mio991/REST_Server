using System;
using System.Xml;
using System.IO;
using log4net;
using System.Reflection;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Text;

namespace REST_Server
{
	public class Server : IDisposable
	{
		public const char SEPERATORCHAR = '/';

		private ILog m_Log;
		private Dictionary<string, PluginBase> m_Plugins;
		private HttpListener m_Listener;
		private RequestHandler m_Handler;
		private string m_WorkingDirectory;
		private ICollectingResource m_RootResource;

		private bool m_ListenerIsDisposed = false;

		public Dictionary<string, PluginBase> Plugins {
			get {
				return m_Plugins;
			}
		}

		public ICollectingResource RootResource {
			get {
				return m_RootResource;
			}
		}

		public ILog Log {
			get {
				return m_Log;
			}
		}

		private void InitLoging ()
		{
			string serverLog4net = m_WorkingDirectory + Path.DirectorySeparatorChar + "server.log4net";

			// testfor server.log4net
			if (!File.Exists (serverLog4net)) {
				throw new FileNotFoundException ("Couldn't finde config file.", serverLog4net);
			}

			log4net.Config.XmlConfigurator.Configure (new FileInfo (serverLog4net));
			m_Log = log4net.LogManager.GetLogger ("root");
		}

		private void InitListener (XmlNode host)
		{
			m_Listener = new HttpListener ();
			foreach (XmlNode listenerNode in host.ChildNodes) {
				if (listenerNode.Name == "listener") {
					m_Listener.Prefixes.Add (listenerNode.Attributes ["url"].InnerText);
				}
			}
		}

		private void InitPlugin (XmlNode plugin)
		{
			XmlNode settings = null, assemblyNode = null;

			foreach (XmlNode node in plugin.ChildNodes) {
				switch (node.Name) {
				case "assembly":
					assemblyNode = node;
					break;
				case "settings":
					settings = node;
					break;
				}
			}
			string path = Path.Combine (m_WorkingDirectory,  assemblyNode.Attributes ["path"].Value);

			Log.Info(String.Format("Load Assembly '{0}'", path));

			Assembly assembly = Assembly.LoadFile (path);
			PluginInitTypeAttribute initType = (PluginInitTypeAttribute)assembly.GetCustomAttributes (typeof(PluginInitTypeAttribute), true)[0];
			m_Plugins.Add(plugin.Attributes["name"].Value, (PluginBase)Activator.CreateInstance (initType.InitType, settings, this));
		}

		private void InitPlugins (XmlNode plugins)
		{
			foreach (XmlNode plugin in plugins.ChildNodes) {
				if (plugin.Name == "plugin") {
					InitPlugin (plugin);
				}
			}
		}

		public Server (string workingDiretory)
		{
			m_WorkingDirectory = workingDiretory;

			InitLoging ();

			Log.Info ("Start Log");

			try {
				m_Plugins = new Dictionary<string, PluginBase> ();

				string serverConfig = m_WorkingDirectory + Path.DirectorySeparatorChar + "server.config";

				m_CallBack = new AsyncCallback (ListenerCallBack);
				m_Handler = new RequestHandler (this, new UnicodeEncoding ());
				m_RootResource = new CollectingResource (this, "root");

				// testfor server.config
				if (!File.Exists (serverConfig)) {
					throw new FileNotFoundException ("Couldn't finde config file.", serverConfig);
				}

				XmlDocument doc = new XmlDocument ();
				doc.Load (serverConfig);

				InitListener (doc.GetElementsByTagName ("host") [0]);

				InitPlugins(doc.GetElementsByTagName("plugins")[0]);

			} catch (Exception ex) {
				Log.Fatal ("During the Initiation is an Error ocured", ex);
			}
		}

		AsyncCallback m_CallBack;

		public void Start ()
		{
			m_Listener.Start ();
			foreach (string prefix  in m_Listener.Prefixes) {
				Log.Info (String.Format ("Listening at '{0}'", prefix));
			}

			m_Listener.BeginGetContext (m_CallBack, null);
		}

		private void ListenerCallBack (IAsyncResult result)
		{
			try {
				HttpListenerContext context = m_Listener.EndGetContext (result);
				m_Handler.HandleRequest (context);
				m_Listener.BeginGetContext (m_CallBack, null);
			} catch (ObjectDisposedException) {
				Log.Info ("Stoped Listening");
				m_ListenerIsDisposed = true;
			} catch (Exception ex) {
				Log.Fatal (ex);
			}
		}

		public bool IsDisposed
		{
			get {
				return m_ListenerIsDisposed;
			}
		}

		public void Dispose ()
		{
			Log.Info ("Stoping Server");
			m_Listener.Abort ();
			while(!IsDisposed){}
			Log.Info ("End Log");
		}
	}
}

