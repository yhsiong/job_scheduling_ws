namespace Job_Scheduling.Model
{
    public class Schedule
    {
        public int? schedule_id { get; set; } 
        public string schedule_date { get; set; }
        public string schedule_remark { get; set; }
        public string schedule_status { get; set; }
        public string schedule_created_by { get; set; }
        public string schedule_updated_by { get; set; } 
        public DateTime? schedule_created_at { get; set; }
        public DateTime? schedule_updated_at { get; set; } 
    }
}
