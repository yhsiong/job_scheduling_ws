using Job_Scheduling.Model;
using Microsoft.EntityFrameworkCore;

namespace Job_Scheduling.Database
{
    public class Material_Context : DbContext
    {
        public Material_Context(DbContextOptions<Material_Context> options) : base(options)
        {

        }

        public DbSet<Material> Material { get; set; } 
    }
}
