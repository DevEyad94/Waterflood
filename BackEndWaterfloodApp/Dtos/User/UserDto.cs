using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BackEndWaterFloodApp.Dtos.User
{
    public class UserDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<RoleDto> Roles { get; set; } = new List<RoleDto>();
        public ICollection<string> Role { get; set; } = new List<string>();
        public string Token { get; set; } = string.Empty;
    }
}
