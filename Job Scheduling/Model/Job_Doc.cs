using System.ComponentModel.DataAnnotations;

namespace Job_Scheduling.Model
{
    public class Job_Doc
    {
        [Key]
        public Guid job_doc_id { get; set; }
        public int? job_doc_job_id { get; set; }
        public string job_doc_url { get; set; }
        public string job_doc_status { get; set; }
        public string job_doc_created_by { get; set; }
        public string job_doc_updated_by { get; set; }
        public DateTime? job_doc_created_at { get; set; }
        public DateTime? job_doc_updated_at { get; set; }
    }
}
