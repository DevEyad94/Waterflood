using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEndWaterFloodApp.Dtos.User;

namespace BackEndWaterFloodApp.Services.UserService
{
    public interface IUserService
    {
        Task<ServiceResponse<UserDto>> GetUser(string userName);
        Task<ServiceResponse<UserDto>> Login(string userName, string passwordHash);
        Task<ServiceResponse<UserDto2Put>> ModifyUser(UserDto2Put userDto2Put);

        Task<ServiceResponse<User>> NewUser(User2RegisterDto user2RegisterDto);

        Task<ServiceResponse<UserDto>> GetUserDataByToken();
        Task<ServiceResponse<List<UserDto>>> GetAllUsers();
        Task<ServiceResponse<List<RoleListDto>>> GetRoles();
        bool CheckUserByUserName(string userName);
        bool CheckUserByUserId(int id);
    }
}
