using Dapper;
using Job_Scheduling.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net;
using System.Text.Json;

namespace Job_Scheduling.Controllers
{
    public class JobController : Controller
    {
        private string _connStr = string.Empty;
        private readonly ILogger<JobController> _logger;
        public JobController(ILogger<JobController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
        }
        [HttpGet]
        [Route("jobs")]
        public IActionResult getJobs()
        {
            // get user & Password
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [job]", connection); 

                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    List<Job> jobs = new List<Job>();
                    if (sdr.HasRows)
                    {                         
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<Job>(typeof(Job));
                            Job job = parser(sdr);
                            jobs.Add(job);
                        }
                    }
                    return new JsonResult(jobs);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }
             
        }

        [HttpGet]
        [Route("job")]
        public IActionResult getJob(string job_no)
        {
            // get user & Password
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [job] where job_no=@job_no", connection);
                    cm.Parameters.AddWithValue("@job_no", job_no);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    Job job = new Job();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<Job>(typeof(Job));
                            job = parser(sdr);
                        }
                    }
                    return new JsonResult(job);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }

        }
        [HttpPost]
        [Route("job")]
        public IActionResult insertJob(Job job)
        {
             
          if (string.IsNullOrEmpty(job.job_created_by))
          {
              return new JsonResult("No Session");
          }
            job.job_no = generateJobNo();
            job.job_created_at = DateTime.Now;
            job.job_status = "Active";
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("insert into [job] " +
                        "(job_no,job_quotation_no,job_remark,job_status, job_created_by,job_created_at) values " +
                        "(@job_no, @job_quotation_no, @job_remark, @job_status, @job_created_by,@job_created_at)", connection);

                    cm.Parameters.AddWithValue("@job_no", job.job_no);
                    cm.Parameters.AddWithValue("@job_remark", job.job_remark);
                    cm.Parameters.AddWithValue("@job_quotation_no", job.job_quotation_no);
                    cm.Parameters.AddWithValue("@job_status", job.job_status);
                    cm.Parameters.AddWithValue("@job_created_by", job.job_created_by);
                    cm.Parameters.AddWithValue("@job_created_at", job.job_created_at);

                    
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                     int result = cm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        return new JsonResult(job);
                    }
                    else
                    {
                        return new JsonResult("Error inserting");
                    }
                    
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }
        }
        private string generateJobNo()
        {
            string jobNoPrefix = "JOB-";
            int formatLenght = 5;
            string generatedJobNo = string.Empty;
                
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select top 1 * from [job] order by job_id desc", connection);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    int last_id = 1;
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            last_id = int.Parse(sdr["job_id"].ToString());

                             
                        }
                    }
                    generatedJobNo = jobNoPrefix + string.Format("{0}", last_id.ToString().PadLeft(formatLenght, '0'));
                }
            }
            catch (Exception e)
            {
                //return new JsonResult("OOPs, something went wrong.\n" + e);
            }

            return generatedJobNo;
        }
        [HttpPut]
        [Route("job")]
        public IActionResult updateJob(string job_id,string job_remark,string job_status)
        {
            Job job = new Job();
            job.job_id = int.Parse(job_id);
            job.job_remark = job_remark;
            job.job_status = job_status;
            job.job_updated_by = HttpContext.Session.GetString("userId");
            job.job_updated_at = DateTime.Now;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("update [job] set job_status=@job_status,job_remark=@job_remark," +
                    "job_updated_by=@job_updated_by,job_updated_at=@job_updated_at where job_id=@job_id", connection);

                    cm.Parameters.AddWithValue("@job_id", job.job_id); 
                    cm.Parameters.AddWithValue("@job_remark", job.job_remark);
                    cm.Parameters.AddWithValue("@job_status", job.job_status);
                    cm.Parameters.AddWithValue("@job_updated_by", job.job_updated_by);
                    cm.Parameters.AddWithValue("@job_updated_at", job.job_updated_at);


                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    int result = cm.ExecuteNonQuery();
                    if (result > 0)
                    { 
                        return new JsonResult(job);
                    }
                    else
                    {
                        return new JsonResult("Error inserting");
                    }

                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }
        }


        [HttpGet]
        [Route("getpostalcode")]
        public IActionResult getPostalCode(string address)
        {
            // get user & Password
            try
            {
                var url = "https://developers.onemap.sg/commonapi/search?searchVal="+ address + "&returnGeom=Y&getAddrDetails=Y&pageNum=1";

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);

                httpRequest.Accept = "application/json";


                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var jResult = JsonConvert.DeserializeObject<dynamic>(result);
                    if (jResult.results.Count > 0)
                    {
                        return new JsonResult(new { postal_code = jResult.results[0]["POSTAL"].ToString() });
                    }
                    else {
                        return new JsonResult(new { postal_code = "Address not found"});
                    } 

                }

                
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }

        }
    }
}
