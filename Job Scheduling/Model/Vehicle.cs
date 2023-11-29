using Job_Scheduling.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Job_Scheduling.Model
{
    public class Vehicle
    {
        [Key]
        public Guid vehicle_id { get; set; }
        public Guid vehicle_driver_id { get; set; }
        public string vehicle_plat_no { get; set; }
        public string vehicle_model { get; set; }
        public string? vehicle_created_by { get; set; }
        public string? vehicle_updated_by { get; set; } 
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
            }
            public class Put : Vehicle
            {
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
                Vehicle dtoVehicle = vehicle_Context.Vehicle.Where(x => x.vehicle_id.Equals(vehicleScheme.vehicle_id)).FirstOrDefault();
                dtoVehicle.vehicle_status = vehicleScheme.vehicle_status;
                dtoVehicle.vehicle_updated_by = vehicleScheme.vehicle_updated_by;
                dtoVehicle.vehicle_updated_at = vehicleScheme.vehicle_updated_at;
                dtoVehicle.vehicle_driver_id = vehicleScheme.vehicle_driver_id;
                dtoVehicle.vehicle_plat_no = vehicleScheme.vehicle_plat_no;
                dtoVehicle.vehicle_model = vehicleScheme.vehicle_model;

                vehicle_Context.Vehicle.Update(dtoVehicle);
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
                List<Vehicle> vehicleLists = vehicle_Context.Vehicle.Where(x => !x.vehicle_status.Equals("Deleted")).ToList();

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
            public static async Task<List<Dto.Get>> ReadAllActive(Vehicle_Context vehicle_Context)
            {
                List<Vehicle> vehicleLists = vehicle_Context.Vehicle.Where(x => x.vehicle_status.Equals("Active")).ToList();
                
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
