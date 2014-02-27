using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace UsersAndRights
{
    public abstract class RightObject
    {
        private string m_ID;
        private string m_Table;

        public string Table
        {
            get
            {
                return m_Table;
            }
        }

        public string ID
        {
            get
            {
                return m_ID;
            }
        }

        public RightObject(string table, string id)
        {
            m_ID = id;
            m_Table = table;
        }

        protected RightObject(IDataReader reader, string table)
        {
            m_ID = reader.GetString(reader.GetOrdinal("ID"));
        }
    }
}
