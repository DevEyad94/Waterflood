using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEndWaterFloodApp.Models.zsk;

namespace BackEndWaterFloodApp.Models
{
    public class UserRole
    {
        public int UserRoleID { get; set; }
        public User User { get; set; } = null!;
        public int UserID { get; set; }
        public zRole zRole { get; set; } = null!;
        public int zRoleId { get; set; }
    }
}
