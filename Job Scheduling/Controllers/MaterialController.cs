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
    public class MaterialController : Controller
    {
        private Material_Context _Material_Context;
        private string _connStr = string.Empty;
        private readonly ILogger<MaterialController> _logger;
        public MaterialController(Material_Context Material_Context, ILogger<MaterialController> logger, IConfiguration configuration)
        {
            _Material_Context = Material_Context;
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
        }
        [HttpGet]
        [Route("materials")]
        public async Task<IActionResult> getMaterials()
        {
            List<Material.Dto.Get> materials = await Material.Operations.ReadAll(_Material_Context);
            if (materials == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, materials);
            }              
        }

        [HttpGet]
        [Route("getMaterialById")]
        public async Task<IActionResult> getMaterialById(Guid material_id)
        {

            Material.Dto.Get material = await Material.Operations.ReadSingleById(_Material_Context, material_id);
            if (material == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, material);
            } 
        } 
        [HttpPost]
        [Route("material")]
        public async Task<IActionResult> insertMaterial(Material.Dto.Post material)
        {
            material.material_id = new Guid();
            material.material_created_at = DateTime.Now;
            bool status = await Material.Operations.Create(_Material_Context, material);
            if (status)
            {
                return StatusCode(200, material);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            } 
        }
  
        [HttpPut]
        [Route("material")]
        public async Task<IActionResult> updateMaterial(Material.Dto.Put material)
        {
            material.material_updated_at = DateTime.Now;
            bool status = await Material.Operations.Update(_Material_Context, material);

            if (status)
            {
                return StatusCode(200, material);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            } 
        }
         
    }
}
