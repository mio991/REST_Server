using System;
using mio991.REST.Server;
using System.Xml;
using System.IO;
using mio991.REST.Server.Resource;
using mio991.REST.Server.Plugins;

[assembly: PluginInitType(typeof(mio991.REST.Plugins.Files.FilePlugin))]

namespace mio991.REST.Plugins.Files
{
    public class FilePlugin : PluginBase
    {
        FileResource m_Resource;

        public FilePlugin(XmlNode settings)
            : base(settings)
        {
            if (!Path.IsPathRooted(m_Settings["saveDirectory"]))
            {
                m_Settings["saveDirectory"] = Server.Server.WorkingDirectory + m_Settings["saveDirectory"];
            }

            if (!Directory.Exists(m_Settings["saveDirectory"]))
            {
                Directory.CreateDirectory(m_Settings["saveDirectory"]);
            }

            m_Resource = new FileResource(m_Settings["ResName"], m_Settings["saveDirectory"], (mio991.REST.Plugins.UsersAndRights.UserAndRightsPlugin)Server.Server.Plugins[m_Settings["UsersAndRightsPlugin"]]);

            Server.Server.RootResource.Add(m_Settings["ResID"], m_Resource);
        }
    }
}

