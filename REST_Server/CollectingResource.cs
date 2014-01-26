using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections;

namespace REST_Server
{
	public class CollectingResource : Resource, ICollectingResource
	{
		private Dictionary<string, Resource> m_Childs = new Dictionary<string, Resource>();
		private Server m_Server;

		public CollectingResource (Server server, string name) : base (name){
			m_Server = server;
		}

		public void Forward (URI uri, HttpListenerContext context)
		{
			if (m_Childs.ContainsKey (uri.GetSegment ())) {
				Resource child = m_Childs [uri.GetSegment ()];
				uri.Next ();

				if (uri.IsEnded) {
					child.WriteResource (context);
				} else if (child is ICollectingResource) {
					((ICollectingResource)child).Forward (uri, context);
				} else {
					throw new RESTProcessException ("Resouce Not Found", 404);
				}
			}	else {
				throw new RESTProcessException ("Resouce Not Found", 404);
			}
		}

		public override void WriteResource (HttpListenerContext context)
		{
			StringBuilder builder = new StringBuilder ();
			builder.Append ("{ 'Type':'ResourceCollection'; 'SubResources' : [");
			foreach (string key in m_Childs.Keys) {
				builder.Append ("{ 'ID' : '");
				builder.Append (key);
				builder.Append ("' ; 'Name' : '");
				builder.Append (m_Childs[key].Name);
				builder.Append ("'}");
			}
			builder.Append ("]}");

			Resource.WriteOut (context, builder.ToString ());
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

	public interface ICollectingResource : IResource
	{
		void Forward(URI uri, HttpListenerContext context);
		void Add (string id, Resource res);
		void Remove(string id);
	}
}

