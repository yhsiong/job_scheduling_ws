namespace Job_Scheduling.Model
{
    public class Schedule_Staff
    {
        public int? schedule_staff_id { get; set; }
        public int? schedule_staff_schedule_job_id { get; set; }
        public string schedule_staff_type { get; set; }
        public string schedule_staff_staff_id { get; set; }
        public string schedule_staff_created_by { get; set; }
        public string schedule_staff_updated_by { get; set; }
        public DateTime? schedule_staff_created_at { get; set; }
        public DateTime? schedule_staff_updated_at { get; set; }
    }
}
