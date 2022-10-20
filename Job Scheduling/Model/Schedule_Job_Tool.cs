using Job_Scheduling.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Job_Scheduling.Model
{
    public class Schedule_Job_Tool
    {
        [Key]
        public Guid sjt_id { get; set; }
        public Guid sjt_schedule_job_id { get; set; }
        public Guid sjt_tool_id { get; set; } 
        public string? sjt_created_by { get; set; }
        public DateTime? sjt_created_at { get; set; }
        public string? sjt_updated_by { get; set; }
        public DateTime? sjt_updated_at { get; set; }
        public string? sjt_status { get; set; } 

        [NotMapped]
        public class Dto
        {
            public class Get : Schedule_Job_Tool
            {
            }
            public class Post : Schedule_Job_Tool
            {
            }
            public class Put : Schedule_Job_Tool
            {
            }
        }

        [NotMapped]
        public class Operations
        {
            public static async Task<bool> Create(Schedule_Context schedule_Context, Dto.Post scheduleJobToolScheme)
            {
                schedule_Context.Schedule_Job_Tool.Add(scheduleJobToolScheme);
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
            public static async Task<bool> Update(Schedule_Context schedule_Context, Dto.Put scheduleJobToolScheme)
            {
                Schedule_Job_Tool dtoScheduleJobTool = schedule_Context.Schedule_Job_Tool.Where(x => x.sjt_id.Equals(scheduleJobToolScheme.sjt_id)).FirstOrDefault();
                dtoScheduleJobTool.sjt_id = scheduleJobToolScheme.sjt_id;
                dtoScheduleJobTool.sjt_updated_by = scheduleJobToolScheme.sjt_updated_by;
                dtoScheduleJobTool.sjt_status = scheduleJobToolScheme.sjt_status;
                dtoScheduleJobTool.sjt_created_by = scheduleJobToolScheme.sjt_created_by;
                dtoScheduleJobTool.sjt_updated_at = scheduleJobToolScheme.sjt_updated_at;
                dtoScheduleJobTool.sjt_created_at = scheduleJobToolScheme.sjt_created_at;
                dtoScheduleJobTool.sjt_tool_id = scheduleJobToolScheme.sjt_tool_id;
                dtoScheduleJobTool.sjt_schedule_job_id = scheduleJobToolScheme.sjt_schedule_job_id;

                schedule_Context.Schedule_Job_Tool.Update(dtoScheduleJobTool);
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
            public static async Task<Dto.Get> ReadSingleById(Schedule_Context schedule_Context, Guid sjt_id)
            {
                Schedule_Job_Tool schedule_Job_Tool = schedule_Context.Schedule_Job_Tool.Where(a => a.sjt_id.Equals(sjt_id)).FirstOrDefault();

                if (schedule_Job_Tool == null)
                {
                    return null;
                }
                else
                {
                    return new Dto.Get
                    {
                        sjt_id = schedule_Job_Tool.sjt_id,
                        sjt_created_at = schedule_Job_Tool.sjt_created_at,
                        sjt_created_by = schedule_Job_Tool.sjt_created_by,
                        sjt_tool_id = schedule_Job_Tool.sjt_tool_id,
                        sjt_schedule_job_id = schedule_Job_Tool.sjt_schedule_job_id,
                        sjt_updated_at = schedule_Job_Tool.sjt_updated_at,
                        sjt_status = schedule_Job_Tool.sjt_status,
                        sjt_updated_by = schedule_Job_Tool.sjt_updated_by 
                    };
                }
            }
            public static async Task<List<Dto.Get>> ReadSingleByScheduleJobId(Schedule_Context schedule_Context, Guid sjt_schedule_job_id)
            {
                List<Schedule_Job_Tool> scheduleJobTools = schedule_Context.Schedule_Job_Tool.Where(a => a.sjt_schedule_job_id.Equals(sjt_schedule_job_id)).ToList();

                if (scheduleJobTools == null)
                {
                    return null;
                }
                else
                {
                    return scheduleJobTools.Select(scheduleJobTool => new Dto.Get
                    {
                        sjt_id = scheduleJobTool.sjt_id,
                        sjt_created_at = scheduleJobTool.sjt_created_at,
                        sjt_created_by = scheduleJobTool.sjt_created_by,
                        sjt_tool_id = scheduleJobTool.sjt_tool_id,
                        sjt_schedule_job_id = scheduleJobTool.sjt_schedule_job_id,
                        sjt_updated_at = scheduleJobTool.sjt_updated_at,
                        sjt_status = scheduleJobTool.sjt_status,
                        sjt_updated_by = scheduleJobTool.sjt_updated_by
                    }).ToList(); 
                }
            }
        }
    }
}
