using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mio991.REST.Server;
using System.Net;
using System.IO;
using mio991.REST.Server.Resource;
using mio991.REST.Plugins.UsersAndRights;

namespace mio991.REST.Plugins.Files
{
    class FileResource : Resource
    {
        private string m_PictureDirectory;
        private UserAndRightsPlugin m_UserPlugin;

        /// <summary>
        /// Creates a new PictureResource Instance with the Directory wich contains the mio991.REST.Plugins.Files
        /// </summary>
        /// <param name="pDirectory">The Diretory that contains the mio991.REST.Plugins.Files</param>
        public FileResource(string id,string pDirectory, UserAndRightsPlugin plugin)
            : base(id)
        {
            m_PictureDirectory = pDirectory;
            m_UserPlugin = plugin;
        }

        /// <summary>
        /// Return a List of Pics or the Requested Picture
        /// </summary>
        /// <param name="uri">the requested Resource (Picture)</param>
        /// <param name="context">The HTTP-Context of the Request</param>
        public override void Get(URI uri, HttpListenerContext context)
        {
            Dictionary<string, object> sessionVariables = Server.Server.GetSessionVariables(context);

            if (uri.IsEnded)
            {
                var builder = new StringBuilder();

                builder.Append("{ 'Type':'ResourceCollection'; 'SubResources' : [");                
                foreach(File file in File.GetFile((User)sessionVariables["user"]))
                {
                        builder.Append("{ 'ID' : '");
                        builder.Append(file.ID);
                        builder.Append("' ; 'Name' : '");
                        builder.Append(file.Name);
                        builder.Append("'}");
                }
                builder.Append("]}");

                Resource.WriteOut(context, builder.ToString());
            }
            else
            {
                File pic = new File(uri.GetSegment(), m_PictureDirectory);
                if (m_UserPlugin.UserHasRights((User)Server.Server.GetSessionVariables(context)["user"], Right.READ, pic))
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

        public override void Post(URI uri, HttpListenerContext context)
        {
            
        }

        public override void Put(URI uri, HttpListenerContext context)
        {
            if (uri.IsEnded)
            {
                throw RESTProcessException.MethodeNotAllowed;
            }
            else
            {
                uri.Next();
                User user = (User)Server.Server.GetSessionVariables(context)["user"];
                Dictionary<string, string> posts = Server.Server.GetPostVariables(context);
                File pic;
                try
                {
                    pic = new File(uri.GetSegment(), m_PictureDirectory);
                }
                catch (ArgumentException ex)
                {
                    pic = File.Create(user, posts["name"], posts["mime"]);
                    m_UserPlugin.GrantRightToUser(user, Right.WRITE | Right.READ, pic);
                }
                if (m_UserPlugin.UserHasRights(user, Right.WRITE, pic))
                {

                }

            }
        }

        public override void Delete(URI uri, HttpListenerContext contect)
        {
            throw new NotImplementedException();
        }
    }
}
