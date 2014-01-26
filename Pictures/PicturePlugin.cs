using System;
using REST_Server;
using System.Xml;

[assembly:PluginInitType(typeof(Pictures.PicturePlugin))]

namespace Pictures
{
	public class PicturePlugin : PluginBase
	{
		public PicturePlugin (XmlNode settings, Server server) : base (settings, server)
		{

		}
	}
}

