using System;
using System.Xml;
using System.IO;
using log4net;
using System.Reflection;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Text;
using mio991.REST.Server.Resource;
using mio991.REST.Server.Plugins;
using System.Data;
using System.Data.Common;

namespace mio991.REST.Server
{
    public static class Server
    {
        /// <summary>
        /// URL Seperator Char
        /// </summary>
        public const char SEPERATORCHAR = '/';

        /// <summary>
        /// DataBase-Connection
        /// </summary>
        private static Dictionary<Thread, IDbConnection> m_DBConnections = new Dictionary<Thread,IDbConnection>();

        private static string m_ConnectionString;

        private static Dictionary<string, Dictionary<string, object>> m_SessionsVariabless;

        private static Type m_DBConnectionType, ConnectionString;

        /// <summary>
        /// Loging Entrypoint
        /// </summary>
        private static ILog m_Log;

        /// <summary>
        /// A Collection of the loaded Plugins 
        /// </summary>
        private static Dictionary<string, PluginBase> m_Plugins;

        /// <summary>
        /// The Listener used to Received the Requests.
        /// </summary>
        private static HttpListener m_Listener;

        /// <summary>
        /// A instance of the Class which Handles the Requests.
        /// </summary>
        private static RequestHandler m_Handler;

        /// <summary>
        /// The Directory the Server is Working in.
        /// </summary>
        private static string m_WorkingDirectory;

        /// <summary>
        /// The Resource used to Entry the Tree.
        /// </summary>
        private static CollectingResource m_RootResource;

        /// <summary>
        /// Used to check if Disposed.
        /// </summary>
        private static bool m_ListenerIsDisposed = false;

        /// <summary>
        /// A collection of all loaded plugins.
        /// </summary>
        public static Dictionary<string, PluginBase> Plugins
        {
            get
            {
                return m_Plugins;
            }
        }

        /// <summary>
        /// The root-resource which will be called for each request.
        /// </summary>
        public static CollectingResource RootResource
        {
            get
            {
                return m_RootResource;
            }
        }

        /// <summary>
        /// The logging entrypoint
        /// </summary>
        public static ILog Log
        {
            get
            {
                return m_Log;
            }
        }

        /// <summary>
        /// The Directory the server is runing in
        /// </summary>
        public static string WorkingDirectory
        {
            get
            {
                return m_WorkingDirectory;
            }
        }

        /// <summary>
        /// A methode to initialise the logging.
        /// </summary>
        private static void InitLoging()
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
        private static void InitListener(XmlNode host)
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

        /*
        /// <summary>
        /// Initialise one Plugin with the plugin-node.
        /// </summary>
        /// <param name="plugin">The plugins-node</param>
        private static void InitPlugin(XmlNode plugin)
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
            m_Plugins.Add(plugin.Attributes["name"].Value, (PluginBase)Activator.CreateInstance(initType.InitType, settings));
        }
         */

        private class PluginLoader
        {

            string m_Assembly;
            XmlNode m_Settings;
            Assembly m_PluginAssembly;
            string m_Name;

            public PluginLoader(XmlNode plugin)
            {

                foreach (XmlNode node in plugin.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "assembly":
                            m_Assembly = Path.GetFullPath(Path.Combine(Server.WorkingDirectory , node.Attributes["path"].Value));
                            break;
                        case "settings":
                            m_Settings = node;
                            break;
                    }
                }
                m_Name = plugin.Attributes["name"].Value;
            }

            public void Load()
            {
                foreach (Assembly test in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (test.Location == m_Assembly)
                    {
                        m_PluginAssembly = test;
                    }
                }

                if (m_PluginAssembly == null)
                {
                    m_PluginAssembly = Assembly.LoadFile(m_Assembly);
                    Server.Log.Info(String.Format("Load Assembly '{0}'", m_Assembly));
                }
                else
                {
                    Server.Log.Info(String.Format("Alredy Load Assembly '{0}'", m_Assembly));
                }
            }

            public void Init()
            {

                Server.Log.Info(String.Format("Init Assembly '{0}'", m_Assembly));

                PluginInitTypeAttribute initType = (PluginInitTypeAttribute)m_PluginAssembly.GetCustomAttributes(typeof(PluginInitTypeAttribute), true)[0];
                Server.m_Plugins.Add(m_Name, (PluginBase)Activator.CreateInstance(initType.InitType, m_Settings));
            }
        }

