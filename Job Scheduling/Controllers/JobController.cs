using Dapper;
using Job_Scheduling.Database;
using Job_Scheduling.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Job_Scheduling.Controllers
{
    public class JobController : Controller
    {
        private Job_Context _Job_Context;
        private string _connStr = string.Empty;
        private readonly ILogger<JobController> _logger;
        public JobController(Job_Context job_Context,ILogger<JobController> logger, IConfiguration configuration)
        {
            _Job_Context = job_Context;
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
        }
        #region job
        [HttpGet]
        [Route("jobs")]
        public async Task<IActionResult> getJobs()
        {
            List<Job.Dto.Get> jobs = await Job.Operations.ReadAll(_Job_Context);
            if (jobs == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, new JsonResult(jobs));
            }  
        }

        [HttpGet]
        [Route("jobById")]
        public async Task<IActionResult> getJobById(string job_id)
        {
            Job.Dto.Get job = await Job.Operations.ReadSingleById(_Job_Context, Guid.Parse(job_id));
            if (job == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, new JsonResult(job));
            } 
        }
        [HttpGet]
        [Route("jobByNo")]
        public async Task<IActionResult> getJobByNo(string job_no)
        {
            Job.Dto.Get job = await Job.Operations.ReadSingleByNo(_Job_Context, job_no);
            if (job == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, new JsonResult(job));
            }
        }
        [HttpPost]
        [Route("job")]
        public async Task<IActionResult> insertJob(Job.Dto.Post job)
        {
            job.job_id = new Guid();
            job.job_no = await Job.Operations.generateJobNo(_Job_Context);
            job.job_created_at = DateTime.Now;
            job.job_status = "Active";
            bool status = await Job.Operations.Create(_Job_Context, job);
            if (status)
            {
                return StatusCode(200, new JsonResult(job));
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        } 
        [HttpPut]
        [Route("job")]
        public async Task<IActionResult> updateJob(Job.Dto.Put job)
        { 
            job.job_updated_at = DateTime.Now;
            bool status = await Job.Operations.Update(_Job_Context,job);

            if (status)
            {
                return StatusCode(200, new JsonResult(job));
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
         
        [HttpGet]
        [Route("getpostalcode")]
        public IActionResult getPostalCode(string address)
        { 
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
                        return new JsonResult(new { 
                            postal_code = jResult.results[0]["POSTAL"].ToString(),
                            latitude = jResult.results[0]["LATITUDE"].ToString(),
                            longtitude = jResult.results[0]["LONGTITUDE"].ToString()
                        });
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

        [HttpGet]
        [Route("getLongLat")]
        public IActionResult getLongLat(string postalCode)
        {
            // get user & Password
            try
            {
                var url = "https://developers.onemap.sg/commonapi/search?searchVal=" + postalCode + "&returnGeom=Y&getAddrDetails=Y&pageNum=1";

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);

                httpRequest.Accept = "application/json";


                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var jResult = JsonConvert.DeserializeObject<dynamic>(result);
                    if (jResult.results.Count > 0)
                    {
                        return new JsonResult(new { 
                            latitude = jResult.results[0]["LATITUDE"].ToString(),
                            longtitude = jResult.results[0]["LONGTITUDE"].ToString(),
                        });
                    }
                    
                    else
                    {
                        return new JsonResult(new { postal_code = "Address not found" });
                    }

                }


            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }

        }
        #endregion
        #region job task
        [HttpGet]
        [Route("jobTaskByJobId")]
        public async Task<IActionResult> jobTaskByJobId(string job_id)
        {
            List<Job_Task.Dto.Get> jobTasks = await Job_Task.Operations.ReadSingleByJobId(_Job_Context, Guid.Parse(job_id));
            if (jobTasks == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, new JsonResult(jobTasks));
            }
        }
        [HttpGet]
        [Route("jobTaskById")]
        public async Task<IActionResult> jobTaskById(string job_task_id)
        {
            Job_Task.Dto.Get jobTask = await Job_Task.Operations.ReadSingleById(_Job_Context, Guid.Parse(job_task_id));
            if (jobTask == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, new JsonResult(jobTask));
            }
        }

        [HttpPost]
        [Route("jobTask")]
        public async Task<IActionResult> insertjobTask(Job_Task.Dto.Post jobTask)
        {
            jobTask.job_task_id = new Guid();
            jobTask.job_task_created_at = DateTime.Now;
            jobTask.job_task_status = "Active";
            bool status = await Job_Task.Operations.Create(_Job_Context, jobTask);
            if (status)
            {
                return StatusCode(200, new JsonResult(jobTask));
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        [HttpPut]
        [Route("jobTask")]
        public async Task<IActionResult> updateJobTask(Job_Task.Dto.Put jobTask)
        {
            jobTask.job_task_updated_at = DateTime.Now;
            bool status = await Job_Task.Operations.Update(_Job_Context, jobTask);

            if (status)
            {
                return StatusCode(200, new JsonResult(jobTask));
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        #endregion

    }
}
