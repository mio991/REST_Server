using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REST_Server;
using System.Net;
using System.IO;

namespace Pictures
{
    class PicturesResource : Resource
    {
        string m_PictureDirectory;

        public PicturesResource(string pDirectory) : base("Pictues")
        {
            m_PictureDirectory = pDirectory;
        }

        private bool HasRights(string id)
        {
            return true;
        }

        public override void Pull(URI uri, HttpListenerContext context)
        {
            if (uri.IsEnded)
            {
                var builder = new StringBuilder();

                builder.Append("{ 'Type':'ResourceCollection'; 'SubResources' : [");
                foreach (string pic in Directory.GetFiles(m_PictureDirectory))
                {
                    //TODO Adding right check
                    builder.Append("{ 'ID' : '");
                    builder.Append(Path.GetFileName(pic));
                    builder.Append("' ; 'Name' : '");
                    builder.Append(pic); // TODO Adding geting name from the DB
                    builder.Append("'}");
                }
                builder.Append("]}");

                Resource.WriteOut(context, builder.ToString());
            }
            else
            {
                // TODO Adding right check
                var requestedPic = m_PictureDirectory + uri.GetSegment();

                if (!File.Exists(requestedPic))
                {
                    throw RESTProcessException.ResorceNotFound;
                }

                var fStream = File.Open(requestedPic, FileMode.Open);

                fStream.CopyTo(context.Response.OutputStream);
            }
        }
    }
}
