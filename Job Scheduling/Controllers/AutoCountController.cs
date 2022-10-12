using System.Data.SqlClient;
using Dapper;
using Job_Scheduling.Model;
using Job_Scheduling.Model.AutoCount;
using Microsoft.AspNetCore.Mvc;

namespace Job_Scheduling.Controllers
{
    public class AutoCountController : Controller
    {

        private string _connAutoCountStr = string.Empty;
        private string _connStr = string.Empty;
        private readonly ILogger<AutoCountController> _logger;

        public AutoCountController(ILogger<AutoCountController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
            _connAutoCountStr = configuration.GetConnectionString("AutoCountConnection");
        }
        
        [HttpGet]
        [Route("quotations")]
        public IActionResult getQuotations(string entity_name)
        {
           /* User user = new User();
            if (!user.checkSession(HttpContext))
            {
                return new JsonResult("No Session");
            }*/
            
            // get user & Password
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                { 
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [entity_conf] where entity_conf_name=@entity_conf_name", connection);
                    cm.Parameters.AddWithValue("@entity_conf_name", entity_name);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    Entity_Conf entity_conf = new Entity_Conf();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<Entity_Conf>(typeof(Entity_Conf));
                            entity_conf = parser(sdr);
                        }
                        connection.Close();
                        using (SqlConnection connectionAutoCount = new SqlConnection(_connAutoCountStr))
                        {
                            connectionAutoCount.Open();
                            connectionAutoCount.ChangeDatabase(entity_conf.entity_conf_db_name);
                            SqlCommand cmAutoCount = new SqlCommand("select top 500 * from [QT] where Cancelled ='F' and ToDocKey is null order by dockey desc", connectionAutoCount);
                            SqlDataReader sdrAutoCount = cmAutoCount.ExecuteReader();

                            List<Quotation> quotations = new List<Quotation>();

                            if (sdrAutoCount.HasRows)
                            {
                                while (sdrAutoCount.Read())
                                {
                                    Quotation quotation = new Quotation();
                                    var parser = sdrAutoCount.GetRowParser<Quotation>(typeof(Quotation));
                                    quotation = parser(sdrAutoCount);
                                    quotations.Add(quotation);
                                }
                            } 
                                
                            connectionAutoCount.Close(); 
                            return StatusCode(200, quotations); 
                        }

                    }
                    else {
                        return StatusCode(404, string.Format("OOPs, something went wrong."));
                     }
                    
                }
            }
            catch (Exception e)
            {
                return StatusCode(404, string.Format("OOPs, something went wrong."+ e.Message));
            }

        }
        [HttpGet]
        [Route("quotation")]
        public IActionResult getQuotation(string entity_name, string quotation_no)
        {   
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [entity_conf] where entity_conf_name=@entity_conf_name", connection);
                    cm.Parameters.AddWithValue("@entity_conf_name", entity_name);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    Entity_Conf entity_conf = new Entity_Conf();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<Entity_Conf>(typeof(Entity_Conf));
                            entity_conf = parser(sdr);
                        }
                        connection.Close();
                        using (SqlConnection connectionAutoCount = new SqlConnection(_connAutoCountStr))
                        {
                            connectionAutoCount.Open();
                            connectionAutoCount.ChangeDatabase(entity_conf.entity_conf_db_name);
                            SqlCommand cmAutoCount = new SqlCommand("select * from [QT] where Cancelled ='F' and ToDocKey is null and docNo='" + quotation_no + "' order by dockey desc", connectionAutoCount);
                            SqlDataReader sdrAutoCount = cmAutoCount.ExecuteReader();

                            List<Quotation> quotations = new List<Quotation>();

                            if (sdrAutoCount.HasRows)
                            {
                                while (sdrAutoCount.Read())
                                {
                                    Quotation quotation = new Quotation();
                                    var parser = sdrAutoCount.GetRowParser<Quotation>(typeof(Quotation));
                                    quotation = parser(sdrAutoCount);
                                    quotations.Add(quotation);
                                }
                            }

                            connectionAutoCount.Close();
                            
                            return StatusCode(200, quotations);
                        }

                    }
                    else
                    {
                        return StatusCode(404, string.Format("OOPs, something went wrong.")); 
                    }

                }
            }
            catch (Exception e)
            {
                return StatusCode(404, string.Format("OOPs, something went wrong." + e.Message));
            }

        }
    }
}
