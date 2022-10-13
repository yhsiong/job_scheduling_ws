using Job_Scheduling.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Job_Scheduling.Model
{
    public class Schedule_Job
    {
        [Key]
        public Guid schedule_job_id { get; set; }
        public Guid schedule_job_schedule_id { get; set; }
        public Guid schedule_job_job_id { get; set; }
        public Guid schedule_job_order { get; set; }
        public Guid schedule_job_vehicle_id { get; set; }
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
                        schedule_job_vehicle_id = scheduleJob.schedule_job_vehicle_id
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
                        schedule_job_vehicle_id = scheduleJob.schedule_job_vehicle_id
                    }).ToList();

                }
            }
        }
    }
}
