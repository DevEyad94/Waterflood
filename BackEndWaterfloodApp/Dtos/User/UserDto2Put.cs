using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEndWaterFloodApp.Dtos.User
{
    public class UserDto2Put
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public int? zRoleId { get; set; }
    }
}
