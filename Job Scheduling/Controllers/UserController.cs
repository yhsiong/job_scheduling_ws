using Dapper;
using Job_Scheduling.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Text.Json;

namespace Job_Scheduling.Controllers
{
    public class UserController : Controller
    {
        private string _connStr = string.Empty;
        private readonly ILogger<UserController> _logger;
        public UserController(ILogger<UserController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
        }
         
        [HttpPost]
        [HttpOptions]
        [Route("login")]
        public IActionResult Login(string username, string password)
        {
            if (HttpContext.Request.Method == "OPTIONS")
            {
                Response.Headers.Add("Access-Control-Allow-Origin","*");
                Response.Headers.Add("Access-Control-Allow-Headers", "Hello");
                return Ok();
            }
            // get user & Password
            try
            { 
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [user] where user_username=@username and user_password=@password", connection);
                    cm.Parameters.AddWithValue("@username", username);
                    cm.Parameters.AddWithValue("@password", password);

                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    if (sdr.HasRows)
                    {
                        var parser = sdr.GetRowParser<User>(typeof(User));
                        while (sdr.Read())
                        { 
                            if (sdr["user_status"].ToString().Equals("Active"))
                            {
                                User user = parser(sdr);
                                user.user_session_id = HttpContext.Session.Id;
                                HttpContext.Session.SetString("sessionId", HttpContext.Session.Id);
                                HttpContext.Session.SetString("userId", sdr["user_id"].ToString());
                                HttpContext.Session.SetString("sessionStart", DateTime.Now.ToString());
                                return new JsonResult(user);
                            }
                            else
                            {
                                Response.StatusCode = 201;
                                return new JsonResult("Invalid user");
                            }
                        }
                    }
                    else {
                        Response.StatusCode = 201;
                        return new JsonResult("Invalid username or password"); 
                    } 
                }
            }
            catch (Exception e)
            {
                return Content("OOPs, something went wrong.\n" + e);
            }

            return new JsonResult("");
        }
        [HttpPost]
        [Route("logout")]
        public IActionResult Logout(string sessionId)
        {
            if (HttpContext.Session.Id.Equals(sessionId))
            {
                HttpContext.Session.Clear();
            }
            else {
                return new JsonResult("Invalid session");
            }
            
            return new JsonResult("Logout successful");
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
    }
}
