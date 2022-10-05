namespace Job_Scheduling.Model
{
    public class Vehicle
    {
        public int? vehicle_id { get; set; }
        public int? vehicle_driver_id { get; set; }
        public string vehicle_plat_no { get; set; }
        public string vehicle_model { get; set; }
        public string vehicle_created_by { get; set; }
        public string vehicle_updated_by { get; set; } 
        public string vehicle_status { get; set; }  
        public DateTime? vehicle_updated_at { get; set; }
        public DateTime? vehicle_created_at { get; set; }
    }
}
