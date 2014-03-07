using System;
using System.Xml;
using System.IO;
using log4net;
using System.Reflection;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Text;
using REST_Server.Resource;
using REST_Server.Plugins;
using System.Data;
using System.Data.Common;

namespace REST_Server
{
    public class Server : IDisposable
    {
        /// <summary>
        /// URL Seperator Char
        /// </summary>
        public const char SEPERATORCHAR = '/';

        /// <summary>
        /// DataBase-Connection
        /// </summary>
        private IDbConnection m_DBConnection;

        private Dictionary<string, Dictionary<string, object>> m_SessionsVariables;

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
        public Dictionary<string, PluginBase> Plugins
        {
            get
            {
                return m_Plugins;
            }
        }

        /// <summary>
        /// The root-resource which will be called for each request.
        /// </summary>
        public CollectingResource RootResource
        {
            get
            {
                return m_RootResource;
            }
        }

        /// <summary>
        /// The logging entrypoint
        /// </summary>
        public ILog Log
        {
            get
            {
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
        private void InitLoging()
        {
            string serverLog4net = m_WorkingDirectory + Path.DirectorySeparatorChar + "server.log4net";

            if (!File.Exists(serverLog4net))
            {
                throw new FileNotFoundException("Couldn't finde config file.", serverLog4net);
            }

            log4net.Config.XmlConfigurator.Configure(new FileInfo(serverLog4net));
            m_Log = log4net.LogManager.GetLogger("root");
        }


        /// <summary>
        /// Initialising the Listener with the data from the host-node of the server.config.
        /// </summary>
        /// <param name="host">The host-node of the server.config</param>
        private void InitListener(XmlNode host)
        {
            Log.Info("Init Listener");

            m_Listener = new HttpListener();
            foreach (XmlNode listenerNode in host.ChildNodes)
            {
                if (listenerNode.Name == "listener")
                {
                    m_Listener.Prefixes.Add(listenerNode.Attributes["url"].InnerText);
                }
            }
        }

        /// <summary>
        /// Initialise one Plugin with the plugin-node.
        /// </summary>
        /// <param name="plugin">The plugins-node</param>
        private void InitPlugin(XmlNode plugin)
        {
            XmlNode settings = null, assemblyNode = null;

            foreach (XmlNode node in plugin.ChildNodes)
            {
                switch (node.Name)
                {
                    case "assembly":
                        assemblyNode = node;
                        break;
                    case "settings":
                        settings = node;
                        break;
                }
            }

            string path = Path.Combine(m_WorkingDirectory, assemblyNode.Attributes["path"].Value);

            Assembly assembly = Assembly.LoadFile(path);

            Log.Info(String.Format("Load Assembly '{0}'", path));

            PluginInitTypeAttribute initType = (PluginInitTypeAttribute)assembly.GetCustomAttributes(typeof(PluginInitTypeAttribute), true)[0];
            m_Plugins.Add(plugin.Attributes["name"].Value, (PluginBase)Activator.CreateInstance(initType.InitType, settings, this));
        }

        private class PluginLoader
        {
            string m_Assembly;
            XmlNode m_Settings;
            Server m_Server;
            Assembly m_PluginAssembly;
            string m_Name;

            static List<Assembly> m_LoadedAssemblys = new List<Assembly>();

            static PluginLoader()
            {
                AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            }

            static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
            {
                m_LoadedAssemblys.Add(args.LoadedAssembly);
            }

            public PluginLoader(XmlNode plugin, Server server)
            {
                m_Server = server;

                foreach (XmlNode node in plugin.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "assembly":
                            m_Assembly = Path.GetFullPath(Path.Combine(m_Server.WorkingDirectory , node.Attributes["path"].Value));
                            break;
                        case "settings":
                            m_Settings = node;
                            break;
                    }

                    m_Name = plugin.Attributes["name"].Value;
                }
            }

            public void Load()
            {
                bool isLoaded = false;

                foreach (Assembly test in m_LoadedAssemblys)
                {
                    if (test.Location == m_Assembly)
                    {
                        isLoaded = true;
                    }
                }

                if (!isLoaded)
                {
                    m_PluginAssembly = Assembly.LoadFile(m_Assembly);
                    m_Server.Log.Info(String.Format("Load Assembly '{0}'", m_Assembly));
                }
                else
                {
                    m_Server.Log.Info(String.Format("Alredy Load Assembly '{0}'", m_Assembly));
                }
            }

            public void Init()
            {

                m_Server.Log.Info(String.Format("Init Assembly '{0}'", m_Assembly));

                PluginInitTypeAttribute initType = (PluginInitTypeAttribute)m_PluginAssembly.GetCustomAttributes(typeof(PluginInitTypeAttribute), true)[0];
                m_Server.m_Plugins.Add(m_Name, (PluginBase)Activator.CreateInstance(initType.InitType, m_Settings, m_Server));
            }
        }

        /// <summary>
        /// Initialising all Plugins in the plugins-node
        /// </summary>
        /// <param name="plugins">The plugins-node containing plugin-nodes</param>
        private void InitPlugins(XmlNode plugins)
        {
            Log.Info("Init Plugins");

            Dictionary<int, PluginLoader> plugs = new Dictionary<int, PluginLoader>();

            foreach (XmlNode plugin in plugins.ChildNodes)
            {
                if (plugin.Name == "plugin")
                {
                    plugs.Add(int.Parse(plugin.Attributes["loadTime"].Value), new PluginLoader(plugin, this));
                }
            }

            int i = plugs.Count-1;

            while (i >= 0)
            {
                plugs[i].Load();
                i--;
            }

            i++;

            while (i < plugs.Count)
            {
                plugs[i].Init();
                i++;
            }
        }

        /// <summary>
        /// Creating a Connection to a DataBase and Test it.
        /// </summary>
        /// <param name="connectionNode">the Config node wich contains the Connection information</param>
        private void InitDBConnection(XmlNode connectionNode)
        {
            Log.Info("Init Database Connection");

            string connectionString = null, assemblyName = null, type = null;

            foreach (XmlNode child in connectionNode.ChildNodes)
            {
                switch (child.Name)
                {
                    case "connectionString":
                        connectionString = child.Attributes["value"].Value;
                        break;
                    case "connector":
                        assemblyName = child.Attributes["assembly"].Value;
                        type = child.Attributes["type"].Value;
                        break;
                }
            }

            string path = Path.Combine(m_WorkingDirectory, assemblyName);

            Log.Info(String.Format("Load Connector-Assembly: {0}", path));
            Assembly assembly = Assembly.LoadFrom(path);
            
            Log.Info(String.Format("Create Connector-Instance: {0}", type));
            m_DBConnection = assembly.CreateInstance(type) as IDbConnection;

            m_DBConnection.ConnectionString = connectionString;

            Log.Info(String.Format("Test DB-Connection: {0}", m_DBConnection.Database));

            try
            {

                m_DBConnection.Open();

                m_DBConnection.Close();

            }
            catch (DbException ex)
            {
                Log.Error(String.Format("Error ocured while connecting to database.\n\t{0}", ex.Message));
            }
        }

        public IDbConnection DBConnection
        {
            get
            {
                return m_DBConnection;
            }
        }

        /// <summary>
        /// Create a new Server configed by the server.config in the working-Directory
        /// </summary>
        /// <param name="workingDiretory">The Directory from wich the Server takes all the components and configuration</param>
        public Server(string workingDiretory) 
        {
            m_WorkingDirectory = workingDiretory;

            InitLoging();

            Log.Info("Start Log");

            try
            {
                m_Plugins = new Dictionary<string, PluginBase>();

                string serverConfig = m_WorkingDirectory + Path.DirectorySeparatorChar + "server.config";

                m_CallBack = new AsyncCallback(ListenerCallBack);
                m_Handler = new RequestHandler(this, new UnicodeEncoding());
                m_RootResource = new CollectingResource(this, "root");

                if (!File.Exists(serverConfig))
                {
                    throw new FileNotFoundException("Couldn't finde config file.", serverConfig);
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(serverConfig);

                InitListener(doc.GetElementsByTagName("host")[0]);

                InitDBConnection(doc.GetElementsByTagName("dataBase")[0]);

                InitPlugins(doc.GetElementsByTagName("plugins")[0]);

                m_SessionsVariables = new Dictionary<string, Dictionary<string, object>>();

            }
            catch (Exception ex)
            {
                Log.Fatal("During the Initiation is an Error ocured", ex);
            }
        }

        public Dictionary<string, object> GetSessionVariables(HttpListenerContext context)
        {
            Cookie sessionid = context.Request.Cookies["sessionid"];
            return m_SessionsVariables[sessionid.Value];
        }

        /// <summary>
        /// The Callback for asyncrounus listening
        /// </summary>
        private AsyncCallback m_CallBack;

        /// <summary>
        /// Start the Server
        /// </summary>
        public void Start()
        {
            m_Listener.Start();
            foreach (string prefix in m_Listener.Prefixes)
            {
                Log.Info(String.Format("Listening at '{0}'", prefix));
            }

            m_Listener.BeginGetContext(m_CallBack, null);
        }

        /// <summary>
        /// Handles the Requests
        /// </summary>
        /// <param name="result"></param>
        private void ListenerCallBack(IAsyncResult result)
        {
            try
            {
                HttpListenerContext context = m_Listener.EndGetContext(result);
                m_Handler.HandleRequest(context);
                m_Listener.BeginGetContext(m_CallBack, null);
            }
            catch (ObjectDisposedException)
            {
                Log.Info("Stoped Listening");
                m_ListenerIsDisposed = true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
            }
        }

        /// <summary>
        /// If all components are Disposed
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return m_ListenerIsDisposed;
            }
        }

        /// <summary>
        /// Stops the Server
        /// </summary>
        public void Dispose()
        {
            Log.Info("Stoping Server");
            m_Listener.Abort();
            while (!IsDisposed) { }
            Log.Info("End Log");
        }

        public static Dictionary<string, string> GetPostVariables(HttpListenerContext context)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            StreamReader reader = new StreamReader(context.Request.InputStream);
            string request = reader.ReadToEnd();
            string[] variables = request.Split('&');
            foreach (string var in variables)
            {
                string[] s = var.Split('=');
                res.Add(s[0], s[1]);
            }
            return res;
        }
    }
}

