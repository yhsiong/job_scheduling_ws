using Dapper;
using Job_Scheduling.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

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
            schedule.schedule_created_by = HttpContext.Session.GetString("userId");
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
            schedule.schedule_updated_by = HttpContext.Session.GetString("userId");
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

    }
}
