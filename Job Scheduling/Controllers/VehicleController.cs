using Dapper;
using Job_Scheduling.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Job_Scheduling.Controllers
{
    public class VehicleController : Controller
    {
        private string _connStr = string.Empty;
        private readonly ILogger<VehicleController> _logger;
        public VehicleController(ILogger<VehicleController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
        }
        [HttpGet]
        [Route("vehicles")]
        public IActionResult getVehicles()
        { 
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [vehicle]", connection); 

                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    List<Vehicle> vehicles = new List<Vehicle>();
                    if (sdr.HasRows)
                    {                         
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<Vehicle>(typeof(Vehicle));
                            Vehicle vehicle = parser(sdr);
                            vehicles.Add(vehicle);
                        }
                    }
                    return new JsonResult(vehicles);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }
             
        }

        [HttpGet]
        [Route("vehicle")]
        public IActionResult getVehicle(string vehicle_id)
        {
            // get user & Password
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [vehicle] where vehicle_id=@vehicle_id", connection);
                    cm.Parameters.AddWithValue("@vehicle_id", vehicle_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    Vehicle vehicle = new Vehicle();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<Vehicle>(typeof(Vehicle));
                            vehicle = parser(sdr);
                        }
                    }
                    return new JsonResult(vehicle);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }

        }
        [HttpPost]
        [Route("vehicle")]
        public IActionResult insertVehicle(Vehicle vehicle)
        {

            if (string.IsNullOrEmpty(vehicle.vehicle_created_by))
            {
                return new JsonResult("No Session");
            }
            vehicle.vehicle_created_at = DateTime.Now;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("insert into [vehicle] " +
                        "(vehicle_plat_no,vehicle_model,vehicle_created_by,vehicle_created_at,vehicle_status,vehicle_driver_id) OUTPUT INSERTED.[vehicle_id] values " +
                        "(@vehicle_plat_no, @vehicle_model, @vehicle_created_by, @vehicle_created_at, @vehicle_status, @vehicle_driver_id)", connection);
            
                    cm.Parameters.AddWithValue("@vehicle_plat_no", vehicle.vehicle_plat_no);
                    cm.Parameters.AddWithValue("@vehicle_created_by", vehicle.vehicle_created_by);
                    cm.Parameters.AddWithValue("@vehicle_created_at", vehicle.vehicle_created_at);
                    cm.Parameters.AddWithValue("@vehicle_status", vehicle.vehicle_status);
                    cm.Parameters.AddWithValue("@vehicle_driver_id", vehicle.vehicle_driver_id);
                     
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    Int64 result = (Int64)cm.ExecuteScalar();
                    if (result > 0)
                    {

                        vehicle.vehicle_id = int.Parse(result.ToString());
                        return new JsonResult(vehicle);
                    }
                    else
                    {
                        return new JsonResult("Error inserting");
                    }

                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }
        }
  
        [HttpPut]
        [Route("vehicle")]
        public IActionResult updateVehicle(string vehicle_id, string vehicle_status, string vehicle_driver_id)
        {
            Vehicle vehicle = new Vehicle();
            vehicle.vehicle_driver_id = int.Parse(vehicle_driver_id);
            vehicle.vehicle_id = int.Parse(vehicle_id);
            vehicle.vehicle_status = vehicle_status;
            vehicle.vehicle_updated_at = DateTime.Now;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                { 
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("update [vehicle] set vehicle_status=@vehicle_status,vehicle_driver_id=@vehicle_driver_id," +
                    "vehicle_updated_at=@vehicle_updated_at where vehicle_id=@vehicle_id", connection);

                    

                    cm.Parameters.AddWithValue("@vehicle_id", vehicle.vehicle_id);
                    cm.Parameters.AddWithValue("@vehicle_driver_id", vehicle.vehicle_driver_id);
                    cm.Parameters.AddWithValue("@vehicle_status", vehicle.vehicle_status);
                    cm.Parameters.AddWithValue("@vehicle_updated_at", vehicle.vehicle_updated_at);

                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    int result = cm.ExecuteNonQuery();
                    if (result > 0)
                    { 
                        return new JsonResult(vehicle);
                    }
                    else
                    {
                        return new JsonResult("Error inserting");
                    }

                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }
        }
         
    }
}
