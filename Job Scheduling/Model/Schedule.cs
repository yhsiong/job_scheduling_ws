using Job_Scheduling.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Job_Scheduling.Model
{
    public class Schedule
    {
        [Key]
        public Guid schedule_id { get; set; } 
        public string schedule_date { get; set; }
        public string? schedule_remark { get; set; }
        public string schedule_status { get; set; }
        public string? schedule_created_by { get; set; }
        public string? schedule_updated_by { get; set; } 
        public DateTime? schedule_created_at { get; set; }
        public DateTime? schedule_updated_at { get; set; }

        [NotMapped]
        public class Dto
        {
            public class Get : Schedule
            {
            }
            public class Post : Schedule
            {
            }
            public class Put : Schedule
            {
            }
        }

        [NotMapped]
        public class Operations
        {
            public static async Task<bool> Create(Schedule_Context schedule_Context, Dto.Post scheduleScheme)
            {
                schedule_Context.Schedule.Add(scheduleScheme);
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
            public static async Task<bool> Update(Schedule_Context schedule_Context, Dto.Put scheduleScheme)
            {
                Schedule dtoSchedule = schedule_Context.Schedule.Where(x => x.schedule_id.Equals(scheduleScheme.schedule_id)).FirstOrDefault();
                dtoSchedule.schedule_status = scheduleScheme.schedule_status;
                dtoSchedule.schedule_updated_by = scheduleScheme.schedule_updated_by;
                dtoSchedule.schedule_updated_at = scheduleScheme.schedule_updated_at;
                dtoSchedule.schedule_remark = scheduleScheme.schedule_remark;

                schedule_Context.Schedule.Update(dtoSchedule);
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

            public static async Task<List<Dto.Get>> ReadAll(Schedule_Context schedule_Context)
            {
                List<Schedule> scheduleLists = schedule_Context.Schedule.ToList();

                if (scheduleLists == null)
                {
                    return null;
                }
                else
                {
                    return scheduleLists.Select(schedule => new Dto.Get
                    {
                        schedule_id = schedule.schedule_id,
                        schedule_updated_at = schedule.schedule_updated_at,
                        schedule_created_at = schedule.schedule_created_at,
                        schedule_created_by = schedule.schedule_created_by,
                        schedule_updated_by = schedule.schedule_updated_by,
                        schedule_status = schedule.schedule_status,
                    }).ToList();
                }
            }
            public static async Task<Dto.Get> ReadSingleById(Schedule_Context schedule_Context, Guid schedule_id)
            {
                Schedule schedule = schedule_Context.Schedule.Where(a => a.schedule_id.Equals(schedule_id)).FirstOrDefault();
                
                if (schedule == null)
                {
                    return null;
                }
                else
                {
                    return new Dto.Get
                    {
                        schedule_id = schedule.schedule_id,
                        schedule_updated_at = schedule.schedule_updated_at,
                        schedule_created_at = schedule.schedule_created_at,
                        schedule_created_by = schedule.schedule_created_by,
                        schedule_updated_by = schedule.schedule_updated_by,
                        schedule_status = schedule.schedule_status,
                    };
                }
            } 
        }
    }
}
