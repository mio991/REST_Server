using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mio991.REST.Plugins.UsersAndRights
{
    // maybe sometimes usefull - INSERT INTO `rest`.`user` (`ID`, `Name`, `Pass`, `Salt`) VALUES ('{0}', '{1}', '{2}', 'fsdfsdf') ON DUPLICATE KEY UPDATE `Name` = VALUES(`Name`), `Pass`=VALUES(`Pass`), `Salt` = VALUES(`Salt`)
    public class User : RightObject
    {
        public User(string id)
            : base(UserAndRightsPlugin.UserTable, id)
        {
            
        }

        public static readonly User Guest = new User("0000-0000-0000-0000");
    }
}