        /// <summary>
        /// Initialising all Plugins in the plugins-node
        /// </summary>
        /// <param name="plugins">The plugins-node containing plugin-nodes</param>
        private static void InitPlugins(XmlNode plugins)
        {
            Log.Info("Init Plugins");

            Dictionary<int, PluginLoader> plugs = new Dictionary<int, PluginLoader>();

            foreach (XmlNode plugin in plugins.ChildNodes)
            {
                if (plugin.Name == "plugin")
                {
                    plugs.Add(int.Parse(plugin.Attributes["loadTime"].Value), new PluginLoader(plugin));
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
        private static void InitDBConnection(XmlNode connectionNode)
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

            m_DBConnectionType = assembly.GetType(type);

            IDbConnection db = Activator.CreateInstance(m_DBConnectionType) as IDbConnection;

            db.ConnectionString = connectionString;

            Log.Info(String.Format("Test DB-Connection: {0}", db.Database));

            try
            {

                db.Open();

                db.Close();

            }
            catch (DbException ex)
            {
                Log.Error(String.Format("Error ocured while connecting to database.\n\t{0}", ex.Message));
            }
        }

        public static IDbConnection DBConnection
        {
            get
            {
                if (m_DBConnections.ContainsKey(Thread.CurrentThread))
                {
                    m_DBConnections.Add(Thread.CurrentThread, Activator.CreateInstance(m_DBConnectionType) as IDbConnection);
                    m_DBConnections[Thread.CurrentThread].ConnectionString = m_ConnectionString;

                }
                return m_DBConnections[Thread.CurrentThread];
            }
        }

        /// <summary>
        /// Create a new Server configed by the server.config in the working-Directory
        /// </summary>
        /// <param name="workingDiretory">The Directory from wich the Server takes all the components and configuration</param>
        public static void Init(string workingDiretory) 
        {
            m_WorkingDirectory = workingDiretory;

            InitLoging();

            Log.Info("Start Log");

            try
            {
                m_Plugins = new Dictionary<string, PluginBase>();

                string serverConfig = m_WorkingDirectory + Path.DirectorySeparatorChar + "server.config";

                m_CallBack = new AsyncCallback(ListenerCallBack);
                m_Handler = new RequestHandler(new UnicodeEncoding());
                m_Handler.DoneRequestHandling += m_Handler_DoneRequestHandling;
                m_RootResource = new CollectingResource("root");

                if (!File.Exists(serverConfig))
                {
                    throw new FileNotFoundException("Couldn't finde config file.", serverConfig);
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(serverConfig);

                InitListener(doc.GetElementsByTagName("host")[0]);

                InitDBConnection(doc.GetElementsByTagName("dataBase")[0]);

                InitPlugins(doc.GetElementsByTagName("plugins")[0]);

                m_SessionsVariabless = new Dictionary<string, Dictionary<string, object>>();

            }
            catch (Exception ex)
            {
                Log.Fatal("During the Initiation is an Error ocured", ex);
            }
        }

        static void m_Handler_DoneRequestHandling(object sender, EventArgs e)
        {
            if (m_DBConnections.ContainsKey(Thread.CurrentThread))
            {
                m_DBConnections.Remove(Thread.CurrentThread);
            }
        }

        public static Dictionary<string, object> GetSessionVariables(HttpListenerContext context)
        {
            Cookie sessionid = context.Request.Cookies["sessionid"];
            if (sessionid == null)
            {
                sessionid = new Cookie("sessionid", Guid.NewGuid().ToString());
                context.Request.Cookies.Add(sessionid);
                m_SessionsVariabless.Add(sessionid.Value, new Dictionary<string, object>());
                if (SessionGenerated != null)
                {
                    SessionGenerated(null, new SessionGenaretedEventArgs(m_SessionsVariabless[sessionid.Value]));
                }
            }
            return m_SessionsVariabless[sessionid.Value];
        }

        public static event EventHandler<SessionGenaretedEventArgs> SessionGenerated;

        /// <summary>
        /// The Callback for asyncrounus listening
        /// </summary>
        private static AsyncCallback m_CallBack;

        /// <summary>
        /// Start the Server
        /// </summary>
        public static void Start()
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
        private static void ListenerCallBack(IAsyncResult result)
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
        public static bool IsDisposed
        {
            get
            {
                return m_ListenerIsDisposed;
            }
        }

        /// <summary>
        /// Stops the Server
        /// </summary>
        public static void Stop()
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

