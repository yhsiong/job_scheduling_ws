namespace Job_Scheduling.Model
{
    public class Entity_Conf
    {
        public int? entity_conf_id { get; set; }
        public string entity_conf_name { get; set; }
        public string entity_conf_db_name { get; set; } 
        public string entity_conf_created_by { get; set; }
        public string entity_conf_updated_by { get; set; }
        public string entity_conf_status { get; set; }
        
        public DateTime? entity_conf_created_at { get; set; }
        public DateTime? entity_conf_updated_at { get; set; }
    }
}
