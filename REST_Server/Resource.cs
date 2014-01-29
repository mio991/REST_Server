using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace REST_Server
{
	public abstract class Resource : IResource
	{
        public abstract void Pull(URI uri, HttpListenerContext context);

        private string m_Name;

        public Resource(string name)
        {
            m_Name = name;
        }

		#region static

        protected static void WriteOut(HttpListenerContext context, string inString)
        {
            byte[] buffer = context.Response.ContentEncoding.GetBytes(inString);
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }

		#endregion

        public string Name
        {
            get 
            {
                return m_Name;
            }
        }
    }

	public interface IResource
	{
        string Name { get; }
		void Pull (URI uri, HttpListenerContext context);
	}
}

