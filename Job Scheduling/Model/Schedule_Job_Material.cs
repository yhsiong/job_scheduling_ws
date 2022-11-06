using Job_Scheduling.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Job_Scheduling.Model
{
    public class Schedule_Job_Material
    {
        [Key]
        public Guid sjm_id { get; set; }
        public Guid sjm_schedule_job_id { get; set; }
        public Guid sjm_material_id { get; set; }
        public string? sjm_created_by { get; set; }
        public DateTime? sjm_created_at { get; set; }
        public string? sjm_updated_by { get; set; }
        public DateTime? sjm_updated_at { get; set; }
        public string? sjm_status { get; set; }
        public float sjm_quantity { get; set; }

        [NotMapped]
        public class Dto
        {
            public class Get : Schedule_Job_Material
            {
            }
            public class Post : Schedule_Job_Material
            {
            }
            public class Put : Schedule_Job_Material
            {
            }
        }

        [NotMapped]
        public class Operations
        {
            public static async Task<bool> Create(Schedule_Context schedule_Context, Dto.Post scheduleJobMaterialScheme)
            {
                schedule_Context.Schedule_Job_Material.Add(scheduleJobMaterialScheme);
                try
                {
                    await schedule_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            public static async Task<bool> Update(Schedule_Context schedule_Context, Dto.Put scheduleJobMaterialScheme)
            {
                Schedule_Job_Material dtoScheduleJobMaterial = schedule_Context.Schedule_Job_Material.Where(x => x.sjm_id.Equals(scheduleJobMaterialScheme.sjm_id)).FirstOrDefault();
                dtoScheduleJobMaterial.sjm_id = scheduleJobMaterialScheme.sjm_id;
                dtoScheduleJobMaterial.sjm_updated_by = scheduleJobMaterialScheme.sjm_updated_by;
                dtoScheduleJobMaterial.sjm_status = scheduleJobMaterialScheme.sjm_status;
                dtoScheduleJobMaterial.sjm_created_by = scheduleJobMaterialScheme.sjm_created_by;
                dtoScheduleJobMaterial.sjm_updated_at = scheduleJobMaterialScheme.sjm_updated_at;
                dtoScheduleJobMaterial.sjm_created_at = scheduleJobMaterialScheme.sjm_created_at;
                dtoScheduleJobMaterial.sjm_material_id = scheduleJobMaterialScheme.sjm_material_id;
                dtoScheduleJobMaterial.sjm_schedule_job_id = scheduleJobMaterialScheme.sjm_schedule_job_id;
                dtoScheduleJobMaterial.sjm_quantity = scheduleJobMaterialScheme.sjm_quantity;
                
                schedule_Context.Schedule_Job_Material.Update(dtoScheduleJobMaterial);
                try
                {
                    await schedule_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            public static async Task<Dto.Get> ReadSingleById(Schedule_Context schedule_Context, Guid sjm_id)
            {
                Schedule_Job_Material schedule_Job_Material = schedule_Context.Schedule_Job_Material.Where(a => a.sjm_id.Equals(sjm_id)).FirstOrDefault();

                if (schedule_Job_Material == null)
                {
                    return null;
                }
                else
                {
                    return new Dto.Get
                    {
                        sjm_id = schedule_Job_Material.sjm_id,
                        sjm_created_at = schedule_Job_Material.sjm_created_at,
                        sjm_created_by = schedule_Job_Material.sjm_created_by,
                        sjm_material_id = schedule_Job_Material.sjm_material_id,
                        sjm_schedule_job_id = schedule_Job_Material.sjm_schedule_job_id,
                        sjm_updated_at = schedule_Job_Material.sjm_updated_at,
                        sjm_status = schedule_Job_Material.sjm_status,
                        sjm_updated_by = schedule_Job_Material.sjm_updated_by,
                        sjm_quantity = schedule_Job_Material.sjm_quantity

                    };
                }
            }
            public static async Task<List<Dto.Get>> ReadByScheduleJobId(Schedule_Context schedule_Context, Guid sjm_schedule_job_id)
            {
                List<Schedule_Job_Material> scheduleJobMaterials = schedule_Context.Schedule_Job_Material.Where(a => a.sjm_schedule_job_id.Equals(sjm_schedule_job_id)).ToList();

                if (scheduleJobMaterials == null)
                {
                    return null;
                }
                else
                {
                    return scheduleJobMaterials.Select(scheduleJobMaterial=> new Dto.Get
                    {
                        sjm_id = scheduleJobMaterial.sjm_id,
                        sjm_created_at = scheduleJobMaterial.sjm_created_at,
                        sjm_created_by = scheduleJobMaterial.sjm_created_by,
                        sjm_material_id = scheduleJobMaterial.sjm_material_id,
                        sjm_schedule_job_id = scheduleJobMaterial.sjm_schedule_job_id,
                        sjm_updated_at = scheduleJobMaterial.sjm_updated_at,
                        sjm_status = scheduleJobMaterial.sjm_status,
                        sjm_updated_by = scheduleJobMaterial.sjm_updated_by,
                        sjm_quantity = scheduleJobMaterial.sjm_quantity
                    }).ToList();
                     
                }
            }
        }
    }
}
