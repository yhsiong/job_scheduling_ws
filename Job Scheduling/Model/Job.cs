namespace Job_Scheduling.Model
{
    public class Job
    {
        public int? job_id { get; set; }
        public string job_no { get; set; }
        public string job_quotation_no { get; set; }
        public string job_remark { get; set; }
        public string job_status { get; set; }
        public string job_postal_code { get; set; }
        public string job_address { get; set; }
        public string job_customer_name { get; set; }
        public string job_customer_code { get; set; } 
        public string job_created_by { get; set; }
        public string job_updated_by { get; set; }
        public DateTime? job_created_at { get; set; }
        public DateTime? job_updated_at { get; set; }
    }
}
