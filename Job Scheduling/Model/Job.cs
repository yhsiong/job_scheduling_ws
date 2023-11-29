using Job_Scheduling.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Job_Scheduling.Model
{
    public class Job
    {
        [Key]
        public Guid job_id { get; set; }
        public string job_no { get; set; }
        [Required]
        public string job_quotation_no { get; set; }
        public string? job_remark { get; set; }
        [Required]
        public string job_status { get; set; }
        [Required]
        public string job_postal_code { get; set; }
        [Required]
        public string job_longtitude { get; set; }
        [Required]
        public string job_latitude { get; set; }
        [Required]
        public string job_address { get; set; }
        [Required]
        public string job_customer_name { get; set; }
        [Required]
        public string job_customer_code { get; set; }
        [Required]
        public DateTime? job_start_date { get; set; }
        [Required]
        public DateTime? job_end_date { get; set; }
        [Required]
        public string job_entity_name { get; set; }
        
        public string? job_created_by { get; set; }
        public string? job_updated_by { get; set; }
        public DateTime? job_created_at { get; set; }
        public DateTime? job_updated_at { get; set; } 
        public string job_sales_agent { get; set; }
        public string job_customer_contact_name { get; set; }
        public string job_customer_contact_no { get; set; }
        //public string job_primary_staff { get; set; }

        [NotMapped]
        public class Dto
        {
            public class Get : Job
            {
                public string job_task_status { get; set; }
            }
            public class Post : Job
            { 
            }
            public class Put : Job
            { 
            }
        }

        [NotMapped]
        public class Operations
        {
            public static async Task<bool> Create(Job_Context job_Context, Dto.Post jobScheme)
            {
                job_Context.Job.Add(jobScheme);
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
            public static async Task<bool> Update(Job_Context job_Context, Dto.Put jobScheme)
            {
                Job dtoJob = job_Context.Job.Where(x => x.job_id.Equals(jobScheme.job_id)).FirstOrDefault();
                dtoJob.job_start_date = jobScheme.job_start_date;
                dtoJob.job_end_date = jobScheme.job_end_date;
                dtoJob.job_status = jobScheme.job_status;
                dtoJob.job_remark = jobScheme.job_remark;
                dtoJob.job_updated_at = jobScheme.job_updated_at;
                dtoJob.job_updated_by = jobScheme.job_updated_by;
                dtoJob.job_sales_agent = jobScheme.job_sales_agent;

                job_Context.Job.Update(dtoJob);
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
                List<Job> jobLists = job_Context.Job.Where(x => !x.job_status.Equals("Deleted")).ToList();

                if (jobLists == null)
                {
                    return null;
                }
                else
                { 
                    return jobLists.Select(job => new Dto.Get
                    {
                        job_address = job.job_address,
                        job_created_at = job.job_created_at,
                        job_created_by = job.job_created_by,
                        job_customer_code = job.job_customer_code,
                        job_customer_name = job.job_customer_name,
                        job_entity_name = job.job_entity_name,
                        job_id = job.job_id,
                        job_latitude = job.job_latitude,
                        job_longtitude = job.job_longtitude,
                        job_no = job.job_no,
                        job_postal_code = job.job_postal_code,
                        job_quotation_no = job.job_quotation_no,
                        job_remark = job.job_remark,
                        job_status=job.job_status,
                        job_updated_at=job.job_updated_at,
                        job_updated_by = job.job_updated_by,
                        job_start_date = job.job_start_date,
                        job_end_date = job.job_end_date,
                        job_customer_contact_no = job.job_customer_contact_no,
                        job_customer_contact_name = job.job_customer_contact_name,
                        job_sales_agent = job.job_sales_agent,
                        job_task_status = Operations.getTaskStatus(job_Context, job.job_id)

                    }).ToList();
                }
            }
            public static async Task<List<Dto.Get>> ReadAllActive(Job_Context job_Context)
            {
                List<Job> jobLists = job_Context.Job.Where(x => x.job_status.Equals("Active")).ToList();

                if (jobLists == null)
                {
                    return null;
                }
                else
                {
                    return jobLists.Select(job => new Dto.Get
                    {
                        job_address = job.job_address,
                        job_created_at = job.job_created_at,
                        job_created_by = job.job_created_by,
                        job_customer_code = job.job_customer_code,
                        job_customer_name = job.job_customer_name,
                        job_entity_name = job.job_entity_name,
                        job_id = job.job_id,
                        job_latitude = job.job_latitude,
                        job_longtitude = job.job_longtitude,
                        job_no = job.job_no,
                        job_postal_code = job.job_postal_code,
                        job_quotation_no = job.job_quotation_no,
                        job_remark = job.job_remark,
                        job_status = job.job_status,
                        job_updated_at = job.job_updated_at,
                        job_updated_by = job.job_updated_by,
                        job_start_date = job.job_start_date,
                        job_end_date = job.job_end_date,
                        job_customer_contact_no = job.job_customer_contact_no,
                        job_customer_contact_name = job.job_customer_contact_name,
                        job_sales_agent = job.job_sales_agent,
                        job_task_status = Operations.getTaskStatus(job_Context, job.job_id)

                    }).ToList();
                }
            }
            private static string getTaskStatus(Job_Context job_Context, Guid job_id)
            {
                Job_Task jobTask = job_Context.Job_Task.Where(x => x.job_task_status.Equals("Active") && x.job_task_job_id.Equals(job_id)).FirstOrDefault();
                if (jobTask != null && !string.IsNullOrEmpty(jobTask.job_task_description))
                {
                    return jobTask.job_task_description;
                }
                else {
                    return "";
                } 
            }
            public static async Task<Dto.Get> ReadSingleById(Job_Context job_Context, Guid job_id)
            {
                Job job = job_Context.Job.Where(a => a.job_id.Equals(job_id)).FirstOrDefault();

                if (job == null)
                {
                    return null;
                }
                else
                {
                    return new Dto.Get
                    {
                        job_address = job.job_address,
                        job_created_at = job.job_created_at,
                        job_created_by = job.job_created_by,
                        job_customer_code = job.job_customer_code,
                        job_customer_name = job.job_customer_name,
                        job_entity_name = job.job_entity_name,
                        job_id = job.job_id,
                        job_latitude = job.job_latitude,
                        job_longtitude = job.job_longtitude,
                        job_no = job.job_no,
                        job_postal_code = job.job_postal_code,
                        job_quotation_no = job.job_quotation_no,
                        job_remark = job.job_remark,
                        job_status = job.job_status,
                        job_updated_at = job.job_updated_at,
                        job_updated_by = job.job_updated_by,
                        job_start_date = job.job_start_date,
                        job_end_date = job.job_end_date,
                        job_customer_contact_no = job.job_customer_contact_no,
                        job_customer_contact_name = job.job_customer_contact_name,
                        job_sales_agent = job.job_sales_agent,
                        job_task_status = Operations.getTaskStatus(job_Context, job.job_id)
                    }; 
                }
            }
            public static async Task<Dto.Get> ReadSingleByNo(Job_Context job_Context, string job_no)
            {
                Job job = job_Context.Job.Where(x => x.job_no.Equals(job_no)).FirstOrDefault<Job>();

                if (job == null)
                {
                    return null;
                }
                else
                {
                    return new Dto.Get
                    {
                        job_address = job.job_address,
                        job_created_at = job.job_created_at,
                        job_created_by = job.job_created_by,
                        job_customer_code = job.job_customer_code,
                        job_customer_name = job.job_customer_name,
                        job_entity_name = job.job_entity_name,
                        job_id = job.job_id,
                        job_latitude = job.job_latitude,
                        job_longtitude = job.job_longtitude,
                        job_no = job.job_no,
                        job_postal_code = job.job_postal_code,
                        job_quotation_no = job.job_quotation_no,
                        job_remark = job.job_remark,
                        job_status = job.job_status,
                        job_updated_at = job.job_updated_at,
                        job_updated_by = job.job_updated_by,
                        job_start_date = job.job_start_date,
                        job_end_date = job.job_end_date,
                        job_customer_contact_no = job.job_customer_contact_no,
                        job_customer_contact_name = job.job_customer_contact_name,
                        job_sales_agent = job.job_sales_agent,
                        job_task_status = Operations.getTaskStatus(job_Context, job.job_id)
                    };
                }
            }
            public static async Task<string> generateJobNo(Job_Context job_Context)
            {
                string jobNoPrefix = "JOB-";
                int formatLenght = 5;
                int last_id = 1;
                string generatedJobNo = string.Empty;
                Job job = job_Context.Job.OrderByDescending(x => x.job_created_at).FirstOrDefault<Job>();


                if (job != null)
                {
                    string[] last_job_no = job.job_no.Split('-');
                    last_id = int.Parse(last_job_no[1]);
                    last_id = last_id + 1;
                }

                generatedJobNo = jobNoPrefix + string.Format("{0}", last_id.ToString().PadLeft(formatLenght, '0'));
                
                return generatedJobNo;
            }
        }
    }
}
