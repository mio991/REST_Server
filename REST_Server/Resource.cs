using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace REST_Server
{
	public abstract class Resource : IResource
	{
		private string m_Name;
		
		public string Name {
			get {
				return m_Name;
			}
		}

		public Resource(string name)
		{
			m_Name = name;
		}

		public abstract void WriteResource (HttpListenerContext context);

		#region static

		protected static void WriteOut(HttpListenerContext context, string inString)
		{
			byte[] buffer = context.Response.ContentEncoding.GetBytes (inString);
			context.Response.OutputStream.Write (buffer, 0, buffer.Length);
		}

		#endregion
	}

	public interface IResource
	{
		void WriteResource (HttpListenerContext context);
	}
}

