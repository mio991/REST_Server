using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REST_Server;
using System.Net;
using System.IO;
using REST_Server.Resource;

namespace Pictures
{
    class PicturesResource : Resource
    {
        string m_PictureDirectory;
        Server m_Server;

        /// <summary>
        /// Creates a new PictureResource Instance with the Directory wich contains the Pictures
        /// </summary>
        /// <param name="pDirectory">The Diretory that contains the pictures</param>
        public PicturesResource(Server server, string pDirectory)
            : base("Pictues")
        {
            m_PictureDirectory = pDirectory;
            m_Server = server;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool HasRights(string id)
        {
            return true; //TODO Adding Right-Check
        }

        /// <summary>
        /// Return a List of Pics or the Requested Picture
        /// </summary>
        /// <param name="uri">the requested Resource (Picture)</param>
        /// <param name="context">The HTTP-Context of the Request</param>
        public override void Pull(URI uri, HttpListenerContext context)
        {
            if (uri.IsEnded)
            {
                var builder = new StringBuilder();

                builder.Append("{ 'Type':'ResourceCollection'; 'SubResources' : [");
                foreach (string pic in Directory.GetFiles(m_PictureDirectory))
                {
                    string id = Path.GetFileName(pic);
                    if (HasRights(id))
                    {
                        builder.Append("{ 'ID' : '");
                        builder.Append(id);
                        builder.Append("' ; 'Name' : '");
                        builder.Append(pic); // TODO Adding geting name from the DB
                        builder.Append("'}");
                    }
                }
                builder.Append("]}");

                Resource.WriteOut(context, builder.ToString());
            }
            else
            {
                if (HasRights(uri.GetSegment()))
                {
                    var requestedPic = m_PictureDirectory + uri.GetSegment();

                    if (!File.Exists(requestedPic))
                    {
                        throw RESTProcessException.ResorceNotFound;
                    }

                    var fStream = File.Open(requestedPic, FileMode.Open);

                    fStream.CopyTo(context.Response.OutputStream);

                    fStream.Close();
                }
                else
                {
                    throw RESTProcessException.NotEnoughRights;
                }
            }
        }
    }
}
