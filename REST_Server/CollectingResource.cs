using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections;

namespace REST_Server
{
	public class CollectingResource : Resource
	{
		private Dictionary<string, Resource> m_Childs = new Dictionary<string, Resource>();
		private Server m_Server;

		public CollectingResource (Server server, string name) : base(name){
			m_Server = server;
		}

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
                    throw RESTProcessException.ResorceNotFound;
                }
            }
		}


		public void Add (string id, Resource res)
		{
			m_Childs.Add (id, res);
		}

		public void Remove(string id)
		{
			m_Childs.Remove (id);
		}
	}
}

