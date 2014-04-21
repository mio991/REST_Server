using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections;

namespace mio991.REST.Server.Resource
{
	public class CollectingResource : Resource
	{
		private Dictionary<string, Resource> m_Childs = new Dictionary<string, Resource>();

        /// <summary>
        /// Creates a new Instance of the Collecting Resource
        /// </summary>
        /// <param name="name">The Name of the Resource</param>
		public CollectingResource (string name) : base(name){
		}

        private bool TryForward(URI uri, HttpListenerContext context, RequestType type)
        {
            if (uri.IsEnded)
            {
                return false;
            }
            else
            {
                if (m_Childs.ContainsKey(uri.GetSegment()))
                {
                    var res = m_Childs[uri.GetSegment()];
                    uri.Next();
                    switch (type)
                    {
                        case RequestType.Get:
                            res.Get(uri, context);
                            break;
                        case RequestType.Post:
                            res.Post(uri, context);
                            break;
                        case RequestType.Put:
                            res.Put(uri, context);
                            break;
                        case RequestType.Delete:
                            res.Delete(uri, context);
                            break;
                    }
                }
                else
                {
                    throw RESTProcessException.ResourceNotFound;
                }
                return true;
            }
        }

        /// <summary>
        /// This Methode will called if the HTTP-Methode is 'PULL'
        /// </summary>
        /// <param name="uri">The requested Resource</param>
        /// <param name="context">The HTTP-Context of the request</param>
        public override void Get(URI uri, HttpListenerContext context)
		{
            if (!TryForward(uri,context,RequestType.Get))
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

        public override void Post(URI uri, HttpListenerContext context)
        {
            throw new NotImplementedException(); //TODO Implement
        }

        public override void Put(URI uri, HttpListenerContext context)
        {
            throw new NotImplementedException();
        }

        public override void Delete(URI uri, HttpListenerContext contect)
        {
            throw new NotImplementedException();
        }

        private enum RequestType
        {
            Get,
            Post,
            Put,
            Delete
        }
    }
}

