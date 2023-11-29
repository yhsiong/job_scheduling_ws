using System.Data.SqlClient;
using Dapper;
using Job_Scheduling.Database;
using Job_Scheduling.Model;
using Job_Scheduling.Model.AutoCount;
using Microsoft.AspNetCore.Mvc;

namespace Job_Scheduling.Controllers
{
    public class AutoCountController : Controller
    {
        private Entity_Conf_Context _Entity_Conf_Context;
        private string _connAutoCountStr = string.Empty;
        private string _connStr = string.Empty;
        private readonly ILogger<AutoCountController> _logger;

        public AutoCountController(Entity_Conf_Context entity_Conf_Context, ILogger<AutoCountController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
            _connAutoCountStr = configuration.GetConnectionString("AutoCountConnection");
            _Entity_Conf_Context = entity_Conf_Context;
        }
        
        [HttpGet]
        [Route("quotations")]
        public async Task<IActionResult> getQuotations(string entity_name)
        { 
            try
            {
                Entity_Conf.Dto.Get conf = await Entity_Conf.Operations.ReadSingleByName(_Entity_Conf_Context, entity_name);

                if (conf != null)
                {
                    List<Quotation.Dto.Get> quotations = await Quotation.Operations.Read(_connAutoCountStr, conf.entity_conf_db_name);
                    return StatusCode(200, quotations);
                }
                else
                {
                    return StatusCode(404, string.Format("OOPs, something went wrong."));
                } 
            }
            catch (Exception e)
            {
                return StatusCode(404, string.Format("OOPs, something went wrong."+ e.Message));
            }

        }
        [HttpGet]
        [Route("quotation")]
        public async Task<IActionResult> getQuotation(string entity_name, string quotation_no)
        {
            try
            {
                Entity_Conf.Dto.Get conf = await Entity_Conf.Operations.ReadSingleByName(_Entity_Conf_Context, entity_name);

                if (conf != null)
                {
                    Quotation.Dto.Get quotation = await Quotation.Operations.ReadSingle(_connAutoCountStr, conf.entity_conf_db_name, quotation_no);
                    return StatusCode(200, quotation);
                }
                else
                {
                    return StatusCode(404, string.Format("OOPs, something went wrong."));
                }
            }
            catch (Exception e)
            {
                return StatusCode(404, string.Format("OOPs, something went wrong." + e.Message));
            }


        }
        [HttpGet]
        [Route("quotationdetails")]
        public async Task<IActionResult> getQuotationDetails(string entity_name, string quotation_no)
        {

            try
            {
                Entity_Conf.Dto.Get conf = await Entity_Conf.Operations.ReadSingleByName(_Entity_Conf_Context, entity_name);

                if (conf != null)
                {
                    List<Quotation_Detail.Dto.Get> quotationDetailss = await Quotation_Detail.Operations.Read(_connAutoCountStr, conf.entity_conf_db_name, quotation_no);
                    return StatusCode(200, quotationDetailss);
                }
                else
                {
                    return StatusCode(404, string.Format("OOPs, something went wrong."));
                }
            }
            catch (Exception e)
            {
                return StatusCode(404, string.Format("OOPs, something went wrong." + e.Message));
            } 
        }
    }
}
