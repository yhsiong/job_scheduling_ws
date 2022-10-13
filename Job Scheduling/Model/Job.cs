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

        [NotMapped]
        public class Dto
        {
            public class Get : Job
            {
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
                job_Context.Job.Update(jobScheme);
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
                List<Job> jobLists = job_Context.Job.ToList();

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
                        job_updated_by = job.job_updated_by

                    }).ToList();
                }
            }
            public static async Task<Dto.Get> ReadSingleById(Job_Context job_Context, Guid job_id)
            {
                List<Job> jobs = job_Context.Job.Where(a => a.job_id.Equals(job_id)).ToList();
                Job job = new Job();

                if (jobs == null)
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
                        job_updated_by = job.job_updated_by
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
                        job_updated_by = job.job_updated_by
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
