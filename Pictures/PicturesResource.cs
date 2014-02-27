using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REST_Server;
using System.Net;
using System.IO;
using REST_Server.Resource;
using UsersAndRights;

namespace Pictures
{
    class PicturesResource : Resource
    {
        private string m_PictureDirectory;
        private Server m_Server;
        private UserAndRightsPlugin m_UserPlugin;

        /// <summary>
        /// Creates a new PictureResource Instance with the Directory wich contains the Pictures
        /// </summary>
        /// <param name="pDirectory">The Diretory that contains the pictures</param>
        public PicturesResource(Server server, string pDirectory, UserAndRightsPlugin plugin)
            : base("Pictues")
        {
            m_PictureDirectory = pDirectory;
            m_Server = server;
            m_UserPlugin = plugin;
        }

        /// <summary>
        /// Return a List of Pics or the Requested Picture
        /// </summary>
        /// <param name="uri">the requested Resource (Picture)</param>
        /// <param name="context">The HTTP-Context of the Request</param>
        public override void Pull(URI uri, HttpListenerContext context)
        {
            Dictionary<string, object> sessionVariables = m_Server.GetSessionVariables(context);

            if (uri.IsEnded)
            {
                var builder = new StringBuilder();

                builder.Append("{ 'Type':'ResourceCollection'; 'SubResources' : [");
                foreach(Picture pic in Picture.GetPictures(m_Server, (User)m_Server.GetSessionVariables(context)["user"]))
                {
                        builder.Append("{ 'ID' : '");
                        builder.Append(pic.ID);
                        builder.Append("' ; 'Name' : '");
                        builder.Append(pic.Name);
                        builder.Append("'}");
                }
                builder.Append("]}");

                Resource.WriteOut(context, builder.ToString());
            }
            else
            {
                Picture pic = new Picture(uri.GetSegment(), m_Server, m_PictureDirectory);
                if (m_UserPlugin.UserHasRights((User)m_Server.GetSessionVariables(context)["user"], Right.READ, pic))
                {
                    context.Response.ContentType = pic.MimeType;
                    pic.WriteFileToStream(context.Response.OutputStream);

                }
                else
                {
                    throw RESTProcessException.NotEnoughRights;
                }
            }
        }
    }
}
