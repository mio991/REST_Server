using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersAndRights
{
    public class User : RightObject
    {
        public User(string id)
            : base(UserAndRightsPlugin.UserTable, id)
        {

        }
    }
}
