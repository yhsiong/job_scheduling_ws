using Job_Scheduling.Model;
using Microsoft.EntityFrameworkCore;

namespace Job_Scheduling.Database
{
    public class User_Context : DbContext
    {
        public User_Context(DbContextOptions<User_Context> options) : base(options)
        {

        }

        public DbSet<User> User { get; set; } 
    }
}
