using Job_Scheduling.Model;
using Microsoft.EntityFrameworkCore;

namespace Job_Scheduling.Database
{
    public class Job_Context : DbContext
    {
        public Job_Context(DbContextOptions<Job_Context> options) : base(options)
        {

        }

        public DbSet<Job> Job { get; set; }
        public DbSet<Job_Task> Job_Task { get; set; }
        public DbSet<Job_Doc> Job_Doc { get; set; }
    }
}
