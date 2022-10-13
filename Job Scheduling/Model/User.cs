using Job_Scheduling.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Job_Scheduling.Model
{
    public class User
    {
        [Key]
        public Guid user_id { get; set; }
        public string user_username { get; set; }
        public string user_name { get; set; }
        public string user_password { get; set; }
        public string user_status { get; set; }
        public string? user_email { get; set; }
        public string? user_mobile { get; set; }
        public string? user_role { get; set; }
        public string? user_created_by { get; set; }
        public string? user_updated_by { get; set; }
        public DateTime? user_created_at { get; set; }
        public DateTime? user_updated_at { get; set; }

        public Boolean checkSession(HttpContext httpContext)
        {
            return (!String.IsNullOrEmpty(httpContext.Session.GetString("userId")));

        }

        [NotMapped]
        public class Dto
        {
            public class Get : User
            {
                [NotMapped]
                public string user_session_id { get; set; }
            }
            public class Post : User
            {
            }
            public class Put : User
            {
            }
        }

        [NotMapped]
        public class Operations
        {
            public static async Task<bool> Create(User_Context user_Context, Dto.Post userScheme)
            {
                user_Context.User.Add(userScheme);
                try
                {
                    await user_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            public static async Task<bool> Update(User_Context user_Context, Dto.Put userScheme)
            {
                User dtoUser = user_Context.User.Where(x => x.user_id.Equals(userScheme.user_id)).FirstOrDefault();

                dtoUser.user_status = userScheme.user_status;
                dtoUser.user_updated_by = userScheme.user_updated_by;
                dtoUser.user_updated_at = userScheme.user_updated_at;
                dtoUser.user_username = userScheme.user_username;
                dtoUser.user_name = userScheme.user_name;
                dtoUser.user_mobile = userScheme.user_mobile;
                dtoUser.user_role = userScheme.user_role;
                dtoUser.user_email = userScheme.user_email;
                
                user_Context.User.Update(dtoUser);
                try
                {
                    await user_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            public static async Task<List<Dto.Get>> ReadAll(User_Context user_Context)
            {
                List<User> userLists = user_Context.User.Where(x => !x.user_status.Equals("Deleted")).ToList();

                if (userLists == null)
                {
                    return null;
                }
                else
                {
                    return userLists.Select(user => new Dto.Get
                    {
                        user_created_at = user.user_created_at,
                        user_created_by = user.user_created_by,
                        user_id = user.user_id,
                        user_name = user.user_name,
                        user_status = user.user_status,
                        user_updated_at = user.user_updated_at,
                        user_updated_by = user.user_updated_by,
                        user_username = user.user_username

                    }).ToList();
                }
            }
            public static async Task<Dto.Get> ReadSingleById(User_Context user_Context, Guid user_id)
            {
                User user = user_Context.User.Where(x => x.user_id.Equals(user_id)).FirstOrDefault<User>();

                if (user == null)
                {
                    return null;
                }
                else
                {
                    return new Dto.Get
                    {
                        user_created_at = user.user_created_at,
                        user_created_by = user.user_created_by,
                        user_id = user.user_id,
                        user_name = user.user_name,
                        user_status = user.user_status,
                        user_updated_at = user.user_updated_at,
                        user_updated_by = user.user_updated_by,
                        user_username = user.user_username
                    };
                }
            }
            public static async Task<Dto.Get> ReadAuthUser(User_Context user_Context, string username, string password)
            {
                User user = user_Context.User.Where(x => x.user_username.Equals(username) && x.user_password.Equals(password)).FirstOrDefault<User>();

                if (user == null)
                {
                    return null;
                }
                else
                {
                    return new Dto.Get
                    {
                        user_created_at = user.user_created_at,
                        user_created_by = user.user_created_by,
                        user_id = user.user_id,
                        user_name = user.user_name,
                        user_status = user.user_status,
                        user_updated_at = user.user_updated_at,
                        user_updated_by = user.user_updated_by,
                        user_username = user.user_username
                    };
                }
            }

        }

    }
}
