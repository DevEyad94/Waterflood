using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BackEndWaterFloodApp.Constants;
using BackEndWaterFloodApp.Dtos.User;
using BackEndWaterFloodApp.Services.DatabaseService;
using BackEndWaterFloodApp.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BackEndWaterFloodApp.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly IConfiguration configuration;
        private readonly IUserService userService;
        private readonly IDatabaseService databaseService;
        private readonly IMapper mapper;
        private readonly WaterfloodDbContext dataContext;

        public AuthController(
            IConfiguration configuration,
            IUserService userService,
            IDatabaseService databaseService,
            IMapper mapper,
            WaterfloodDbContext dataContext
        )
        {
            this.configuration = configuration;
            this.userService = userService;
            this.databaseService = databaseService;
            this.mapper = mapper;
            this.dataContext = dataContext;
        }

        [HttpPost("register"),
        // Authorize(Policy = Policies.AdminPolicy)
        ]
        public async Task<ActionResult<ServiceResponse<User>>> Register(User2RegisterDto userDto)
        {
            var user = await userService.NewUser(userDto);

            if (user.Success)
                return Ok(user);

            return BadRequest(user);
        }

        [HttpPut("ModifyUser"), Authorize(Policy = Policies.AdminPolicy)]
        public async Task<ActionResult<ServiceResponse<UserDto2Put>>> ModifyUser(
            UserDto2Put userDto
        )
        {
            if (!userService.CheckUserByUserId(userDto.UserID))
                return BadRequest();

            var user = await userService.ModifyUser(userDto);
            if (user.Success)
                return Ok(user);

            return BadRequest(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ServiceResponse<UserDto>>> Login(UserDto2Login userDto)
        {
            var userFromDB = await userService.Login(userDto.Username, userDto.PasswordHash);

            if (userFromDB.Success == false)
                return BadRequest(userFromDB);

            if (userFromDB.Data == null)
                return BadRequest(userFromDB);

            string token = CreateToken(userFromDB.Data);
            userFromDB.Data.Token = token;

            return Ok(userFromDB);
        }

        [Authorize(Policy = Policies.AdminPolicy)]
        [HttpGet("users")]
        public async Task<ActionResult<ServiceResponse<List<UserDto>>>> GetUsers()
        {
            var users = await userService.GetAllUsers();
            if (!users.Success)
                return BadRequest(users);
            return Ok(users);
        }

        [Authorize(Policy = Policies.AdminPolicy)]
        [HttpGet("roles")]
        public async Task<ActionResult<ServiceResponse<List<RoleListDto>>>> GetRoles()
        {
            var roles = await userService.GetRoles();
            return Ok(roles);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<ServiceResponse<UserDto>>> GetUserDataByToken()
        {
            var userFromDB = await userService.GetUserDataByToken();

            if (userFromDB.Success == false)
                return BadRequest(userFromDB);

            if (userFromDB.Data == null)
                return BadRequest(userFromDB);

            string token = CreateToken(userFromDB.Data);
            userFromDB.Data.Token = token;

            return Ok(userFromDB);
        }

        private string CreateToken(UserDto user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.UserData, user.UserID.ToString()),
                // new Claim("Issuer", configuration.GetSection("AppSettings:Issuer").Value!),
                // new Claim("Audience", configuration.GetSection("AppSettings:Audience").Value!)
            };

            claims.AddRange(user.Roles.Select(x => new Claim(ClaimTypes.Role, x.RoleName)));
            // foreach (var item in user.Role)
            // {
            //     Console.WriteLine(item);
            //     claims.Add(new Claim(ClaimTypes.Role, item));
            // }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:Token").Value!)
            );

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: cred,
                issuer: configuration["AppSettings:Issuer"],
                audience: configuration["AppSettings:Audience"]
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        private string GenerateJwtToken(UserDto user)
        {
            var issuer = this.configuration["AppSettings:Issuer"];
            var audience = this.configuration["AppSettings:Audience"];
            var key = Encoding.UTF8.GetBytes(
                configuration["AppSettings:Token"]
                    ?? throw new InvalidOperationException("JWT Token is not configured")
            );
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    new[]
                    {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                        new Claim(JwtRegisteredClaimNames.Email, user.Username),
                        // new Claim(ClaimTypes.Role, string.Join(",", user.Role)),
                        // new Claim(ClaimTypes.Role, user.Role),


                        // new Claim(ClaimTypes.Role, user.Role.ToString()),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    }
                ),
                Expires = DateTime.Now.AddDays(1),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature
                ),
            };
            foreach (var item in user.Role)
            {
                Console.WriteLine(item);
                tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, item));
            }

            // tokenDescriptor.Subject.AddClaim

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
