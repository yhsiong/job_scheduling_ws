using Job_Scheduling.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Job_Scheduling.Model
{
    public class Material
    {
        [Key]
        public Guid material_id { get; set; }
        public string material_name { get; set; }
        public string material_remark { get; set; }
        public string material_unit { get; set; }
        public string? material_created_by { get; set; }
        public DateTime? material_created_at { get; set; }
        public string? material_updated_by { get; set; }
        public DateTime? material_updated_at { get; set; }
        public string? material_status { get; set; } 

        [NotMapped]
        public class Dto
        {
            public class Get : Material
            {
            }
            public class Post : Material
            {
            }
            public class Put : Material
            {
            }
        }

        [NotMapped]
        public class Operations
        {
            public static async Task<bool> Create(Material_Context material_Context, Dto.Post materialScheme)
            {
                material_Context.Material.Add(materialScheme);
                try
                {
                    await material_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            public static async Task<bool> Update(Material_Context material_Context, Dto.Put materialScheme)
            {
                Material dtoMaterial = material_Context.Material.Where(x => x.material_id.Equals(materialScheme.material_id)).FirstOrDefault();
                dtoMaterial.material_name = materialScheme.material_name;
                dtoMaterial.material_remark = materialScheme.material_remark;
                dtoMaterial.material_unit = materialScheme.material_unit;
                dtoMaterial.material_updated_at = materialScheme.material_updated_at;
                dtoMaterial.material_updated_by = materialScheme.material_updated_by;
                dtoMaterial.material_status = materialScheme.material_status;

                material_Context.Material.Update(dtoMaterial);
                try
                {
                    await material_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            public static async Task<Dto.Get> ReadSingleById(Material_Context material_Context, Guid material_id)
            {
                Material material = material_Context.Material.Where(a => a.material_id.Equals(material_id)).FirstOrDefault();

                if (material == null)
                {
                    return null;
                }
                else
                {
                    return new Dto.Get
                    {
                        material_id = material.material_id,
                        material_name = material.material_name,
                        material_created_at = material.material_created_at,
                        material_created_by = material.material_created_by,
                        material_remark = material.material_remark,
                        material_status = material.material_status,
                        material_unit = material.material_unit,
                        material_updated_at = material.material_updated_at,
                        material_updated_by = material.material_updated_by
                    };
                }
            }
            public static async Task<List<Dto.Get>> ReadAll(Material_Context material_Context)
            {
                List<Material> materials = material_Context.Material.Where(a => !a.material_status.Equals("Deleted")).ToList();

                if (materials == null)
                {
                    return null;
                }
                else
                {
                    return materials.Select(material => new Dto.Get
                    {
                        material_id = material.material_id,
                        material_name = material.material_name,
                        material_created_at = material.material_created_at,
                        material_created_by = material.material_created_by,
                        material_remark = material.material_remark,
                        material_status = material.material_status,
                        material_unit = material.material_unit,
                        material_updated_at = material.material_updated_at,
                        material_updated_by = material.material_updated_by
                    }).ToList();

                }
            }
        }
    }
}
