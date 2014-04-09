using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mio991.REST.Server
{
    public class SessionGenaretedEventArgs : EventArgs
    {
        private Dictionary<string, object> m_NewSessionVariables;

        public SessionGenaretedEventArgs(Dictionary<string, object> newSessionVariables)
        {
            m_NewSessionVariables = newSessionVariables;
        }

        public Dictionary<string, object> NewSessionVariables
        {
            get
            {
                return m_NewSessionVariables;
            }
        }
    }
}
