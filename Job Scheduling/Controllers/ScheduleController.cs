using Dapper;
using Job_Scheduling.Database;
using Job_Scheduling.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net;
using System.Text;

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
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from[Schedule_job] inner join vehicle on vehicle_id =[Schedule_job].schedule_job_vehicle_id " +
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
        public async Task<IActionResult> updateScheduleJob(string schedule_id, string jsonSchedule)
        {
            return StatusCode(404, string.Format("Could not find config"));
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
            List<Schedule_Job.Dto.Get> scheduleJobs = await Schedule_Job.Operations.ReadSingleByScheduleId(_Schedule_Context,Guid.Parse(schedule_id));
            _Schedule_Context.Schedule_Job.RemoveRange(scheduleJobs);
            _Schedule_Context.SaveChanges();

            Schedule_Job.Dto.Post newScheduleJob = new Schedule_Job.Dto.Post();
            foreach(KeyValuePair<string,List<dynamic>> schedule in schedules)
            {
                newScheduleJob = new Schedule_Job.Dto.Post();
               
                Vehicle.Dto.Get vehicle = await Vehicle.Operations.ReadSingleByPlatNo(_Vehicle_Context, schedule.Key);
                newScheduleJob.schedule_job_vehicle_id = vehicle.vehicle_id;
                newScheduleJob.schedule_job_schedule_id = Guid.Parse(schedule_id);
                newScheduleJob.schedule_job_created_at = DateTime.Now;
                
                for (int i = 0; i < schedule.Value.Count(); i++)
                {
                    newScheduleJob.schedule_job_id = new Guid();
                    newScheduleJob.schedule_job_job_id = schedule.Value[i].job_id;
                    newScheduleJob.schedule_job_order = i;
                    
                    await Schedule_Job.Operations.Create(_Schedule_Context, newScheduleJob);

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
        [Route("schedulejobmaterials")]
        public async Task<IActionResult> getScheduleJobMaterials(string schedule_job_id)
        {
            List<Schedule_Job_Material.Dto.Get> scheduleJobMaterials = await Schedule_Job_Material.Operations.ReadByScheduleJobId(_Schedule_Context, Guid.Parse(schedule_job_id));
            if (scheduleJobMaterials == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, scheduleJobMaterials);
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
        [Route("schedulejobtools")]
        public async Task<IActionResult> getScheduleJobTools(string schedule_job_id)
        {
            List<Schedule_Job_Tool.Dto.Get> scheduleJobTools= await Schedule_Job_Tool.Operations.ReadSingleByScheduleJobId(_Schedule_Context, Guid.Parse(schedule_job_id));
            if (scheduleJobTools == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, scheduleJobTools);
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
        [HttpGet]
        [Route("generateRoute")]
        public IActionResult generateRoute(string schedule_id, string generationType)
        { 
            try
            {
                #region get all active job
                List<Job> jobs = new List<Job>();
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
                            var parser = sdr.GetRowParser<Job>(typeof(Job));
                            Job job = parser(sdr);
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
