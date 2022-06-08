namespace Job_Scheduling.Model
{
    public class User
    {
        public int? user_id { get; set; }
        public string user_username { get; set; }
        public string user_name { get; set; }
        public string user_password { get; set; }
        public string user_status { get; set; }
        public string user_created_by { get; set; }
        public string user_updated_by { get; set; }
        public DateTime? user_created_at { get; set; }
        public DateTime? user_updated_at { get; set; }
        public string user_session_id { get; set; }

        public Boolean checkSession(HttpContext httpContext)
        {
            return (!String.IsNullOrEmpty(httpContext.Session.GetString("userId")));

        }
    }
}
