namespace Job_Scheduling.Model
{
    public class Schedule_Job
    {
        public int? schedule_job_id { get; set; }
        public int? schedule_job_schedule_id { get; set; }
        public int? schedule_job_job_id { get; set; }
        public int? schedule_job_order { get; set; }  
        public string schedule_job_car_no { get; set; }
        public string schedule_job_created_by { get; set; }
        public string schedule_job_updated_by { get; set; }
        public DateTime? schedule_job_created_at { get; set; }
        public DateTime? schedule_job_updated_at { get; set; }
    }
}
