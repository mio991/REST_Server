using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mio991.REST.Plugins.UsersAndRights;
using mio991.REST.Server;
using System.IO;
using System.Data;

namespace mio991.REST.Plugins.Files
{
    class File : RightObject
    {
        private Server.Server m_Server;
        private string m_Name;
        private string m_FileDirectory;
        private string m_MimeType;

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        public string MimeType
        {
            get
            {
                return m_MimeType;
            }
        }

        public void WriteFileToStream(Stream outStream)
        {
            string path = m_FileDirectory + ID;
            if (System.IO.File.Exists(path))
            {
                FileStream picStream = new FileStream(path, FileMode.Open);
                picStream.CopyTo(outStream);
                picStream.Close();
            }
        }

        public static File Create(Server.Server server, User user, string name, string mime)
        {
            File result = null;
            lock (server.DBConnection)
            {
                server.DBConnection.Open();
                IDbCommand command = server.DBConnection.CreateCommand();
                command.CommandText = String.Format("INSERT INTO `rest`.`mio991.REST.Plugins.Files` (`ID`, `Name`, `mimeType`) VALUES ('{0}', '{1}', '{2}');", Guid.NewGuid(), name, mime);
                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result = new File(reader);
                }
                reader.Close();
                server.DBConnection.Close();
            }
            return result;
        }

        public static File[] GetFile(Server.Server server, User user)
        {
            List<File> result = new List<File>();
            lock (server.DBConnection)
            {
                server.DBConnection.Open();
                IDbCommand command = server.DBConnection.CreateCommand();
                command.CommandText = String.Format("SELECT mio991.REST.Plugins.Files.* FROM mio991.REST.Plugins.Files LEFT JOIN object_user ON mio991.REST.Plugins.Files.id = object_user.object_id WHERE object_user.user_id = '{0}'", user.ID);
                IDataReader reader = command.ExecuteReader();
                int i = 0;
                while (reader.Read())
                {
                    result.Add(new File(reader));
                }
                reader.Close();
                server.DBConnection.Close();
            }
            return result.ToArray();
        }

        private void LoadFromReader(IDataReader reader)
        {
            m_Name = reader.GetString(reader.GetOrdinal("Name"));
            m_MimeType = reader.GetString(reader.GetOrdinal("mimeType"));
        }

        private File(IDataReader reader)
            : base(reader, "mio991.REST.Plugins.Files")
        {
            LoadFromReader(reader);
        }

        public void Update(string name, string mime, string picData)
        {
            lock (m_Server.DBConnection)
            {
                m_Server.DBConnection.Open();
                IDbCommand command = m_Server.DBConnection.CreateCommand();
                command.CommandText = String.Format("INSERT INTO `rest`.`mio991.REST.Plugins.Files` (`ID`, `Name`, `mimeType`) VALUES ('{0}', '{1}', '{2}') ON DUPLICATE KEY UPDATE `Name` = VALUES(`Name`), `mimeType` = VALUES(`mimeType`)", ID, name,mime);
                command.ExecuteNonQuery();
                m_Server.DBConnection.Close();
            }

            m_Name = name;
            m_MimeType = mime;
            byte[] buffer = Convert.FromBase64String(picData);
            FileStream pic = new FileStream(m_FileDirectory + ID, FileMode.Create);
            pic.Write(buffer, 0, buffer.Length);
        }

        public File(string id, Server.Server server, string fileDiretory)
            : base("mio991.REST.Plugins.Files", id)
        {
            m_Server = server;
            m_FileDirectory = fileDiretory;
            lock (m_Server.DBConnection)
            {
                m_Server.DBConnection.Open();
                IDbCommand command = m_Server.DBConnection.CreateCommand();
                command.CommandText = String.Format("SELECT * FROM files WHERE ID = '{0}'", ID);
                IDataReader reader = command.ExecuteReader();
                if (reader.RecordsAffected == 1)
                {
                    while (reader.Read())
                    {
                        LoadFromReader(reader);
                    }
                }
                else
                {
                    throw new ArgumentException("Resource not found");
                }
                reader.Close();
                m_Server.DBConnection.Close();
            }
        }
    }
}
