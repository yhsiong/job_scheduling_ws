using Job_Scheduling.Model;
using Microsoft.EntityFrameworkCore;

namespace Job_Scheduling.Database
{
    public class Tool_Context : DbContext
    {
        public Tool_Context(DbContextOptions<Tool_Context> options) : base(options)
        {

        }

        public DbSet<Tool> Tool { get; set; } 
    }
}
