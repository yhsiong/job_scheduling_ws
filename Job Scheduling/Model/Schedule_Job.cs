using Dapper;
using Job_Scheduling.Database;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;

namespace Job_Scheduling.Model
{
    public class Schedule_Job
    {
        [Key]
        public Guid schedule_job_id { get; set; }
        public Guid schedule_job_schedule_id { get; set; }
        public Guid schedule_job_job_id { get; set; }
        public Int64 schedule_job_order { get; set; }
        public Guid schedule_job_vehicle_id { get; set; }
        public string? schedule_job_remark { get; set; }
        public string? schedule_job_created_by { get; set; }
        public string? schedule_job_updated_by { get; set; }
        public DateTime? schedule_job_created_at { get; set; }
        public DateTime? schedule_job_updated_at { get; set; }

        [NotMapped]
        public class Dto
        {
            public class Get : Schedule_Job
            {
            }
            public class Post : Schedule_Job
            {
            }
            public class Put : Schedule_Job
            {
            }
        }

        [NotMapped]
        public class Operations
        {
            public static async Task<bool> Create(Schedule_Context schedule_Context, Dto.Post scheduleJobScheme)
            {
                schedule_Context.Schedule_Job.Add(scheduleJobScheme);
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
            public static async Task<bool> Update(Schedule_Context schedule_Context, Dto.Put scheduleJobScheme)
            {
                Schedule_Job dtoScheduleJob = schedule_Context.Schedule_Job.Where(x => x.schedule_job_id.Equals(scheduleJobScheme.schedule_job_id)).FirstOrDefault();
                dtoScheduleJob.schedule_job_order = scheduleJobScheme.schedule_job_order;
                dtoScheduleJob.schedule_job_updated_by = scheduleJobScheme.schedule_job_updated_by;
                dtoScheduleJob.schedule_job_updated_at = scheduleJobScheme.schedule_job_updated_at;
                dtoScheduleJob.schedule_job_vehicle_id = scheduleJobScheme.schedule_job_vehicle_id;
                dtoScheduleJob.schedule_job_remark = scheduleJobScheme.schedule_job_remark;

                schedule_Context.Schedule_Job.Update(dtoScheduleJob);
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
            public static async Task<Dto.Get> ReadSingleById(Schedule_Context schedule_Context, Guid schedule_job_id)
            {
                Schedule_Job scheduleJob = schedule_Context.Schedule_Job.Where(a => a.schedule_job_id.Equals(schedule_job_id)).FirstOrDefault();

                if (scheduleJob == null)
                {
                    return null;
                }
                else
                {
                    return new Dto.Get
                    {
                        schedule_job_id = scheduleJob.schedule_job_id,
                        schedule_job_created_at = scheduleJob.schedule_job_created_at,
                        schedule_job_created_by = scheduleJob.schedule_job_created_by,
                        schedule_job_job_id = scheduleJob.schedule_job_job_id,
                        schedule_job_order = scheduleJob.schedule_job_order,
                        schedule_job_schedule_id = scheduleJob.schedule_job_job_id,
                        schedule_job_updated_at = scheduleJob.schedule_job_updated_at,
                        schedule_job_updated_by = scheduleJob.schedule_job_updated_by,
                        schedule_job_vehicle_id = scheduleJob.schedule_job_vehicle_id,
                        schedule_job_remark = scheduleJob.schedule_job_remark
                };
                }
            }
            public static async Task<List<Dto.Get>> ReadSingleByScheduleId(Schedule_Context schedule_Context, Guid schedule_id)
            {
                List<Schedule_Job> scheduleJobs = schedule_Context.Schedule_Job.Where(a => a.schedule_job_schedule_id.Equals(schedule_id)).ToList();

                if (scheduleJobs == null)
                {
                    return null;
                }
                else
                {
                    return scheduleJobs.Select(scheduleJob => new Dto.Get
                    {
                        schedule_job_id = scheduleJob.schedule_job_id,
                        schedule_job_created_at = scheduleJob.schedule_job_created_at,
                        schedule_job_created_by = scheduleJob.schedule_job_created_by,
                        schedule_job_job_id = scheduleJob.schedule_job_job_id,
                        schedule_job_order = scheduleJob.schedule_job_order,
                        schedule_job_schedule_id = scheduleJob.schedule_job_job_id,
                        schedule_job_updated_at = scheduleJob.schedule_job_updated_at,
                        schedule_job_updated_by = scheduleJob.schedule_job_updated_by,
                        schedule_job_vehicle_id = scheduleJob.schedule_job_vehicle_id,
                        schedule_job_remark = scheduleJob.schedule_job_remark
                    }).ToList();

                }
            }
            public static async Task<List<dynamic>> ReadScheduleJobDetailsCustom(string connString, string schedule_id)
            {
                List<dynamic> schedulejobs = new List<dynamic>();

                using (SqlConnection connection = new SqlConnection(connString))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from[Schedule_job] right join vehicle on vehicle_id =[Schedule_job].schedule_job_vehicle_id " +
                    " inner join job on job_id =[Schedule_job].schedule_job_job_id where schedule_job_schedule_id=@schedule_job_schedule_id", connection);
                    cm.Parameters.AddWithValue("@schedule_job_schedule_id", schedule_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    
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
                }
                return schedulejobs;
            }
            public static async Task<List<dynamic>> ReadScheduleJobTasksCustom(string connString, string schedule_job_id)
            {
                List<dynamic> schedulejobtasks = new List<dynamic>();

                using (SqlConnection connection = new SqlConnection(connString))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from job_task inner join schedule_job on job_task_job_id=schedule_job_job_id where job_task_status='Active' and schedule_job_id=@schedule_job_id", connection);
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
                            dynamic schedulejobtask = parser(sdr);
                            schedulejobtasks.Add(schedulejobtask);
                        }
                    }
                    connection.Close(); 
                }
                return schedulejobtasks;
            }
        }
    }
}
