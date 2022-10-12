using Job_Scheduling.Model;
using Microsoft.EntityFrameworkCore;

namespace Job_Scheduling.Database
{
    public class Entity_Conf_Context : DbContext
    {
        public Entity_Conf_Context(DbContextOptions<Entity_Conf_Context> options) : base(options)
        {

        }

        public DbSet<Entity_Conf> Entity_Conf { get; set; }
    }
}
