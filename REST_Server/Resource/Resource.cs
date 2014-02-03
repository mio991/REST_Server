using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace REST_Server.Resource
{
	public abstract class Resource : IResource
	{
        public abstract void Pull(URI uri, HttpListenerContext context);

        private string m_Name;

        /// <summary>
        /// Create a new Instance of Resource
        /// </summary>
        /// <param name="name">The name of the Resource</param>
        public Resource(string name)
        {
            m_Name = name;
        }

		#region static

        /// <summary>
        /// A Methode to write a String in the Output
        /// </summary>
        /// <param name="context">the HTTP-context withe the output-Stream</param>
        /// <param name="inString">The String to Write into the Output</param>
        protected static void WriteOut(HttpListenerContext context, string inString)
        {
            byte[] buffer = context.Response.ContentEncoding.GetBytes(inString);
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }

		#endregion

        /// <summary>
        /// The Name of the Resource
        /// </summary>
        public string Name
        {
            get 
            {
                return m_Name;
            }
        }
    }
    
    /// <summary>
    /// An Interface defining Resources
    /// </summary>
	public interface IResource
	{
        /// <summary>
        /// The Name of the Resource
        /// </summary>
        string Name { get; }

        /// <summary>
        /// This Methode will called if the HTTP-Methode is 'PULL'
        /// </summary>
        /// <param name="uri">The requested Resource</param>
        /// <param name="context">The HTTP-Context of the request</param>
		void Pull (URI uri, HttpListenerContext context);
	}
}

