using System;

namespace mio991.REST.Server.Plugins
{
    /// <summary>
    /// Atribute to Mark the Plugin-Class of an Assembly
    /// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	public class PluginInitTypeAttribute : Attribute
	{
		private Type m_InitType;

		public Type InitType
		{
			get {
				return m_InitType;
			}
		}

		public PluginInitTypeAttribute (Type initType)
		{
			if (typeof(PluginBase).IsAssignableFrom (initType)) {
				m_InitType = initType;
			} else {
				throw new ArgumentException (String.Format("{0} is not inherited from 'PluginBase'", initType));
			}
		}
	}
}

