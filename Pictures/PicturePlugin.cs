using System;
using REST_Server;
using System.Xml;
using System.IO;
using REST_Server.Resource;
using REST_Server.Plugins;

[assembly: PluginInitType(typeof(Pictures.PicturePlugin))]

namespace Pictures
{
    public class PicturePlugin : PluginBase
    {
        PicturesResource m_Resource;


        public PicturePlugin(XmlNode settings, Server server)
            : base(settings, server)
        {
            if (!Path.IsPathRooted(m_Settings["saveDirectory"]))
            {
                m_Settings["saveDirectory"] = m_Server.WorkingDirectory + m_Settings["saveDirectory"];
            }

            if (!Directory.Exists(m_Settings["saveDirectory"]))
            {
                Directory.CreateDirectory(m_Settings["saveDirectory"]);
            }

            m_Resource = new PicturesResource(m_Server, m_Settings["saveDirectory"], (UsersAndRights.UserAndRightsPlugin)m_Server.Plugins[m_Settings["UsersAndRightsPlugin"]]);

            m_Server.RootResource.Add("pictures", m_Resource);
        }
    }
}

