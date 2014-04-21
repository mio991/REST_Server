using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace mio991.REST.Server.Plugins
{
	public abstract class PluginBase : IDisposable
	{
		protected Dictionary<string, string> m_Settings;

		public PluginBase (XmlNode settings)
		{
			m_Settings = new Dictionary<string, string>();
			foreach (XmlNode setting in settings.ChildNodes) {
				if (setting.Name == "setting") {
					m_Settings.Add (setting.Attributes ["name"].Value, setting.Attributes ["value"].Value);
				}
			}
		}

        //public abstract void Init

		public virtual void Dispose()
		{

		}
	}
}

