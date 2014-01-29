using System;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace REST_Server
{
	public class RequestHandler
	{
		private Server m_Server;
		private List<Thread> m_Threads;
		private ParameterizedThreadStart m_Start;
		private Encoding m_Encoding;

		public RequestHandler (Server server, Encoding enco)
		{
			m_Server = server;
			m_Threads = new List<Thread> ();
			m_Start = new ParameterizedThreadStart (PrivateHandleRequest);
			m_Encoding = enco;
		}

		private void PrivateHandleRequest (object o)
		{
			try {
				HttpListenerContext context = (HttpListenerContext)o;
				try {
					m_Server.Log.Info (String.Format ("Begin Handle Request from {0} for {1}", context.Request.RemoteEndPoint, context.Request.RawUrl));
					context.Response.ContentEncoding = m_Encoding;
					URI uri = new URI (context.Request.RawUrl);
				    m_Server.RootResource.Pull (uri, context);
				} catch (RESTProcessException ex) {
					m_Server.Log.Error (ex);
					context.Response.StatusCode = ex.ErrorCode;
					context.Response.StatusDescription = ex.Message;
				}
				context.Response.Close ();
				m_Server.Log.Info (String.Format ("End Handle Request from {0} for {1}", context.Request.RemoteEndPoint, context.Request.RawUrl));
			} catch (Exception ex) {
				m_Server.Log.Error (ex);
			}
		}

		public void HandleRequest (HttpListenerContext context)
		{
			var t = new Thread (m_Start);
			t.Start (context);
		}
	}
}

