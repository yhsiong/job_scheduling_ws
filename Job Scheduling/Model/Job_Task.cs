using Job_Scheduling.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Job_Scheduling.Model
{
    public class Job_Task
    {
        [Key]
        public Guid job_task_id { get; set; }
        public Guid job_task_job_id { get; set; }
        public string job_task_description { get; set; }
        public string job_task_status { get; set; }
        public string? job_task_remark { get; set; }
        public string? job_task_created_by { get; set; }
        public string? job_task_updated_by { get; set; }
        public DateTime? job_task_created_at { get; set; }
        public DateTime? job_task_updated_at { get; set; }

        [NotMapped]
        public class Dto
        {
            public class Get : Job_Task
            {
            }
            public class Post : Job_Task
            {
            }
            public class Put : Job_Task
            {
            }
        }


        [NotMapped]
        public class Operations
        {
            public static async Task<bool> Create(Job_Context job_Context, Dto.Post jobTaskScheme)
            {
                job_Context.Job_Task.Add(jobTaskScheme);
                try
                {
                    await job_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            public static async Task<bool> Update(Job_Context job_Context, Dto.Put jobTaskScheme)
            {
                Job_Task dtoJobTask = job_Context.Job_Task.Where(x => x.job_task_id.Equals(jobTaskScheme.job_task_id)).FirstOrDefault();
                dtoJobTask.job_task_status = jobTaskScheme.job_task_status;
                dtoJobTask.job_task_updated_by = jobTaskScheme.job_task_updated_by;
                dtoJobTask.job_task_updated_at = jobTaskScheme.job_task_updated_at;
                dtoJobTask.job_task_remark = jobTaskScheme.job_task_remark;
                
                job_Context.Job_Task.Update(dtoJobTask);
                try
                {
                    await job_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            public static async Task<List<Dto.Get>> ReadAll(Job_Context job_Context)
            {
                List<Job_Task> jobTaskLists = job_Context.Job_Task.Where(x => !x.job_task_status.Equals("Deleted")).ToList();

                if (jobTaskLists == null)
                {
                    return null;
                }
                else
                {
                    return jobTaskLists.Select(jobTask => new Dto.Get
                    {
                        job_task_created_at = jobTask.job_task_created_at,
                        job_task_created_by = jobTask.job_task_created_by,
                        job_task_description = jobTask.job_task_description,
                        job_task_id = jobTask.job_task_id,
                        job_task_job_id = jobTask.job_task_job_id,
                        job_task_status = jobTask.job_task_status,
                        job_task_updated_at = jobTask.job_task_updated_at,
                        job_task_updated_by = jobTask.job_task_updated_by
                    }).ToList();
                }
            }
            public static async Task<Dto.Get> ReadSingleById(Job_Context job_Context, Guid job_task_id)
            {
                Job_Task jobTask = job_Context.Job_Task.Where(x => x.job_task_id.Equals(job_task_id)).FirstOrDefault<Job_Task>();

                if (jobTask == null)
                {
                    return null;
                }
                else
                {
                    return new Dto.Get
                    {
                        job_task_created_at = jobTask.job_task_created_at,
                        job_task_created_by = jobTask.job_task_created_by,
                        job_task_description = jobTask.job_task_description,
                        job_task_id = jobTask.job_task_id,
                        job_task_job_id = jobTask.job_task_job_id,
                        job_task_status = jobTask.job_task_status,
                        job_task_updated_at = jobTask.job_task_updated_at,
                        job_task_updated_by = jobTask.job_task_updated_by
                    };
                }
            }
            public static async Task<List<Dto.Get>> ReadSingleByJobId(Job_Context job_Context, Guid job_task_job_id)
            {
                List<Job_Task> jobTasks = job_Context.Job_Task.Where(x => x.job_task_job_id.Equals(job_task_job_id)).ToList();

                if (jobTasks == null)
                {
                    return null;
                }
                else
                {
                    return jobTasks.Select(jobTask => new Dto.Get
                    {
                        job_task_created_at = jobTask.job_task_created_at,
                        job_task_created_by = jobTask.job_task_created_by,
                        job_task_description = jobTask.job_task_description,
                        job_task_id = jobTask.job_task_id,
                        job_task_job_id = jobTask.job_task_job_id,
                        job_task_status = jobTask.job_task_status,
                        job_task_updated_at = jobTask.job_task_updated_at,
                        job_task_updated_by = jobTask.job_task_updated_by
                    }).ToList();
                }
            }

        }
    }
}
