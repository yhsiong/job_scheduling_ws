using Dapper;
using Job_Scheduling.Database;
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
        private Vehicle_Context _Vehicle_Context;
        private string _connStr = string.Empty;
        private readonly ILogger<VehicleController> _logger;
        public VehicleController(Vehicle_Context Vehicle_Context, ILogger<VehicleController> logger, IConfiguration configuration)
        {
            _Vehicle_Context = Vehicle_Context;
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
        }
        [HttpGet]
        [Route("vehicles")]
        public async Task<IActionResult> getVehicles()
        {
            List<Vehicle.Dto.Get> vehicles = await Vehicle.Operations.ReadAll(_Vehicle_Context);
            if (vehicles == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, vehicles);
            }              
        }

        [HttpGet]
        [Route("getVehicleById")]
        public async Task<IActionResult> getVehicleById(Guid vehicle_id)
        {

            Vehicle.Dto.Get vehicle = await Vehicle.Operations.ReadSingleById(_Vehicle_Context, vehicle_id);
            if (vehicle == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, vehicle);
            } 
        }


        [HttpGet]
        [Route("getVehicleByPlatNo")]
        public async Task<IActionResult> getVehicleByPlatNo(string vehicle_plat_no)
        {

            Vehicle.Dto.Get vehicle = await Vehicle.Operations.ReadSingleByPlatNo(_Vehicle_Context, vehicle_plat_no);
            if (vehicle == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, vehicle);
            }
        }
        [HttpPost]
        [Route("vehicle")]
        public async Task<IActionResult> insertVehicle(Vehicle.Dto.Post vehicle)
        {
            vehicle.vehicle_id = new Guid();
            vehicle.vehicle_created_at = DateTime.Now;
            vehicle.vehicle_created_by = UserController.checkUserId(HttpContext);
            bool status = await Vehicle.Operations.Create(_Vehicle_Context, vehicle);
            if (status)
            {
                return StatusCode(200, vehicle);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            } 
        }
  
        [HttpPut]
        [Route("vehicle")]
        public async Task<IActionResult> updateVehicle(Vehicle.Dto.Put vehicle)
        {
            vehicle.vehicle_updated_by = UserController.checkUserId(HttpContext);
            vehicle.vehicle_updated_at = DateTime.Now;
            bool status = await Vehicle.Operations.Update(_Vehicle_Context, vehicle);

            if (status)
            {
                return StatusCode(200, vehicle);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            } 
        }
         
    }
}
