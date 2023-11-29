using Dapper;
using Job_Scheduling.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using Z.EntityFramework.Plus;

namespace Job_Scheduling.Model
{
    public class Schedule_Job_Worker
    {
        [Key]
        public Guid sjw_id { get; set; }
        public Guid sjw_schedule_job_id { get; set; }
        public Guid sjw_worker_id { get; set; }
        public string? sjw_created_by { get; set; }
        public DateTime? sjw_created_at { get; set; }
        public string? sjw_updated_by { get; set; }
        public DateTime? sjw_updated_at { get; set; }
        public string? sjw_status { get; set; } 

        [NotMapped]
        public class Dto
        {
            public class Get : Schedule_Job_Worker
            {
            }
            public class Post : Schedule_Job_Worker
            {
            }
            public class Put : Schedule_Job_Worker
            {
            }
        }

        [NotMapped]
        public class Operations
        {
            public static async Task<bool> Create(Schedule_Context schedule_Context, Dto.Post scheduleJobWorkerScheme)
            {
                schedule_Context.Schedule_Job_Worker.Add(scheduleJobWorkerScheme);
                try
                {
                    await schedule_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            public static async Task<bool> Update(Schedule_Context schedule_Context, Dto.Put scheduleJobWorkerScheme)
            {
                Schedule_Job_Worker dtoScheduleJobWorker = schedule_Context.Schedule_Job_Worker.Where(x => x.sjw_id.Equals(scheduleJobWorkerScheme.sjw_id)).FirstOrDefault();
                dtoScheduleJobWorker.sjw_id = scheduleJobWorkerScheme.sjw_id;
                dtoScheduleJobWorker.sjw_updated_by = scheduleJobWorkerScheme.sjw_updated_by;
                dtoScheduleJobWorker.sjw_status = scheduleJobWorkerScheme.sjw_status;
                dtoScheduleJobWorker.sjw_created_by = scheduleJobWorkerScheme.sjw_created_by;
                dtoScheduleJobWorker.sjw_updated_at = scheduleJobWorkerScheme.sjw_updated_at;
                dtoScheduleJobWorker.sjw_created_at = scheduleJobWorkerScheme.sjw_created_at;
                dtoScheduleJobWorker.sjw_worker_id = scheduleJobWorkerScheme.sjw_worker_id;
                dtoScheduleJobWorker.sjw_schedule_job_id = scheduleJobWorkerScheme.sjw_schedule_job_id;

                schedule_Context.Schedule_Job_Worker.Update(dtoScheduleJobWorker);
                try
                {
                    await schedule_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            public static async Task<List<Dto.Get>> ReadByScheduleJobId(Schedule_Context schedule_Context, Guid sjw_schedule_job_id)
            {
                List<Schedule_Job_Worker> scheduleJobWorkers = schedule_Context.Schedule_Job_Worker.Where(a => a.sjw_schedule_job_id.Equals(sjw_schedule_job_id)).ToList();

                if (scheduleJobWorkers == null)
                {
                    return null;
                }
                else
                {
                    return scheduleJobWorkers.Select(scheduleJobWorker=> new Dto.Get
                    {
                        sjw_id = scheduleJobWorker.sjw_id,
                        sjw_created_at = scheduleJobWorker.sjw_created_at,
                        sjw_created_by = scheduleJobWorker.sjw_created_by,
                        sjw_worker_id = scheduleJobWorker.sjw_worker_id,
                        sjw_schedule_job_id = scheduleJobWorker.sjw_schedule_job_id,
                        sjw_updated_at = scheduleJobWorker.sjw_updated_at,
                        sjw_status = scheduleJobWorker.sjw_status,
                        sjw_updated_by = scheduleJobWorker.sjw_updated_by
                    }).ToList();
                     
                }
            }
            public static async void removeLines(string connString, string schedule_job_id)
            {
                using (SqlConnection connection = new SqlConnection(connString))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("delete from Schedule_Job_Worker where sjw_schedule_job_id=@sjw_schedule_job_id", connection);
                    cm.Parameters.AddWithValue("@sjw_schedule_job_id", schedule_job_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    cm.ExecuteNonQuery();
                    connection.Close();
                }
            }
            public static async Task<List<dynamic>> ReadScheduleJobWorkersCustom(string connString, string schedule_job_id)
            {
                List<dynamic> schedulejobworkers = new List<dynamic>();

                using (SqlConnection connection = new SqlConnection(connString))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from schedule_job_worker inner join [user] on [user].user_id=sjw_worker_id where sjw_schedule_job_id=@schedule_job_id", connection);
                    cm.Parameters.AddWithValue("@schedule_job_id", schedule_job_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();

                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<dynamic>();
                            dynamic schedulejobworker = parser(sdr);
                            schedulejobworkers.Add(schedulejobworker);
                        }
                    }
                }
                return schedulejobworkers;
            }
        }
    }
}
