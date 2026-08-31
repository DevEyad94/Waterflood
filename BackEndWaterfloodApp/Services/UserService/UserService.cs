using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BackEndWaterFloodApp.Dtos.User;
using BackEndWaterFloodApp.Services.DatabaseService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;

namespace BackEndWaterFloodApp.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;
        private readonly WaterfloodDbContext context;
        private readonly IDatabaseService databaseService;

        public UserService(
            IConfiguration configuration,
            IMapper mapper,
            WaterfloodDbContext context,
            IDatabaseService databaseService,
            IHttpContextAccessor httpContextAccessor
        )
        {
            this.configuration = configuration;
            this.mapper = mapper;
            this.context = context;
            this.databaseService = databaseService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public bool CheckUserByUserId(int id)
        {
            if (!context.Users.Any(e => e.UserID == id))
                return false;
            return true;
        }

        public bool CheckUserByUserName(string userName)
        {
            if (!context.Users.Any(e => e.Username.ToLower() == userName.ToLower()))
                return false;
            return true;
        }

        public async Task<ServiceResponse<UserDto>> GetUser(string userName)
        {
            var serviceResponse = new ServiceResponse<UserDto>();
            // Console.WriteLine(userName);
            try
            {
                var user = await mapper
                    .ProjectTo<UserDto>(context.Users, mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(e => e.Username.ToLower() == userName.ToLower());

                if (user is null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "User not found!";
                    return serviceResponse;
                }
                // throw new Exception($"User not Found");
                var listOfRoles = new List<string>();

                foreach (var item in user.Roles)
                {
                    listOfRoles.Add(item.RoleName);
                }
                serviceResponse.Data = mapper.Map<UserDto>(user);
                user.Role = listOfRoles;
                return serviceResponse;
            }
            catch
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "User not found!";
                return serviceResponse;
                // return
            }
        }

        public async Task<ServiceResponse<UserDto>> GetUserDataByToken()
        {
            var serviceResponse = new ServiceResponse<UserDto>();
            try
            {
                var httpContext = httpContextAccessor.HttpContext;
                if (httpContext?.User.Identity?.IsAuthenticated != true)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "User is not authenticated.";
                    return serviceResponse;
                }

                var user = httpContext.User;
                var userName = user
                    .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                    ?.Value;
                if (string.IsNullOrWhiteSpace(userName))
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "User claim not found.";
                    return serviceResponse;
                }

                Console.WriteLine(userName);
                var data = await mapper
                    .ProjectTo<UserDto>(context.Users, mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(e => e.Username.ToLower() == userName.ToLower());
                if (data is null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Unauthenticated";
                    return serviceResponse;
                }

                var listOfRoles = new List<string>();

                foreach (var item in data.Roles)
                {
                    listOfRoles.Add(item.RoleName);
                }

                serviceResponse.Data = mapper.Map<UserDto>(data);
                data.Role = listOfRoles;
                return serviceResponse;
            }
            catch
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Unauthenticated";
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<UserDto>>> GetAllUsers()
        {
            var serviceResponse = new ServiceResponse<List<UserDto>>();
            var users = await mapper
                .ProjectTo<UserDto>(context.Users, mapper.ConfigurationProvider)
                .ToListAsync();

            foreach (var user in users)
            {
                user.Role = user.Roles.Select(r => r.RoleName).ToList();
            }

            serviceResponse.Data = users;
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<RoleListDto>>> GetRoles()
        {
            var roles = await context
                .zRoles.Select(r => new RoleListDto { ZRoleId = r.zRoleID, Name = r.Name })
                .ToListAsync();

            return new ServiceResponse<List<RoleListDto>> { Data = roles };
        }

        public async Task<ServiceResponse<UserDto>> Login(string userName, string passwordHash)
        {
            var serviceResponse = new ServiceResponse<UserDto>();
            Console.WriteLine(userName);
            try
            {
                var user = await mapper
                    .ProjectTo<UserDto>(context.Users, mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(e => e.Username.ToLower() == userName.ToLower());

                if (user is null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Wrong Username or Password!";
                    return serviceResponse;
                }
                if (!BCrypt.Net.BCrypt.Verify(passwordHash, user.PasswordHash))
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Wrong Username or Password!";
                    return serviceResponse;
                }

                // throw new Exception($"User not Found");
                var listOfRoles = new List<string>();

                foreach (var item in user.Roles)
                {
                    listOfRoles.Add(item.RoleName);
                }
                serviceResponse.Data = mapper.Map<UserDto>(user);
                user.Role = listOfRoles;
                return serviceResponse;
            }
            catch
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Wrong Username or Password!";
                return serviceResponse;
                // return
            }
        }

        public async Task<ServiceResponse<UserDto2Put>> ModifyUser(UserDto2Put userDto2Put)
        {
            var serviceResponse = new ServiceResponse<UserDto2Put>();

            // var user = await mapper.ProjectTo<User>(dataContext.Users, mapper.ConfigurationProvider)
            // .Where(e => e.UserID == userDto.UserID).FirstOrDefaultAsync();

            var existingEntity = await context
                .Users.Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserID == userDto2Put.UserID);

            if (existingEntity is null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "User not found.";
                return serviceResponse;
            }

            existingEntity.FullName = userDto2Put.FullName;
            if (!string.IsNullOrWhiteSpace(userDto2Put.PasswordHash))
            {
                existingEntity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto2Put.PasswordHash);
            }

            if (userDto2Put.zRoleId.HasValue)
            {
                context.UserRoles.RemoveRange(existingEntity.UserRoles);
                existingEntity.UserRoles.Add(
                    new UserRole { UserID = existingEntity.UserID, zRoleId = userDto2Put.zRoleId.Value }
                );
            }

            try
            {
                if (await databaseService.SaveAll())
                {
                    serviceResponse.Data = userDto2Put;
                    return serviceResponse;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.ToString();
                serviceResponse.Success = false;
                return serviceResponse;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<User>> NewUser(User2RegisterDto user2RegisterDto)
        {
            // context
            var serviceResponse = new ServiceResponse<User>();
            if (user2RegisterDto is null)
            {
                // throw new Exception("Something wrong happens while register!");
                serviceResponse.Message = "Something wrong happens while register!";
                serviceResponse.Success = false;
                return serviceResponse;
            }

            if (CheckUserByUserName(user2RegisterDto.Username.ToLower()))
            {
                serviceResponse.Message = "User Already exits!";
                serviceResponse.Success = false;
                return serviceResponse;
                // throw new Exception("User Already exits!");
            }

            var data = mapper.Map<User>(user2RegisterDto);
            data.Username = data.Username.ToLower();
            data.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user2RegisterDto.PasswordHash);

            databaseService.add(data);

            if (await databaseService.SaveAll())
            {
                var userRole = new UserRolePost()
                {
                    UserID = data.UserID,
                    zRoleId = user2RegisterDto.zRoleId,
                };
                var userRole2Save = mapper.Map<UserRole>(userRole);
                databaseService.add(userRole2Save);
                if (await databaseService.SaveAll())
                {
                    serviceResponse.Data = mapper.Map<User>(data);
                    return serviceResponse;
                }

                return serviceResponse;
            }
            return serviceResponse;
        }
    }
}
