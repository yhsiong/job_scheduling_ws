using Dapper;
using Job_Scheduling.Database;
using Job_Scheduling.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Text.Json;

namespace Job_Scheduling.Controllers
{
    public class UserController : Controller
    {
        private User_Context _User_Context;
        private string _connStr = string.Empty;
        private readonly ILogger<UserController> _logger;
        public UserController(User_Context user_Context, ILogger<UserController> logger, IConfiguration configuration)
        {
            _User_Context = user_Context;
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
        }
         
        [HttpPost]
        [HttpOptions]
        [Route("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            User.Dto.Get user = await Job_Scheduling.Model.User.Operations.ReadAuthUser(_User_Context, username,password);
            if (user == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                if (user.user_status.ToString().Equals("Active"))
                {
                    user.user_session_id = HttpContext.Session.Id;
                    HttpContext.Session.SetString("sessionId", HttpContext.Session.Id);
                    HttpContext.Session.SetString("userId", user.user_id.ToString());
                    HttpContext.Session.SetString("sessionStart", DateTime.Now.ToString());
                }
                return StatusCode(200, new JsonResult(user));
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
                return StatusCode(200, new JsonResult(users));
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
                return StatusCode(200, new JsonResult(user));
            }
        }

        [HttpPost]
        [Route("user")]
        public async Task<IActionResult> insertUser(User.Dto.Post user)
        {
            user.user_id = new Guid();
            user.user_created_at = DateTime.Now;
            bool status = await Job_Scheduling.Model.User.Operations.Create(_User_Context, user);
            if (status)
            {
                return StatusCode(200, new JsonResult(user));
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
            user.user_updated_at = DateTime.Now;
            bool status = await Job_Scheduling.Model.User.Operations.Update(_User_Context, user);

            if (status)
            {
                return StatusCode(200, new JsonResult(user));
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        } 

    }
}
