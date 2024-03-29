﻿using Castle.MicroKernel.Registration;
using Dapper;
using Job_Scheduling.Database;
using Job_Scheduling.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Job_Scheduling.Controllers
{
    public class JobController : Controller
    {
        private Job_Context _Job_Context;
        private string _connStr = string.Empty;
        private string _accessToken = string.Empty;
        private readonly ILogger<JobController> _logger;
        public JobController(Job_Context job_Context,ILogger<JobController> logger, IConfiguration configuration)
        {
            _Job_Context = job_Context;
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
            _accessToken = configuration.GetValue<string>("AccessToken");

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
                return StatusCode(200, jobs);
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
                return StatusCode(200, job);
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
                return StatusCode(200, job);
            }
        }
        [HttpPost]
        [Route("job")]
        public async Task<IActionResult> insertJob(Job.Dto.Post job)
        {
            job.job_id = new Guid();
            job.job_no = await Job.Operations.generateJobNo(_Job_Context);
            job.job_created_at = DateTime.Now;
            job.job_created_by = UserController.checkUserId(HttpContext);
            job.job_status = (job.job_created_at >= job.job_start_date)? "Active" : "Pending";
            bool status = await Job.Operations.Create(_Job_Context, job);
            if (status)
            {
                return StatusCode(200, job);
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
            job.job_created_by = UserController.checkUserId(HttpContext);
            job.job_updated_at = DateTime.Now;
            bool status = await Job.Operations.Update(_Job_Context,job);

            if (status)
            {
                return StatusCode(200, job);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("cronjob")]
        public async Task<IActionResult> jobChecker(string accessCode)
        {
            if (accessCode == _accessToken)
            {
                List<Job.Dto.Get> jobs = await Job.Operations.ReadAllPending(_Job_Context, DateTime.Now);
                if(jobs.Count > 0) { 
                    List<Job.Dto.Put> updJobs = new List<Job.Dto.Put>();
                    for (int i = 0; i < jobs.Count; i++)
                    {
                        jobs[i].job_status = "Active";
                        jobs[i].job_updated_at = DateTime.Now;

                        Job.Dto.Put updJob = JsonConvert.DeserializeObject<Job.Dto.Put>(JsonConvert.SerializeObject(jobs[i]));
                        updJobs.Add(updJob);
                    } 
                 
                    Job.Operations.UpdateRange(_Job_Context, updJobs);
                }
                return StatusCode(200);
            }
            else
            {
                return StatusCode(404, "Error!");
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
                    else 
                    {
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

        [HttpGet]
        [Route("checkCloseById")]
        public async Task<IActionResult> checkCloseById(string job_id)
        {
            List<Job_Task.Dto.Get> jobTasks = await Job_Task.Operations.ReadSingleActiveByJobId(_Job_Context, Guid.Parse(job_id));
            if (jobTasks == null || jobTasks.Count == 0)
            {
                Job job = await Job.Operations.ReadSingleById(_Job_Context, Guid.Parse(job_id));
                Job.Dto.Put dtoJob = new Job.Dto.Put();               
                dtoJob.job_remark = job.job_remark;
                dtoJob.job_id = job.job_id;
                dtoJob.job_status = "Completed";
                dtoJob.job_updated_by = UserController.checkUserId(HttpContext);
                dtoJob.job_updated_at = DateTime.Now;

                bool status = await Job.Operations.Update(_Job_Context, dtoJob);

                return StatusCode(200, job);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
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
                return StatusCode(200, jobTasks);
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
                return StatusCode(200, jobTask);
            }
        }

        [HttpPost]
        [Route("jobTask")]
        public async Task<IActionResult> insertjobTask(Job_Task.Dto.Post jobTask)
        {
            jobTask.job_task_id = new Guid();
            jobTask.job_task_created_at = DateTime.Now;
            jobTask.job_task_created_by = UserController.checkUserId(HttpContext);
            jobTask.job_task_status = "Active";
            bool status = await Job_Task.Operations.Create(_Job_Context, jobTask);
            if (status)
            {
                return StatusCode(200, jobTask);
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
            jobTask.job_task_updated_by = UserController.checkUserId(HttpContext);
            jobTask.job_task_updated_at = DateTime.Now;
            bool status = await Job_Task.Operations.Update(_Job_Context, jobTask);

            if (status)
            {
                return StatusCode(200, jobTask);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        #endregion

    }
}
