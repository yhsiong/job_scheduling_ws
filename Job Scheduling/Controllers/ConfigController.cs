using Dapper;
using Job_Scheduling.Database;
using Job_Scheduling.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Text.Json;

namespace Job_Scheduling.Controllers
{
    public class ConfController : Controller
    {
        private Entity_Conf_Context _Entity_Conf_Context;
        private string _connStr = string.Empty;
        private readonly ILogger<ConfController> _logger;
        public ConfController(Entity_Conf_Context entity_Conf_Context, ILogger<ConfController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
            _Entity_Conf_Context = entity_Conf_Context;
        }

        [HttpGet]
        [Route("getEntityName")]
        public async Task<IActionResult> getEntityNameAsync()
        {
            List<Entity_Conf.Dto.Get> confs = await Entity_Conf.Operations.ReadAll(_Entity_Conf_Context);
            if (confs == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {  
                return StatusCode(200, confs);
            } 
        }

        [HttpGet]
        [Route("getEntityById")]
        public async Task<IActionResult> getEntityById(Guid entity_conf_id)
        {
            Entity_Conf.Dto.Get conf = await Entity_Conf.Operations.ReadSingle(_Entity_Conf_Context, entity_conf_id);
            if (conf == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, conf);
            }

        }


    }
}
