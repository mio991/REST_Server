using System;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace mio991.REST.Server
{
    public class RequestHandler
    {
        private List<Thread> m_Threads;
        private ParameterizedThreadStart m_Start;
        private Encoding m_Encoding;

        public RequestHandler(Encoding enco)
        {
            m_Threads = new List<Thread>();
            m_Start = new ParameterizedThreadStart(PrivateHandleRequest);
            m_Encoding = enco;
        }

        private void PrivateHandleRequest(object o)
        {
            try
            {
                HttpListenerContext context = (HttpListenerContext)o;
                try
                {
                    Server.Log.Info(String.Format("Begin Handle Request from {0} for {1}", context.Request.RemoteEndPoint, context.Request.RawUrl));
                    context.Response.ContentEncoding = m_Encoding;
                    URI uri = new URI(context.Request.RawUrl);
                    switch (context.Request.HttpMethod)
                    {
                        default:
                        case "GET":
                            Server.RootResource.Get(uri, context);
                            break;
                        case "POST":
                            Server.RootResource.Post(uri, context);
                            break;
                        case "Put":
                            Server.RootResource.Put(uri, context);
                            break;
                    }
                }
                catch (RESTProcessException ex)
                {
                    Server.Log.Error(ex);
                    context.Response.StatusCode = ex.ErrorCode;
                    context.Response.StatusDescription = ex.Message;
                }
                context.Response.Close();
                Server.Log.Info(String.Format("End Handle Request from {0} for {1}", context.Request.RemoteEndPoint, context.Request.RawUrl));
            }
            catch (Exception ex)
            {
                Server.Log.Error(ex);
            }
            if (DoneRequestHandling != null)
            {
                DoneRequestHandling(this, new EventArgs());
            }
        }

        public event EventHandler DoneRequestHandling;

        public void HandleRequest(HttpListenerContext context)
        {
            var t = new Thread(m_Start);
            t.Start(context);
        }
    }
}

