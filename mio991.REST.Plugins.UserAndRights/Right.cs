using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mio991.REST.Plugins.UsersAndRights
{
    public class Right
    {
        private string[] m_IDs;

        public string ID
        {
            get
            {
                return GetJoined();
            }
        }

        public Right(string id)
        {
            m_IDs = Seperate(id);
        }

        public override bool Equals(object obj)
        {
            if (obj is Right)
            {
                Right r2 = (Right)obj;
                return this.Compare(r2) | r2.Compare(this);
            }
            return false;
        }

        private bool Compare(Right r2)
        {
            bool result = true;
            foreach (string id in r2.m_IDs)
            {
                if (!m_IDs.Contains(id))
                {
                    result = false;
                }
            }
            return result;
        }

        private string GetJoined()
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;
            while (i < m_IDs.Length - 1)
            {
                builder.Append(m_IDs[i]);
                builder.Append(", ");
                i++;
            }
            builder.Append(m_IDs[i]);
            return builder.ToString();
        }

        private string[] Seperate(string input)
        {
            return input.Split(',', ' ');
        }

        #region static

        public static readonly Right READ = new Right("READ");
        public static readonly Right WRITE = new Right("WRITE");

        #endregion

        public static Right operator |(Right r1, Right r2)
        {
            return new Right(r1.ID + ", " + r2.ID);
        }

        public static bool operator ==(Right r1,Right r2)
        {
            return r1.Equals(r2);
        }

        public static bool operator !=(Right r1, Right r2)
        {
            return !(r1 == r2);
        }
    }
}
