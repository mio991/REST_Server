using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UsersAndRights;
using REST_Server;
using System.IO;
using System.Data;

namespace Pictures
{
    class Picture : RightObject
    {
        private Server m_Server;
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
            FileStream picStream = new FileStream(m_FileDirectory + ID, FileMode.Open);
            picStream.CopyTo(outStream);
            picStream.Close();
        }

        public static Picture[] GetPictures(Server server, User user)
        {
            Picture[] result;
            lock (server.DBConnection)
            {
                server.DBConnection.Open();
                IDbCommand command = server.DBConnection.CreateCommand();
                command.CommandText = String.Format("SELECT pictures.* FROM pictures LEFT JOIN object_user ON pictures.id = object_user.object_id WHERE object_user.user_id = '{0}'", user.ID);
                IDataReader reader = command.ExecuteReader();
                result = new Picture[reader.RecordsAffected];
                int i = 0;
                while (reader.Read())
                {
                    result[i] = new Picture(reader);
                }
                reader.Close();
                server.DBConnection.Close();
            }
            return result;
        }

        private void LoadFromReader(IDataReader reader)
        {
            m_Name = reader.GetString(reader.GetOrdinal("Name"));
            m_MimeType = reader.GetString(reader.GetOrdinal("mimeType"));
        }

        private Picture(IDataReader reader)
            : base(reader, "pictures")
        {
            LoadFromReader(reader);
        }

        public Picture(string id, Server server, string fileDiretory)
            : base("pictures", id)
        {
            m_Server = server;
            m_FileDirectory = fileDiretory;
            lock (m_Server.DBConnection)
            {
                m_Server.DBConnection.Open();
                IDbCommand command = m_Server.DBConnection.CreateCommand();
                command.CommandText = String.Format("SELECT * FROM files WHERE ID = '{0}'", ID);
                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    LoadFromReader(reader);
                }
                reader.Close();
                m_Server.DBConnection.Close();
            }
        }
    }
}
