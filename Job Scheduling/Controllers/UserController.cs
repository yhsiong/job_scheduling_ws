using Dapper;
using Job_Scheduling.Database;
using Job_Scheduling.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Job_Scheduling.Controllers
{
    public class UserController : Controller
    {
        private User_Context _User_Context;
        private string _connStr = string.Empty;
        private readonly ILogger<UserController> _logger;
        private string _issuer = string.Empty;
        private string _audience = string.Empty;
        private byte[] _key;
        public UserController(User_Context user_Context, ILogger<UserController> logger, IConfiguration configuration)
        {
            _User_Context = user_Context;
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
            _issuer = configuration.GetValue<string>("Jwt:Issuer");
            _audience = configuration.GetValue<string>("Jwt:Audience");
            _key = Encoding.ASCII.GetBytes(configuration.GetValue<string>("Jwt:Key"));
        }
         
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password)
        {
            User.Dto.Get user = await Job_Scheduling.Model.User.Operations.ReadAuthUser(_User_Context, username, passwordHash(password));
            if (user == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                if (user.user_status.ToString().Equals("Active"))
                {
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new[]
                        {
                            new Claim(JwtRegisteredClaimNames.NameId, user.user_id.ToString()),
                            new Claim(JwtRegisteredClaimNames.Name, user.user_name),
                            new Claim(JwtRegisteredClaimNames.Email, user.user_name),
                        }),
                        Expires = DateTime.Now.AddMinutes(15),
                        Issuer = _issuer,
                        Audience = _audience,
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha512Signature)
                    };
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var jwtToken = tokenHandler.WriteToken(token);
                    var stringToken = tokenHandler.WriteToken(token);
                    return StatusCode(200, stringToken);
                }
                return StatusCode(200, user);
            }
             
        }
        [HttpPost]
        [Route("logout")]
        public IActionResult Logout(string sessionId)
        {
            string returnMsg = string.Empty;
            if (HttpContext.Session.Id.Equals(sessionId))
            {
                HttpContext.Session.Clear();
                return StatusCode(200, new JsonResult("Logout successful"));
            }
            else {
                return StatusCode(404, new JsonResult("Invalid session"));
            } 
        }
        [HttpGet]
        [Route("getSessionId")]
        public IActionResult getSessionid()
        {
            User user = new User();
            if (user.checkSession(HttpContext))
            {
                return new JsonResult(HttpContext.Session.Id);
            }
            return new JsonResult("No Session");
        }


        [HttpGet]
        [Route("users")]
        public async Task<IActionResult> getUsers()
        {
            List<User.Dto.Get> users = await Job_Scheduling.Model.User.Operations.ReadAll(_User_Context);
            if (users == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, users);
            }
        }

        [HttpGet]
        [Route("userById")]
        public async Task<IActionResult> userById(Guid user_id)
        {
            User.Dto.Get user = await Job_Scheduling.Model.User.Operations.ReadSingleById(_User_Context,user_id);
            if (user == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, user);
            }
        }

        [HttpPost]
        [Route("user")]
        public async Task<IActionResult> insertUser(User.Dto.Post user)
        {
            user.user_id = new Guid();
            user.user_created_at = DateTime.Now;
            user.user_password = passwordHash(user.user_password);

            bool status = await Job_Scheduling.Model.User.Operations.Create(_User_Context, user);
            if (status)
            {
                return StatusCode(200, user);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        [HttpPut]
        [Route("user")]
        public async Task<IActionResult> updateUser(User.Dto.Put user)
        {
            if (!string.IsNullOrEmpty(user.user_password))
            {
                user.user_password = passwordHash(user.user_password);
            }

            user.user_updated_at = DateTime.Now;
            bool status = await Job_Scheduling.Model.User.Operations.Update(_User_Context, user);

            if (status)
            {
                return StatusCode(200, user);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        private string passwordHash(string password)
        {

            byte[] data = System.Text.Encoding.ASCII.GetBytes(password);
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            return System.Text.Encoding.ASCII.GetString(data);
        }
        public static string checkUserId(HttpContext httpContext)
        {
            string userId = string.Empty;
            var identity = httpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                IEnumerable<Claim> claim = identity.Claims;
                userId = claim.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            }
            return userId;
        }

    }
}
