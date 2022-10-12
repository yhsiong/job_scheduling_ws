using Job_Scheduling.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Job_Scheduling.Model
{
    public class Vehicle
    {
        public int? vehicle_id { get; set; }
        public int? vehicle_driver_id { get; set; }
        public string vehicle_plat_no { get; set; }
        public string vehicle_model { get; set; }
        public string vehicle_created_by { get; set; }
        public string vehicle_updated_by { get; set; } 
        public string vehicle_status { get; set; }  
        public DateTime? vehicle_updated_at { get; set; }
        public DateTime? vehicle_created_at { get; set; }


        [NotMapped]
        public class Dto
        {
            public class Get : Vehicle
            {

            }
            public class Post : Vehicle
            {
                [Required]
                public string vehicle_created_by { get; set; }
            }
            public class Put : Vehicle
            {
                [Required]
                public string vehicle_updated_by { get; set; }
            }
        }



        [NotMapped]
        public class Operations
        {
            public static async Task<bool> Create(Vehicle_Context vehicle_Context, Dto.Post vehicleScheme)
            {
                vehicle_Context.Vehicle.Add(vehicleScheme);
                try
                {
                    await vehicle_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            public static async Task<bool> Update(Vehicle_Context vehicle_Context, Dto.Put vehicleScheme)
            {
                vehicle_Context.User.Update(vehicleScheme);
                try
                {
                    await vehicle_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            public static async Task<List<Dto.Get>> ReadAll(Vehicle_Context vehicle_Context)
            {
                List<Vehicle> vehicleLists = vehicle_Context.Vehicle.ToList();

                if (vehicleLists == null)
                {
                    return null;
                }
                else
                {
                    return vehicleLists.Select(vehicle => new Dto.Get
                    {
                        vehicle_created_at = vehicle.vehicle_created_at,
                        vehicle_created_by = vehicle.vehicle_created_by,
                        vehicle_driver_id = vehicle.vehicle_id,
                        vehicle_id = vehicle.vehicle_id,
                        vehicle_model = vehicle.vehicle_model,
                        vehicle_plat_no = vehicle.vehicle_plat_no,
                        vehicle_status = vehicle.vehicle_status,
                        vehicle_updated_at = vehicle.vehicle_created_at,
                        vehicle_updated_by = vehicle.vehicle_updated_by
                    }).ToList();
                }
            }
            public static async Task<Dto.Get> ReadSingleById(Vehicle_Context vehicle_Context, Guid vehicle_id)
            {
                Vehicle vehicle = vehicle_Context.Vehicle.Where(x => x.vehicle_id.Equals(vehicle_id)).FirstOrDefault<Vehicle>();

                if (vehicle == null)
                {
                    return null;
                }
                else
                {
                    return new Dto.Get
                    {
                        vehicle_created_at = vehicle.vehicle_created_at,
                        vehicle_created_by = vehicle.vehicle_created_by,
                        vehicle_driver_id = vehicle.vehicle_id,
                        vehicle_id = vehicle.vehicle_id,
                        vehicle_model = vehicle.vehicle_model,
                        vehicle_plat_no = vehicle.vehicle_plat_no,
                        vehicle_status = vehicle.vehicle_status,
                        vehicle_updated_at = vehicle.vehicle_created_at,
                        vehicle_updated_by = vehicle.vehicle_updated_by
                    };
                }
            }
            public static async Task<Dto.Get> ReadSingleByPlatNo(Vehicle_Context vehicle_Context, string vehicle_plat_no)
            {
                Vehicle vehicle = vehicle_Context.Vehicle.Where(x => x.vehicle_plat_no.Equals(vehicle_plat_no)).FirstOrDefault<Vehicle>();

                if (vehicle == null)
                {
                    return null;
                }
                else
                {
                    return new Dto.Get
                    {
                        vehicle_created_at = vehicle.vehicle_created_at,
                        vehicle_created_by = vehicle.vehicle_created_by,
                        vehicle_driver_id = vehicle.vehicle_id,
                        vehicle_id = vehicle.vehicle_id,
                        vehicle_model = vehicle.vehicle_model,
                        vehicle_plat_no = vehicle.vehicle_plat_no,
                        vehicle_status = vehicle.vehicle_status,
                        vehicle_updated_at = vehicle.vehicle_created_at,
                        vehicle_updated_by = vehicle.vehicle_updated_by
                    };
                }
            }

        }
    }
}
