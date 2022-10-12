using Job_Scheduling.Model;
using Microsoft.EntityFrameworkCore;

namespace Job_Scheduling.Database
{
    public class Vehicle_Context : DbContext
    {
        public Vehicle_Context(DbContextOptions<Vehicle_Context> options) : base(options)
        {

        }

        public DbSet<Vehicle> Vehicle { get; set; } 
    }
}
