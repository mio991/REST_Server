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
        /// <summary>
        /// URL Seperator Char
        /// </summary>
		public const char SEPERATORCHAR = '/';

        /// <summary>
        /// Loging Entrypoint
        /// </summary>
		private ILog m_Log;

        /// <summary>
        /// A Collection of the loaded Plugins 
        /// </summary>
		private Dictionary<string, PluginBase> m_Plugins;

        /// <summary>
        /// The Listener used to Received the Requests.
        /// </summary>
		private HttpListener m_Listener;

        /// <summary>
        /// A instance of the Class which Handles the Requests.
        /// </summary>
		private RequestHandler m_Handler;

        /// <summary>
        /// The Directory the Server is Working in.
        /// </summary>
		private string m_WorkingDirectory;

        /// <summary>
        /// The Resource used to Entry the Tree.
        /// </summary>
		private CollectingResource m_RootResource;

        /// <summary>
        /// Used to check if Disposed.
        /// </summary>
		private bool m_ListenerIsDisposed = false;

        /// <summary>
        /// A collection of all loaded plugins.
        /// </summary>
		public Dictionary<string, PluginBase> Plugins {
			get {
				return m_Plugins;
			}
		}

        /// <summary>
        /// The root-resource which will be called for each request.
        /// </summary>
		public CollectingResource RootResource {
			get {
				return m_RootResource;
			}
		}

        /// <summary>
        /// The logging entrypoint
        /// </summary>
		public ILog Log {
			get {
				return m_Log;
			}
		}

        /// <summary>
        /// The Directory the server is runing in
        /// </summary>
        public string WorkingDirectory
        {
            get
            {
                return m_WorkingDirectory;
            }
        }

        /// <summary>
        /// A methode to initialise the logging.
        /// </summary>
		private void InitLoging ()
		{
            // excepted Log4net config File
			string serverLog4net = m_WorkingDirectory + Path.DirectorySeparatorChar + "server.log4net";

			// testfor server.log4net
			if (!File.Exists (serverLog4net)) {
				throw new FileNotFoundException ("Couldn't finde config file.", serverLog4net);
			}

            //Loading config
			log4net.Config.XmlConfigurator.Configure (new FileInfo (serverLog4net));
			m_Log = log4net.LogManager.GetLogger ("root");
		}


        /// <summary>
        /// Initialising the Listener with the data from the host-node of the server.config.
        /// </summary>
        /// <param name="host">The host-node of the server.config</param>
		private void InitListener (XmlNode host)
		{
            //Init a Listener
			m_Listener = new HttpListener ();

            //geting listener-nodes
			foreach (XmlNode listenerNode in host.ChildNodes) {
				if (listenerNode.Name == "listener") {

                    //tell Listener to listen at some port
					m_Listener.Prefixes.Add (listenerNode.Attributes ["url"].InnerText);

				}
			}
		}

        /// <summary>
        /// Initialise one Plugin with the plugin-node.
        /// </summary>
        /// <param name="plugin">The plugins-node</param>
		private void InitPlugin (XmlNode plugin)
		{
			XmlNode settings = null, assemblyNode = null;

            //geting the settings-node and the assembly-node
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

            //Creating expected Path of the Assembly
			string path = Path.Combine (m_WorkingDirectory,  assemblyNode.Attributes ["path"].Value);

            //Load Plugin assembly
            Assembly assembly = Assembly.LoadFile(path);

            Log.Info(String.Format("Load Assembly '{0}'", path));

            //Initilise Plugin with the settings
			PluginInitTypeAttribute initType = (PluginInitTypeAttribute)assembly.GetCustomAttributes (typeof(PluginInitTypeAttribute), true)[0];
			m_Plugins.Add(plugin.Attributes["name"].Value, (PluginBase)Activator.CreateInstance (initType.InitType, settings, this));
		}

        /// <summary>
        /// Initialising all Plugins in the plugins-node
        /// </summary>
        /// <param name="plugins">The plugins-node containing plugin-nodes</param>
		private void InitPlugins (XmlNode plugins)
		{
            //dividing the node in the Plugins
			foreach (XmlNode plugin in plugins.ChildNodes) {
				if (plugin.Name == "plugin") {
					InitPlugin (plugin);
				}
			}
		}

        /// <summary>
        /// Create a new Server configed by the server.config in the working-Directory
        /// </summary>
        /// <param name="workingDiretory">The Directory from wich the Server takes all the components and configuration</param>
		public Server (string workingDiretory) // TODO Adding DB Connection
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

        /// <summary>
        /// The Callback for asyncrounus listening
        /// </summary>
		private AsyncCallback m_CallBack;

        /// <summary>
        /// Start the Server
        /// </summary>
		public void Start ()
		{
			m_Listener.Start ();
			foreach (string prefix  in m_Listener.Prefixes) {
				Log.Info (String.Format ("Listening at '{0}'", prefix));
			}

			m_Listener.BeginGetContext (m_CallBack, null);
		}

        /// <summary>
        /// Handles the Requests
        /// </summary>
        /// <param name="result"></param>
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

        /// <summary>
        /// If all components are Disposed
        /// </summary>
		public bool IsDisposed
		{
			get {
				return m_ListenerIsDisposed;
			}
		}

        /// <summary>
        /// Stops the Server
        /// </summary>
		public void Dispose ()
		{
			Log.Info ("Stoping Server");
			m_Listener.Abort ();
			while(!IsDisposed){}
			Log.Info ("End Log");
		}
	}
}

