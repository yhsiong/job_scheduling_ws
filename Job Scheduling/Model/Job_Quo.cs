namespace Job_Scheduling.Model
{
    public class Job_Quo
    {
        public int? job_quo_id { get; set; }
        public int? job_quo_job_id { get; set; }
        public string job_quo_quotation_no { get; set; }
        public string job_quo_status { get; set; }
        public string job_quo_created_by { get; set; }
        public string job_quo_updated_by { get; set; }
        public DateTime? job_quo_created_at { get; set; }
        public DateTime? job_quo_updated_at { get; set; }
    }
}
