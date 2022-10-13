using Dapper;
using Job_Scheduling.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Job_Scheduling.Model
{
    public class Entity_Conf
    {
        [Key]
        public Guid entity_conf_id { get; set; }
        [Required]
        public string entity_conf_name { get; set; }
        [Required]
        public string entity_conf_db_name { get; set; }
        [Required]
        public string entity_conf_status { get; set; }

        public string? entity_conf_created_by { get; set; }
        public DateTime? entity_conf_created_at { get; set; }
        public string? entity_conf_updated_by { get; set; }
        public DateTime? entity_conf_updated_at { get; set; }

        [NotMapped]
        public class Dto
        {
            public class Get : Entity_Conf
            { 
            }
            public class Post : Entity_Conf
            {
            } 
            public class Put : Entity_Conf
            {
            }
        }


        [NotMapped]
        public class Operations
        {
            public static async Task<bool> Create(Entity_Conf_Context entity_Conf_Context, Dto.Post entityConfScheme)
            { 
                entity_Conf_Context.Entity_Conf.Add(entityConfScheme);
                try
                {
                    await entity_Conf_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            public static async Task<bool> Update(Entity_Conf_Context entity_Conf_Context, Dto.Put entityConfScheme)
            { 
                entity_Conf_Context.Entity_Conf.Update(entityConfScheme);
                try
                {
                    await entity_Conf_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                } 
            }

            public static async Task<List<Dto.Get>> ReadAll(Entity_Conf_Context entity_Conf_Context)
            { 
                List<Entity_Conf> confLists = entity_Conf_Context.Entity_Conf.ToList();
               
                if (confLists == null)
                {
                    return null;
                }
                else
                {
                    
                    return confLists.Select(conf => new Dto.Get{ 
                        entity_conf_created_at = conf.entity_conf_created_at,
                        entity_conf_created_by = conf.entity_conf_created_by,
                        entity_conf_db_name = conf.entity_conf_db_name,
                        entity_conf_id = conf.entity_conf_id,
                        entity_conf_name = conf.entity_conf_name,
                        entity_conf_status = conf.entity_conf_status,
                        entity_conf_updated_at = conf.entity_conf_updated_at,
                        entity_conf_updated_by = conf.entity_conf_updated_by
                    }).ToList();
                    
                     
                }
            }
            public static async Task<Dto.Get> ReadSingle(Entity_Conf_Context entity_Conf_Context, Guid entity_conf_id)
            {
                Entity_Conf conf = entity_Conf_Context.Entity_Conf.Where(x => x.entity_conf_id.Equals(entity_conf_id)).FirstOrDefault<Entity_Conf>();
                
                if (conf == null)
                {
                    return null;
                }
                else
                {
                    return new Dto.Get {
                        entity_conf_created_at = conf.entity_conf_created_at,
                        entity_conf_created_by = conf.entity_conf_created_by,
                        entity_conf_db_name = conf.entity_conf_db_name,
                        entity_conf_id = conf.entity_conf_id,
                        entity_conf_name = conf.entity_conf_name,
                        entity_conf_status = conf.entity_conf_status,
                        entity_conf_updated_at = conf.entity_conf_updated_at,
                        entity_conf_updated_by = conf.entity_conf_updated_by
                    }; 


                }
            }

        }
    }
}
