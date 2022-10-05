using Dapper;
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
        private string _connStr = string.Empty;
        private readonly ILogger<ScheduleController> _logger;
        public ScheduleController(ILogger<ScheduleController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
        }
        [HttpGet]
        [Route("schedules")]
        public IActionResult getSchedules()
        {
            // get user & Password
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [Schedule]", connection);

                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    List<Schedule> schedules = new List<Schedule>();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<Schedule>(typeof(Schedule));
                            Schedule schedule = parser(sdr);
                            schedules.Add(schedule);
                        }
                    }
                    return new JsonResult(schedules);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }

        }


        [HttpGet]
        [Route("schedulejobs")]
        public IActionResult getScheduleJobs(string schedule_id)
        {
            // get user & Password
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [Schedule_job] where schedule_job_schedule_id=@schedule_job_schedule_id", connection);
                    cm.Parameters.AddWithValue("@schedule_job_schedule_id", schedule_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    List<Schedule_Job> schedulejobs = new List<Schedule_Job>();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<Schedule_Job>(typeof(Schedule_Job));
                            Schedule_Job schedulejob = parser(sdr);
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

        [HttpGet]
        [Route("schedule")]
        public IActionResult getSchedule(string schedule_id)
        {
            // get user & Password
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [schedule] where schedule_id=@schedule_id", connection);
                    cm.Parameters.AddWithValue("@schedule_id", schedule_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    Schedule schedule = new Schedule();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<Schedule>(typeof(Schedule));
                            schedule = parser(sdr);
                        }
                    }
                    return new JsonResult(schedule);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }

        }
        [HttpPost]
        [Route("schedule")]
        public IActionResult insertSchedule(Schedule schedule)
        {
            schedule.schedule_created_at = DateTime.Now;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("insert into [schedule] " +
                        "(schedule_date,schedule_remark,schedule_status,schedule_created_by,schedule_created_at) values " +
                        "(@schedule_date,@schedule_remark,@schedule_status,@schedule_created_by,@schedule_created_at)", connection);

                    cm.Parameters.AddWithValue("@schedule_date", schedule.schedule_date);
                    cm.Parameters.AddWithValue("@schedule_remark", schedule.schedule_remark);
                    cm.Parameters.AddWithValue("@schedule_status", schedule.schedule_status);
                    cm.Parameters.AddWithValue("@schedule_created_by", schedule.schedule_created_by);
                    cm.Parameters.AddWithValue("@schedule_created_at", schedule.schedule_created_at); 


                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    int result = cm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        return new JsonResult(schedule);
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
        [HttpPut]
        [Route("schedule")]
        public IActionResult updateSchedule(string schedule_id, string schedule_remark, string schedule_status)   
        {
            Schedule schedule = new Schedule();
            schedule.schedule_id = int.Parse(schedule_id);
            schedule.schedule_remark = schedule_remark;
            schedule.schedule_status = schedule_status; 
            schedule.schedule_updated_at = DateTime.Now;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("update [schedule] set schedule_status=@schedule_status,schedule_remark=@schedule_remark," +
                    "schedule_updated_by=@schedule_updated_by,schedule_updated_at=@schedule_updated_at where schedule_id=@schedule_id", connection);

                    cm.Parameters.AddWithValue("@schedule_id", schedule.schedule_id);
                    cm.Parameters.AddWithValue("@schedule_remark", schedule.schedule_remark);
                    cm.Parameters.AddWithValue("@schedule_status", schedule.schedule_status);
                    cm.Parameters.AddWithValue("@schedule_updated_by", schedule.schedule_updated_by);
                    cm.Parameters.AddWithValue("@schedule_updated_at", schedule.schedule_updated_at);


                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    int result = cm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        return new JsonResult(schedule);
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
        [Route("generateRoute")]
        public IActionResult generateRoute(string schedule_id, string generationType)
        {
            // get list of active job
            // get list of car
            // loop trhough the car
                // loop the job
                    // check car distance to job, shortest distance/time win
            
            // return car object with task

            try
            {
                #region get all active job
                List<Job> jobs = new List<Job>();
                List<Vehicle> vehicles = new List<Vehicle>();
                Dictionary<string, Dictionary<string, string>> carLastPoints= new Dictionary<string, Dictionary<string, string>>();
                Dictionary<string, List<string>> carJobs = new Dictionary<string, List<string>>();
                Dictionary<string, int?> vehicleIDs = new Dictionary<string, int?>();
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
                                    "(schedule_job_schedule_id, schedule_job_job_id, schedule_job_order, schedule_job_vehicle_id, schedule_job_created_by, schedule_job_created_at) values " +
                                    "(@schedule_job_schedule_id, @schedule_job_job_id, @schedule_job_order, @schedule_job_vehicle_id, @schedule_job_created_by, @schedule_job_created_at)", connection);
                                 
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
