using Dapper;
using Hangfire.Common;
using Job_Scheduling.Database;
using Job_Scheduling.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using Z.EntityFramework.Plus;

namespace Job_Scheduling.Controllers
{
    public class ScheduleController : Controller
    {
        private Schedule_Context _Schedule_Context;
        private Vehicle_Context _Vehicle_Context;
        private string _connStr = string.Empty;
        private readonly ILogger<ScheduleController> _logger;
        public ScheduleController(Schedule_Context schedule_Context, Vehicle_Context vehicle_Context, ILogger<ScheduleController> logger, IConfiguration configuration)
        {
            _Schedule_Context = schedule_Context;
            _Vehicle_Context = vehicle_Context;
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
        }
        #region schedule
        [HttpGet]
        [Route("schedules")]
        public async Task<IActionResult> getSchedules()
        { 
            List<Schedule.Dto.Get> schedules = await Schedule.Operations.ReadAll(_Schedule_Context);
            if (schedules == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, schedules);
            } 
        }
        [HttpGet]
        [Route("schedule")]
        public async Task<IActionResult> getSchedule(string schedule_id)
        {
            Schedule.Dto.Get schedule = await Schedule.Operations.ReadSingleById(_Schedule_Context, Guid.Parse(schedule_id));
            if (schedule == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, schedule);
            } 
        }
        [HttpGet]
        [Route("schedulebydate")]
        public async Task<IActionResult> getScheduleByDate(string schedule_date)
        {
            Schedule.Dto.Get schedule = await Schedule.Operations.ReadSingleByDate(_Schedule_Context, schedule_date);
            if (schedule == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, schedule);
            }
        }
        [HttpGet]
        [Route("cronschedule")]
        public async Task<IActionResult> insertCronSchedule(string schedule_date)
        {
            Schedule.Dto.Get schedule = await Schedule.Operations.ReadSingleByDate(_Schedule_Context, schedule_date);
            if (schedule == null)
            {
                Schedule.Dto.Post schedule_post = new Schedule.Dto.Post();
                schedule_post.schedule_date = schedule_date;
                schedule_post.schedule_id = new Guid();
                schedule_post.schedule_created_at = DateTime.Now;
                schedule_post.schedule_status = "Processing";
                bool status = await Schedule.Operations.Create(_Schedule_Context, schedule_post);
                if (status)
                {
                    this.generateRoute(schedule_post.schedule_id.ToString(), "distance");
                    return StatusCode(200, schedule_post);
                }
                else
                {
                    return StatusCode(404, string.Format("Could not find config"));
                }
            }
            else
            {
                return StatusCode(200, schedule);
            } 
        }
        [HttpPost]
        [Route("schedule")]
        public async Task<IActionResult> insertSchedule(Schedule.Dto.Post schedule)
        {
            schedule.schedule_id = new Guid();
            schedule.schedule_created_at = DateTime.Now;
            schedule.schedule_status = "Processing";
            bool status = await Schedule.Operations.Create(_Schedule_Context, schedule);
            if (status)
            {
                this.generateRoute(schedule.schedule_id.ToString(),"distance");
                return StatusCode(200, schedule);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        [HttpPut]
        [Route("schedule")]
        public async Task<IActionResult> updateSchedule(Schedule.Dto.Put scheduleScheme)   
        {
            scheduleScheme.schedule_updated_at = DateTime.Now;
            bool status = await Schedule.Operations.Update(_Schedule_Context, scheduleScheme);

            if (status)
            {
                return StatusCode(200, scheduleScheme);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        #endregion
        #region schedule job
        [HttpGet]
        [Route("schedulejobs")]
        public async Task<IActionResult> getScheduleJobs(string schedule_id)
        {
            List<Schedule_Job.Dto.Get> scheduleJobs = await Schedule_Job.Operations.ReadSingleByScheduleId(_Schedule_Context, Guid.Parse(schedule_id));
            if (scheduleJobs == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, scheduleJobs);
            }
        }
        [HttpGet]
        [Route("schedulejobswithdetails")]
        public IActionResult getScheduleJobsWithDetails(string schedule_id)
        {
            // get user & Password
            try
            {
                //schedule job
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from[Schedule_job] right join vehicle on vehicle_id =[Schedule_job].schedule_job_vehicle_id " +
                    " inner join job on job_id =[Schedule_job].schedule_job_job_id where schedule_job_schedule_id=@schedule_job_schedule_id", connection);
                    cm.Parameters.AddWithValue("@schedule_job_schedule_id", schedule_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    List<dynamic> schedulejobs = new List<dynamic>();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<dynamic>();
                            dynamic schedulejob = parser(sdr);
                            schedulejobs.Add(schedulejob);
                        }
                    }
                    connection.Close();

                    connection.Open();
                    //unscheduled job
                    cm = new SqlCommand("select *, 'Z_Unassigned Job' as vehicle_plat_no from job where job_status='Active' and job_id not in (select schedule_job_job_id from Schedule_job  where schedule_job_schedule_id = @schedule_job_schedule_id)", connection);
                    cm.Parameters.AddWithValue("@schedule_job_schedule_id", schedule_id);
                    sdr = cm.ExecuteReader();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<dynamic>();
                            dynamic schedulejob = parser(sdr); 
                            schedulejobs.Add(schedulejob);
                        }
                    }
                    connection.Close();

                    return new JsonResult(schedulejobs);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }

        }
        [HttpPut]
        [Route("scheduleJob")]
        public async Task<IActionResult> updateScheduleJob(string jsonSchedule)
        {
            dynamic schedule = JsonConvert.DeserializeObject<dynamic>(jsonSchedule);
            Guid schedule_job_id = Guid.Parse(schedule["schedule_job_id"].ToString());

            if (schedule["tools"] != null)
            {
                var tools = schedule["tools"];
                // clear all tools
                var scheduleJobTools = _Schedule_Context.Schedule_Job_Tool.Where(x => x.sjt_schedule_job_id.Equals(schedule_job_id));
                _Schedule_Context.Schedule_Job_Tool.RemoveRange(scheduleJobTools);
                _Schedule_Context.SaveChanges();
                Schedule_Job_Tool.Dto.Post scheduleTool = new Schedule_Job_Tool.Dto.Post();

                for (int i = 0; i < tools.Count; i++)
                {
                    scheduleTool = new Schedule_Job_Tool.Dto.Post();
                    scheduleTool.sjt_id = new Guid();
                    scheduleTool.sjt_tool_id = tools[i]["tool_id"];
                    scheduleTool.sjt_schedule_job_id = schedule_job_id;
                    scheduleTool.sjt_status = "Active";
                    scheduleTool.sjt_created_at = DateTime.Now;
                    await Schedule_Job_Tool.Operations.Create(_Schedule_Context, scheduleTool);
                }
            }

            if (schedule["materials"] != null)
            {
                var materials = schedule["materials"];
                // clear all material
                var scheduleJobMaterials = _Schedule_Context.Schedule_Job_Material.Where(x => x.sjm_schedule_job_id.Equals(schedule_job_id));
                _Schedule_Context.Schedule_Job_Material.RemoveRange(scheduleJobMaterials);
                _Schedule_Context.SaveChanges();
                Schedule_Job_Material.Dto.Post scheduleMaterial = new Schedule_Job_Material.Dto.Post();
                for (int i = 0; i < materials.Count; i++)
                {
                    scheduleMaterial = new Schedule_Job_Material.Dto.Post();
                    scheduleMaterial.sjm_id = new Guid();
                    scheduleMaterial.sjm_material_id = materials[i]["material_id"];
                    scheduleMaterial.sjm_schedule_job_id = schedule_job_id;
                    scheduleMaterial.sjm_status = "Active";
                    scheduleMaterial.sjm_created_at = DateTime.Now;
                    scheduleMaterial.sjm_quantity = materials[i]["material_quantity"];
                    await Schedule_Job_Material.Operations.Create(_Schedule_Context, scheduleMaterial);
                }

            }
             
            if (schedule["workers"] != null)
            {
                var workers = schedule["workers"];
                // clear all worker
                var scheduleJobWorkers = _Schedule_Context.Schedule_Job_Worker.Where(x => x.sjw_schedule_job_id.Equals(schedule_job_id));
                _Schedule_Context.Schedule_Job_Worker.RemoveRange(scheduleJobWorkers);
                _Schedule_Context.SaveChanges();
                Schedule_Job_Worker.Dto.Post scheduleWorker = new Schedule_Job_Worker.Dto.Post();
                for (int i = 0; i < workers.Count; i++)
                {
                    scheduleWorker = new Schedule_Job_Worker.Dto.Post();
                    scheduleWorker.sjw_id = new Guid();
                    scheduleWorker.sjw_worker_id = workers[i]["user_id"];
                    scheduleWorker.sjw_schedule_job_id = schedule_job_id;
                    scheduleWorker.sjw_status = "Active";
                    scheduleWorker.sjw_created_at = DateTime.Now;
                    await Schedule_Job_Worker.Operations.Create(_Schedule_Context, scheduleWorker);
                }

            }

            string remarks = schedule["remarks"].ToString(); 
            Schedule_Job scheduleJob = _Schedule_Context.Schedule_Job.Where(x => x.schedule_job_id.Equals(schedule_job_id)).FirstOrDefault();
            scheduleJob.schedule_job_remark = remarks;
            await Schedule_Job.Operations.Update(_Schedule_Context, (Schedule_Job.Dto.Put)scheduleJob);
             
            return StatusCode(200, true);
            /*bool status = await Schedule.Operations.Update(_Schedule_Context, scheduleScheme);

            if (status)
            {
                return StatusCode(200, scheduleScheme);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }*/
        }
        [HttpPut]
        [Route("scheduleJobs")]
        public async Task<IActionResult> updateScheduleJobs(string schedule_id, string jsonSchedule)
        {
            Dictionary<string, List<dynamic>> schedules = JsonConvert.DeserializeObject<Dictionary<string, List<dynamic>>>(jsonSchedule);

            // set all job id to deleted
            var scheduleJobs = _Schedule_Context.Schedule_Job.Where(x => x.schedule_job_schedule_id.Equals(Guid.Parse(schedule_id)));
            if (scheduleJobs.Count() > 0)
            {
                _Schedule_Context.Schedule_Job.RemoveRange(scheduleJobs);
                _Schedule_Context.SaveChanges();
            }

            Schedule_Job.Dto.Post newScheduleJob = new Schedule_Job.Dto.Post();
            foreach(KeyValuePair<string,List<dynamic>> schedule in schedules)
            {
                newScheduleJob = new Schedule_Job.Dto.Post();
                if (!schedule.Key.Equals("Z_Unassigned Job"))
                {

                    Vehicle.Dto.Get vehicle = await Vehicle.Operations.ReadSingleByPlatNo(_Vehicle_Context, schedule.Key);
                    newScheduleJob.schedule_job_vehicle_id = vehicle.vehicle_id;
                    newScheduleJob.schedule_job_schedule_id = Guid.Parse(schedule_id);
                    newScheduleJob.schedule_job_created_at = DateTime.Now;

                    for (int i = 0; i < schedule.Value.Count(); i++)
                    {
                        if (schedule.Value[i].job_id != null)
                        {
                            newScheduleJob.schedule_job_id = new Guid();
                            newScheduleJob.schedule_job_job_id = schedule.Value[i].job_id;
                            newScheduleJob.schedule_job_order = i;
                            newScheduleJob.schedule_job_schedule_id = Guid.Parse(schedule_id);
                            newScheduleJob.schedule_job_remark = schedule.Value[i].schedule_job_remark ?? "";
                            await Schedule_Job.Operations.Create(_Schedule_Context, newScheduleJob);
                            dynamic _schedule = schedule.Value[i];
                            Guid old_schedule_job_id = Guid.Parse(_schedule["schedule_job_id"].ToString());
                            Guid new_schedule_job_id = newScheduleJob.schedule_job_id;

                            if (_schedule["tools"] != null)
                            {
                                var tools = _schedule["tools"];
                                if (tools.Count > 0)
                                {
                                    // clear all tools
                                    using (SqlConnection connection = new SqlConnection(_connStr))
                                    {
                                        // Creating SqlCommand objcet   
                                        SqlCommand cm = new SqlCommand("delete from Schedule_Job_Tool where sjt_schedule_job_id=@sjt_schedule_job_id", connection);
                                        cm.Parameters.AddWithValue("@sjt_schedule_job_id", old_schedule_job_id);
                                        // Opening Connection  
                                        connection.Open();
                                        // Executing the SQL query  
                                        cm.ExecuteNonQuery();
                                    } 

                                    Schedule_Job_Tool.Dto.Post scheduleTool = new Schedule_Job_Tool.Dto.Post();

                                    for (int i1 = 0; i1 < tools.Count; i1++)
                                    {
                                        scheduleTool = new Schedule_Job_Tool.Dto.Post();
                                        scheduleTool.sjt_id = new Guid();
                                        scheduleTool.sjt_tool_id = tools[i1]["tool_id"];
                                        scheduleTool.sjt_schedule_job_id = new_schedule_job_id;
                                        scheduleTool.sjt_status = "Active";
                                        scheduleTool.sjt_quantity = tools[i1]["tool_quantity"];
                                        scheduleTool.sjt_created_at = DateTime.Now;
                                        await Schedule_Job_Tool.Operations.Create(_Schedule_Context, scheduleTool);
                                    }
                                }

                            }

                            if (_schedule["materials"] != null)
                            {
                                var materials = _schedule["materials"];
                                if (materials.Count > 0)
                                {
                                    // clear all material
                                    using (SqlConnection connection = new SqlConnection(_connStr))
                                    {
                                        // Creating SqlCommand objcet   
                                        SqlCommand cm = new SqlCommand("delete from Schedule_Job_Material where sjm_schedule_job_id=@sjm_schedule_job_id", connection);
                                        cm.Parameters.AddWithValue("@sjm_schedule_job_id", old_schedule_job_id);
                                        // Opening Connection  
                                        connection.Open();
                                        // Executing the SQL query  
                                        cm.ExecuteNonQuery(); 
                                    }
                                     
                                    Schedule_Job_Material.Dto.Post scheduleMaterial = new Schedule_Job_Material.Dto.Post();
                                    for (int i2 = 0; i2 < materials.Count; i2++)
                                    {
                                        scheduleMaterial = new Schedule_Job_Material.Dto.Post();
                                        scheduleMaterial.sjm_id = new Guid();
                                        scheduleMaterial.sjm_material_id = materials[i2]["material_id"];
                                        scheduleMaterial.sjm_schedule_job_id = new_schedule_job_id;
                                        scheduleMaterial.sjm_status = "Active";
                                        scheduleMaterial.sjm_created_at = DateTime.Now;
                                        scheduleMaterial.sjm_quantity = materials[i2]["material_quantity"];
                                        await Schedule_Job_Material.Operations.Create(_Schedule_Context, scheduleMaterial);
                                    }
                                }
                            }

                            if (_schedule["workers"] != null)
                            {
                                var workers = _schedule["workers"];
                                if (workers.Count > 0)
                                {
                                    // clear all worker
                                    using (SqlConnection connection = new SqlConnection(_connStr))
                                    {
                                        // Creating SqlCommand objcet   
                                        SqlCommand cm = new SqlCommand("delete from Schedule_Job_Worker where sjw_schedule_job_id=@sjw_schedule_job_id", connection);
                                        cm.Parameters.AddWithValue("@sjw_schedule_job_id", old_schedule_job_id);
                                        // Opening Connection  
                                        connection.Open();
                                        // Executing the SQL query  
                                        cm.ExecuteNonQuery();
                                    } 

                                    Schedule_Job_Worker.Dto.Post scheduleWorker = new Schedule_Job_Worker.Dto.Post();
                                    for (int i3 = 0; i3 < workers.Count; i3++)
                                    {
                                        scheduleWorker = new Schedule_Job_Worker.Dto.Post();
                                        scheduleWorker.sjw_id = new Guid();
                                        scheduleWorker.sjw_worker_id = workers[i3]["user_id"];
                                        scheduleWorker.sjw_schedule_job_id = new_schedule_job_id;
                                        scheduleWorker.sjw_status = "Active";
                                        scheduleWorker.sjw_created_at = DateTime.Now;
                                        await Schedule_Job_Worker.Operations.Create(_Schedule_Context, scheduleWorker);
                                    }
                                }

                            }

                            /* string remarks = _schedule["remarks"].ToString();
                             Schedule_Job scheduleJob = _Schedule_Context.Schedule_Job.Where(x => x.schedule_job_id.Equals(schedule_job_id)).FirstOrDefault();
                             scheduleJob.schedule_job_remark = remarks;
                             await Schedule_Job.Operations.Update(_Schedule_Context, (Schedule_Job.Dto.Put)scheduleJob);*/

                        }

                    }
                }
            }
            return StatusCode(200, true); 
            /*bool status = await Schedule.Operations.Update(_Schedule_Context, scheduleScheme);

            if (status)
            {
                return StatusCode(200, scheduleScheme);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }*/
        }

        #endregion

        #region schedule job material
        [HttpGet]
        [Route("schedulejobmaterialswithdetails")]
        public async Task<IActionResult> getScheduleJobMaterialsWithDetails(string schedule_job_id)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select *,sjm_quantity as material_quantity from schedule_job_material inner join material on material_id=sjm_material_id where sjm_schedule_job_id=@schedule_job_id", connection);
                    cm.Parameters.AddWithValue("@schedule_job_id", schedule_job_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    List<dynamic> schedulejobmaterials = new List<dynamic>();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<dynamic>();
                            dynamic schedulejobmaterial = parser(sdr);
                            schedulejobmaterials.Add(schedulejobmaterial);
                        }
                    }
                    return new JsonResult(schedulejobmaterials);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }
        }
        [HttpPut]
        [Route("removeScheduleJobMaterial")]
        public async Task<IActionResult> removeScheduleJobMaterial(string sjm_id,string sjm_updated_by)
        {
            Schedule_Job_Material.Dto.Put scheduleJobMaterial = new Schedule_Job_Material.Dto.Put();
            scheduleJobMaterial.sjm_status = "Deleted";
            scheduleJobMaterial.sjm_id = Guid.Parse(sjm_id);
            scheduleJobMaterial.sjm_updated_at = DateTime.Now;
            scheduleJobMaterial.sjm_updated_by = sjm_updated_by;
            
            bool status = await Schedule_Job_Material.Operations.Update(_Schedule_Context, scheduleJobMaterial);

            if (status)
            {
                return StatusCode(200, scheduleJobMaterial);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        [HttpPost]
        [Route("addScheduleJobMaterial")]
        public async Task<IActionResult> addScheduleJobMaterial(Schedule_Job_Material.Dto.Post scheduleJobMaterial)
        {  
            bool status = await Schedule_Job_Material.Operations.Create(_Schedule_Context, scheduleJobMaterial);

            if (status)
            {
                return StatusCode(200, scheduleJobMaterial);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        #endregion
        #region schedule job tool
        [HttpGet]
        [Route("schedulejobtoolswithdetails")]
        public async Task<IActionResult> getScheduleJobToolsWithDetails(string schedule_job_id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select *, sjt_quantity as tool_quantity from schedule_job_tool inner join tool on tool_id=sjt_tool_id where sjt_schedule_job_id=@schedule_job_id", connection);
                    cm.Parameters.AddWithValue("@schedule_job_id", schedule_job_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    List<dynamic> schedulejobtools = new List<dynamic>();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<dynamic>();
                            dynamic schedulejobtool = parser(sdr);
                            schedulejobtools.Add(schedulejobtool);
                        }
                    }
                    return new JsonResult(schedulejobtools);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            } 
        }
        [HttpPut]
        [Route("removeScheduleJobTool")]
        public async Task<IActionResult> removeScheduleJobTool(string sjt_id, string sjt_updated_by)
        {
            Schedule_Job_Tool.Dto.Put scheduleJobTool = new Schedule_Job_Tool.Dto.Put();
            scheduleJobTool.sjt_status = "Deleted";
            scheduleJobTool.sjt_id = Guid.Parse(sjt_id);
            scheduleJobTool.sjt_updated_at = DateTime.Now;
            scheduleJobTool.sjt_updated_by = sjt_updated_by;

            bool status = await Schedule_Job_Tool.Operations.Update(_Schedule_Context, scheduleJobTool);

            if (status)
            {
                return StatusCode(200, scheduleJobTool);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        [HttpPost]
        [Route("addScheduleJobTool")]
        public async Task<IActionResult> addScheduleJobTool(Schedule_Job_Tool.Dto.Post scheduleJobTool)
        {
            bool status = await Schedule_Job_Tool.Operations.Create(_Schedule_Context, scheduleJobTool);

            if (status)
            {
                return StatusCode(200, scheduleJobTool);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }

        #endregion
        #region schedule job worker
        [HttpGet]
        [Route("schedulejobworkerswithdetails")]
        public async Task<IActionResult> getScheduleJobWorkersWithDetails(string schedule_job_id)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from schedule_job_worker inner join [user] on [user].user_id=sjw_worker_id where sjw_schedule_job_id=@schedule_job_id", connection);
                    cm.Parameters.AddWithValue("@schedule_job_id", schedule_job_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    List<dynamic> schedulejobworkers = new List<dynamic>();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<dynamic>();
                            dynamic schedulejobworker = parser(sdr);
                            schedulejobworkers.Add(schedulejobworker);
                        }
                    }
                    return new JsonResult(schedulejobworkers);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }
        }
        #endregion
        #region schedule job task
        [HttpGet]
        [Route("schedulejobtaskswithdetails")]
        public async Task<IActionResult> getScheduleJobTaskWithDetails(string schedule_job_id)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from job_task inner join schedule_job on job_task_job_id=schedule_job_job_id where job_task_status='Active' and schedule_job_id=@schedule_job_id", connection);
                    cm.Parameters.AddWithValue("@schedule_job_id", schedule_job_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    List<dynamic> schedulejobtasks = new List<dynamic>();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<dynamic>();
                            dynamic schedulejobtask = parser(sdr);
                            schedulejobtasks.Add(schedulejobtask);
                        }
                    }
                    return new JsonResult(schedulejobtasks);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }
        }
        #endregion

        [HttpGet]
        [Route("generateRoute")]
        public IActionResult generateRoute(string schedule_id, string generationType)
        { 
            try
            {
                #region get all active job
                List<Model.Job> jobs = new List<Model.Job>();
                List<Vehicle> vehicles = new List<Vehicle>();
                Dictionary<string, Dictionary<string, string>> carLastPoints= new Dictionary<string, Dictionary<string, string>>();
                Dictionary<string, List<string>> carJobs = new Dictionary<string, List<string>>();
                Dictionary<string, Guid> vehicleIDs = new Dictionary<string, Guid>();
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [job] where job_status='Active'", connection);

                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<Model.Job>(typeof(Model.Job));
                            Model.Job job = parser(sdr);
                            jobs.Add(job);
                        }
                    }
                }
                #endregion
                #region get all active vehicle
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [vehicle] where vehicle_status='Active'", connection);

                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<Vehicle>(typeof(Vehicle));
                            Vehicle vehicle = parser(sdr);
                            vehicles.Add(vehicle);
                        }
                    }
                }
                #endregion

                int maxJobTake = (jobs.Count / vehicles.Count) + 1;

                    
                //729908 office postal code
                string token = this.getOneMapToken();
                dynamic coords = this.getLongLat("729908");

                for (int i = 0; i < vehicles.Count; i++)
                {
                    Dictionary<string, string> carLastPoint = new Dictionary<string, string>();
                    if (!carLastPoints.ContainsKey(vehicles[i].vehicle_plat_no))
                    {
                        carLastPoint.Add("latitude", coords.Value.latitude);
                        carLastPoint.Add("longtitude", coords.Value.longtitude);
                        carLastPoints.Add(vehicles[i].vehicle_plat_no, carLastPoint);
                        vehicleIDs.Add(vehicles[i].vehicle_plat_no, vehicles[i].vehicle_id);
                    }
                }
                for (int k = 0; k < jobs.Count; k++)
                {
                    string jobPostalCode = jobs[k].job_postal_code;
                    dynamic jobCoords = this.getLongLat(jobPostalCode);
                    string endPoint = jobCoords.Value.latitude + "," + jobCoords.Value.longtitude;
                    float shortest = 0;
                    string winnerPlatNo = ""; 
                      
                    foreach (KeyValuePair<string, Dictionary<string, string>> car in carLastPoints)
                    {
                        string carPlat = car.Key;
                        if (carJobs.ContainsKey(carPlat) && carJobs[carPlat].Count > maxJobTake)
                        {
                            continue;
                        }
                            string longtitude = car.Value["longtitude"];
                            string latitude = car.Value["latitude"];

                            string startPoint = car.Value["latitude"] + "," + car.Value["longtitude"];
                            dynamic calculatedDistance = this.getSiteDistance(startPoint, endPoint, token, generationType);
                            if (shortest == 0 || (float.Parse(calculatedDistance.Value.distance) < shortest))
                            {
                                shortest = float.Parse(calculatedDistance.Value.distance);
                                winnerPlatNo = carPlat;
                            }
                        
                    }

                    if (carJobs.ContainsKey(winnerPlatNo))
                    {
                        List<string> carJob = carJobs[winnerPlatNo];
                        carJob.Add(jobs[k].job_id.ToString());
                        
                    }
                    else
                    {
                        List<string> carJob = new List<string> ();
                        carJob.Add(jobs[k].job_id.ToString());
                        carJobs.Add(winnerPlatNo, carJob);
                    }
                    Dictionary<string, string> carLastPoint = carLastPoints[winnerPlatNo];
                    carLastPoint["latitude"] = jobCoords.Value.latitude;
                    carLastPoint["longtitude"] = jobCoords.Value.longtitude;
                    carLastPoints[winnerPlatNo] = carLastPoint;

                }


                #region loop to insert job
                if (carJobs.Count > 0)
                {
                    DateTime currentDt = DateTime.Now;
                    foreach(KeyValuePair<string,List<string>> car in carJobs)
                    {
                        for (int z = 0; z < car.Value.Count; z++)
                        {
                            using (SqlConnection connection = new SqlConnection(_connStr))
                            {
                                // Creating SqlCommand objcet   
                                SqlCommand cm = new SqlCommand("insert into [schedule_job] " +
                                    "(schedule_job_id,schedule_job_schedule_id, schedule_job_job_id, schedule_job_order, schedule_job_vehicle_id, schedule_job_created_by, schedule_job_created_at) values " +
                                    "(NEWID(),@schedule_job_schedule_id, @schedule_job_job_id, @schedule_job_order, @schedule_job_vehicle_id, @schedule_job_created_by, @schedule_job_created_at)", connection);
                                 
                                cm.Parameters.AddWithValue("@schedule_job_schedule_id", schedule_id);
                                cm.Parameters.AddWithValue("@schedule_job_job_id", car.Value[z]);
                                cm.Parameters.AddWithValue("@schedule_job_order", z);
                                cm.Parameters.AddWithValue("@schedule_job_vehicle_id", vehicleIDs[car.Key]); 
                                cm.Parameters.AddWithValue("@schedule_job_created_at", currentDt);
                                cm.Parameters.AddWithValue("@schedule_job_created_by", "");

                                // Opening Connection  
                                connection.Open();
                                // Executing the SQL query  
                                int result = cm.ExecuteNonQuery();
                                if (result > 0)
                                {
                                    
                                }
                                else
                                {
                                    return new JsonResult("Error inserting");
                                }

                            }
                        }
                    }

                    Schedule schedule = _Schedule_Context.Schedule.Where(x => x.schedule_id.Equals(Guid.Parse(schedule_id))).FirstOrDefault();
                    schedule.schedule_status = "Completed";
                    schedule.schedule_updated_by = "";
                    schedule.schedule_updated_at = DateTime.Now;
                    _Schedule_Context.Update(schedule);
                    _Schedule_Context.SaveChanges();
                }
                return new JsonResult(carJobs);

                #endregion
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }

        }


        [HttpGet]
        [Route("getSiteDistance")]
        public IActionResult getSiteDistance(string startPoint, string endPoint, string token, string returnType)
        {
            // get user & Password
            try
            {
                var url = "https://developers.onemap.sg/privateapi/routingsvc/route";
                url += "?start=" + startPoint + "&end=" + endPoint + "&routeType=drive&token=" + token;

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);

                httpRequest.Accept = "application/json";


                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var jResult = JsonConvert.DeserializeObject<dynamic>(result);
                    if (returnType == "distance")
                    {
                        return new JsonResult(new { distance = jResult.route_summary.total_distance.ToString() });
                    }
                    else
                    {
                        return new JsonResult(new { time = jResult.route_summary.total_time.ToString() });
                    }
                }


            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }

        }
         
        private IActionResult getLongLat(string postalCode)
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
                        return new JsonResult(new
                        {
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

         
        private string getOneMapToken()
        {
            try
            {
                var url = "https://developers.onemap.sg/privateapi/auth/post/getToken";

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                var postData = "email=" + Uri.EscapeDataString("fuibuilderspl@gmail.com");
                postData += "&password=" + Uri.EscapeDataString("z3Kc2buYTZ");
                var data = Encoding.ASCII.GetBytes(postData);

                httpRequest.Method = "POST";

                httpRequest.ContentType = "application/x-www-form-urlencoded";
                httpRequest.ContentLength = data.Length;
                using (var stream = httpRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    var jResult = JsonConvert.DeserializeObject<dynamic>(result);
                    if (!string.IsNullOrEmpty(jResult.access_token.ToString()))
                    {
                        return jResult.access_token;
                    }
                    else
                    {
                        return "";
                    }

                }


            }
            catch (Exception e)
            {
                return "OOPs, something went wrong.\n" + e;
            }
        }


    }
}
