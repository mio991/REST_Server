using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mio991.REST.Server.Plugins;
using mio991.REST.Server;
using System.Data;

[assembly:PluginInitType(typeof(mio991.REST.Plugins.UsersAndRights.UserAndRightsPlugin))]

/*
SELECT * FROM user
LEFT JOIN object_user
ON user.id = object_user.object_id
WHERE object_user.rights & 'READ'
 */

namespace mio991.REST.Plugins.UsersAndRights
{
    public class UserAndRightsPlugin : PluginBase
    {
        private static string m_UserTable, m_UserObjectTable;

        public static string UserTable
        {
            get
            {
                return m_UserTable;
            }
        }
        public static string UserObjectTable
        {
            get
            {
                return m_UserObjectTable;
            }
        }

        public UserAndRightsPlugin(System.Xml.XmlNode settings, Server.Server server)
            : base(settings, server)
        {
            m_UserTable = m_Settings["user"];
            m_UserObjectTable = m_Settings["objectUser"];
            m_Server.SessionGenerated += m_Server_SessionGenerated;
        }

        void m_Server_SessionGenerated(object sender, SessionGenaretedEventArgs e)
        {
            e.NewSessionVariables.Add("user", User.Guest);
        }

        public bool UserHasRights(User user, Right right, RightObject obj)
        {
            bool result;
            lock (m_Server.DBConnection)
            {
                m_Server.DBConnection.Open();
                IDbCommand command = m_Server.DBConnection.CreateCommand();
                command.CommandText = String.Format("SELECT * FROM object_user WHERE rights & '{0}' AND user_id = {1} AND objecttable = '{2}' AND object_id = {3}", right.ID, user.ID, obj.Table, obj.ID);
                IDataReader reader = command.ExecuteReader();
                result = reader.RecordsAffected == 1;
                m_Server.DBConnection.Close();
            }
            return result;
        }

        public void GrantRightToUser(User user, Right right, RightObject obj)
        {
            lock (m_Server.DBConnection)
            {
                m_Server.DBConnection.Open();
                IDbCommand command = m_Server.DBConnection.CreateCommand();
                command.CommandText = String.Format("INSERT INTO `rest`.`object_user` (`Object_ID`, `User_ID`, `Object_Table`, `Rights`) VALUES ('{0}', '{1}', '{2}', '{3}') ON DUPLICATE KEY UPDATE `Rights` = VALUES(`Rights`)",obj.ID, user.ID, obj.Table, right.ID);
                command.ExecuteNonQuery();
                m_Server.DBConnection.Close();
            }
        }
    }
}
