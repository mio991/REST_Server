using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace REST_Server
{
	public abstract class PluginBase : IDisposable
	{
		protected Dictionary<string, string> m_Settings;
		protected Server m_Server;

		public PluginBase (XmlNode settings, Server server)
		{
			m_Settings = new Dictionary<string, string>();
			m_Server = server;
			foreach (XmlNode setting in settings.ChildNodes) {
				if (setting.Name == "setting") {
					m_Settings.Add (setting.Attributes ["name"].Value, setting.Attributes ["value"].Value);
				}
			}
		}

		public virtual void Dispose()
		{

		}
	}
}

