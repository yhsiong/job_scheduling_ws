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
    public class ToolController : Controller
    {
        private Tool_Context _Tool_Context;
        private string _connStr = string.Empty;
        private readonly ILogger<ToolController> _logger;
        public ToolController(Tool_Context Tool_Context, ILogger<ToolController> logger, IConfiguration configuration)
        {
            _Tool_Context = Tool_Context;
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
        }
        [HttpGet]
        [Route("tools")]
        public async Task<IActionResult> getTools()
        {
            List<Tool.Dto.Get> tools = await Tool.Operations.ReadAll(_Tool_Context);
            if (tools == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, tools);
            }              
        }

        [HttpGet]
        [Route("getToolById")]
        public async Task<IActionResult> getToolById(Guid tool_id)
        {

            Tool.Dto.Get tool = await Tool.Operations.ReadSingleById(_Tool_Context, tool_id);
            if (tool == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, tool);
            } 
        }

        [HttpPost]
        [Route("tool")]
        public async Task<IActionResult> insertTool(Tool.Dto.Post tool)
        {
            tool.tool_id = new Guid();
            tool.tool_created_at = DateTime.Now;
            tool.tool_created_by = UserController.checkUserId(HttpContext);
            bool status = await Tool.Operations.Create(_Tool_Context, tool);
            if (status)
            {
                return StatusCode(200, tool);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            } 
        }
  
        [HttpPut]
        [Route("tool")]
        public async Task<IActionResult> updateVehicle(Tool.Dto.Put tool)
        {
            tool.tool_updated_by = UserController.checkUserId(HttpContext);
            tool.tool_updated_at = DateTime.Now;
            bool status = await Tool.Operations.Update(_Tool_Context, tool);

            if (status)
            {
                return StatusCode(200, tool);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            } 
        }
         
    }
}
