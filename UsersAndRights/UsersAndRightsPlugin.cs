using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REST_Server.Plugins;
using REST_Server;
using System.Data;

[assembly:PluginInitType(typeof(UsersAndRights.UserAndRightsPlugin))]

/*
SELECT * FROM user
LEFT JOIN object_user
ON user.id = object_user.object_id
WHERE object_user.rights & 'READ'
 */

namespace UsersAndRights
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

        public UserAndRightsPlugin(System.Xml.XmlNode settings, Server server)
            : base(settings, server)
        {
            m_UserTable = m_Settings["user"];
            m_UserObjectTable = m_Settings["objectUser"];
        }

        public bool UserHasRights(User user, Right right, RightObject obj)
        {
            IDbCommand command = m_Server.DBConnection.CreateCommand();
            command.CommandText = String.Format("SELECT * FROM object_user WHERE rights & '{0}' AND user_id = {1} AND objecttable = '{2}' AND object_id = {3}", right.ID, user.ID, obj.Table, obj.ID);
            IDataReader reader = command.ExecuteReader();
            return reader.RecordsAffected == 1;
        }
    }
}
