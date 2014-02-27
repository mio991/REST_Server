using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections;

namespace REST_Server.Resource
{
	public class CollectingResource : Resource
	{
		private Dictionary<string, Resource> m_Childs = new Dictionary<string, Resource>();
		private Server m_Server;

        /// <summary>
        /// Creates a new Instance of the Collecting Resource
        /// </summary>
        /// <param name="server">The Server holding the Resource</param>
        /// <param name="name">The Name of the Resource</param>
		public CollectingResource (Server server, string name) : base(name){
			m_Server = server;
		}

        /// <summary>
        /// This Methode will called if the HTTP-Methode is 'PULL'
        /// </summary>
        /// <param name="uri">The requested Resource</param>
        /// <param name="context">The HTTP-Context of the request</param>
        public override void Pull(URI uri, HttpListenerContext context)
		{
            if (uri.IsEnded)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("{ 'Type':'ResourceCollection'; 'SubResources' : [");
                foreach (string key in m_Childs.Keys)
                {
                    builder.Append("{ 'ID' : '");
                    builder.Append(key);
                    builder.Append("' ; 'Name' : '");
                    builder.Append(m_Childs[key].Name);
                    builder.Append("'}");
                }
                builder.Append("]}");

                Resource.WriteOut(context, builder.ToString());
            }
            else
            {
                if (m_Childs.ContainsKey(uri.GetSegment()))
                {
                    var res = m_Childs[uri.GetSegment()];
                    uri.Next();
                    res.Pull(uri, context);
                }
                else
                {
                    throw RESTProcessException.ResourceNotFound;
                }
            }
		}

        /// <summary>
        /// Adds a Resource to the Collegtion
        /// </summary>
        /// <param name="id">the id of the Resource</param>
        /// <param name="res">The Resource it self</param>
		public void Add (string id, Resource res)
		{
			m_Childs.Add (id, res);
		}

        /// <summary>
        /// Removes a Resource from the Collection
        /// </summary>
        /// <param name="id">The id of the Resource to remove</param>
		public void Remove(string id)
		{
			m_Childs.Remove (id);
		}
	}
}

