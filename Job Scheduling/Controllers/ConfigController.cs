using Dapper;
using Job_Scheduling.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Text.Json;

namespace Job_Scheduling.Controllers
{
    public class ConfController : Controller
    {
        private string _connStr = string.Empty;
        private readonly ILogger<ConfController> _logger;
        public ConfController(ILogger<ConfController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        [Route("getEntityName")]
        public IActionResult getEntityName()
        {
            // get user & Password
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [entity_conf] where entity_conf_status='Active'", connection); 

                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    List<Entity_Conf> confs = new List<Entity_Conf>();
                    if (sdr.HasRows)
                    {                         
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<Entity_Conf>(typeof(Entity_Conf));
                            Entity_Conf conf = parser(sdr);
                            confs.Add(conf);
                        }
                    }
                    return new JsonResult(confs);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }
             
        }

      

    }
}
