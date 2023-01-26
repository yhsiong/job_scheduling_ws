using Dapper;
using Job_Scheduling.Database;
using Job_Scheduling.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using Z.EntityFramework.Plus;

namespace Job_Scheduling.Controllers
{
    public class ScheduleController : Controller
    {
        private Schedule_Context _Schedule_Context;
        private Vehicle_Context _Vehicle_Context;
        private string _connStr = string.Empty;
        private readonly ILogger<ScheduleController> _logger;
        public ScheduleController(Schedule_Context schedule_Context, Vehicle_Context vehicle_Context, ILogger<ScheduleController> logger, IConfiguration configuration)
        {
            _Schedule_Context = schedule_Context;
            _Vehicle_Context = vehicle_Context;
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection");
        }
        #region schedule
        [HttpGet]
        [Route("schedules")]
        public async Task<IActionResult> getSchedules()
        { 
            List<Schedule.Dto.Get> schedules = await Schedule.Operations.ReadAll(_Schedule_Context);
            if (schedules == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, schedules);
            } 
        }
        [HttpGet]
        [Route("schedule")]
        public async Task<IActionResult> getSchedule(string schedule_id)
        {
            Schedule.Dto.Get schedule = await Schedule.Operations.ReadSingleById(_Schedule_Context, Guid.Parse(schedule_id));
            if (schedule == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, schedule);
            } 
        }
        [HttpGet]
        [Route("schedulebydate")]
        public async Task<IActionResult> getScheduleByDate(string schedule_date)
        {
            Schedule.Dto.Get schedule = await Schedule.Operations.ReadSingleByDate(_Schedule_Context, schedule_date);
            if (schedule == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, schedule);
            }
        }
        [HttpGet]
        [Route("cronschedule")]
        public async Task<IActionResult> insertCronSchedule(string schedule_date)
        {
            Schedule.Dto.Post schedule = new Schedule.Dto.Post();
            schedule.schedule_date = schedule_date;
            schedule.schedule_id = new Guid();
            schedule.schedule_created_at = DateTime.Now;
            schedule.schedule_status = "Processing";
            bool status = await Schedule.Operations.Create(_Schedule_Context, schedule);
            if (status)
            {
                this.generateRoute(schedule.schedule_id.ToString(), "distance");
                return StatusCode(200, schedule);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        [HttpPost]
        [Route("schedule")]
        public async Task<IActionResult> insertSchedule(Schedule.Dto.Post schedule)
        {
            schedule.schedule_id = new Guid();
            schedule.schedule_created_at = DateTime.Now;
            schedule.schedule_status = "Processing";
            bool status = await Schedule.Operations.Create(_Schedule_Context, schedule);
            if (status)
            {
                this.generateRoute(schedule.schedule_id.ToString(),"distance");
                return StatusCode(200, schedule);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        [HttpPut]
        [Route("schedule")]
        public async Task<IActionResult> updateSchedule(Schedule.Dto.Put scheduleScheme)   
        {
            scheduleScheme.schedule_updated_at = DateTime.Now;
            bool status = await Schedule.Operations.Update(_Schedule_Context, scheduleScheme);

            if (status)
            {
                return StatusCode(200, scheduleScheme);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        #endregion
        #region schedule job
        [HttpGet]
        [Route("schedulejobs")]
        public async Task<IActionResult> getScheduleJobs(string schedule_id)
        {
            List<Schedule_Job.Dto.Get> scheduleJobs = await Schedule_Job.Operations.ReadSingleByScheduleId(_Schedule_Context, Guid.Parse(schedule_id));
            if (scheduleJobs == null)
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
            else
            {
                return StatusCode(200, scheduleJobs);
            }
        }
        [HttpGet]
        [Route("schedulejobswithdetails")]
        public IActionResult getScheduleJobsWithDetails(string schedule_id)
        {
            // get user & Password
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from[Schedule_job] right join vehicle on vehicle_id =[Schedule_job].schedule_job_vehicle_id " +
                    " inner join job on job_id =[Schedule_job].schedule_job_job_id where schedule_job_schedule_id=@schedule_job_schedule_id", connection);
                    cm.Parameters.AddWithValue("@schedule_job_schedule_id", schedule_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    List<dynamic> schedulejobs = new List<dynamic>();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<dynamic>();
                            dynamic schedulejob = parser(sdr);
                            schedulejobs.Add(schedulejob);
                        }
                    }
                    return new JsonResult(schedulejobs);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }

        }
        [HttpPut]
        [Route("scheduleJob")]
        public async Task<IActionResult> updateScheduleJob(string jsonSchedule)
        {
            dynamic schedule = JsonConvert.DeserializeObject<dynamic>(jsonSchedule);
            Guid schedule_job_id = Guid.Parse(schedule["schedule_job_id"].ToString());

            if (schedule["tools"] != null)
            {
                var tools = schedule["tools"];
                // clear all tools
                var scheduleJobTools = _Schedule_Context.Schedule_Job_Tool.Where(x => x.sjt_schedule_job_id.Equals(schedule_job_id));
                _Schedule_Context.Schedule_Job_Tool.RemoveRange(scheduleJobTools);
                _Schedule_Context.SaveChanges();
                Schedule_Job_Tool.Dto.Post scheduleTool = new Schedule_Job_Tool.Dto.Post();

                for (int i = 0; i < tools.Count; i++)
                {
                    scheduleTool = new Schedule_Job_Tool.Dto.Post();
                    scheduleTool.sjt_id = new Guid();
                    scheduleTool.sjt_tool_id = tools[i]["tool_id"];
                    scheduleTool.sjt_schedule_job_id = schedule_job_id;
                    scheduleTool.sjt_status = "Active";
                    scheduleTool.sjt_created_at = DateTime.Now;
                    await Schedule_Job_Tool.Operations.Create(_Schedule_Context, scheduleTool);
                }
            }

            if (schedule["materials"] != null)
            {
                var materials = schedule["materials"];
                // clear all material
                var scheduleJobMaterials = _Schedule_Context.Schedule_Job_Material.Where(x => x.sjm_schedule_job_id.Equals(schedule_job_id));
                _Schedule_Context.Schedule_Job_Material.RemoveRange(scheduleJobMaterials);
                _Schedule_Context.SaveChanges();
                Schedule_Job_Material.Dto.Post scheduleMaterial = new Schedule_Job_Material.Dto.Post();
                for (int i = 0; i < materials.Count; i++)
                {
                    scheduleMaterial = new Schedule_Job_Material.Dto.Post();
                    scheduleMaterial.sjm_id = new Guid();
                    scheduleMaterial.sjm_material_id = materials[i]["material_id"];
                    scheduleMaterial.sjm_schedule_job_id = schedule_job_id;
                    scheduleMaterial.sjm_status = "Active";
                    scheduleMaterial.sjm_created_at = DateTime.Now;
                    scheduleMaterial.sjm_quantity = materials[i]["material_quantity"];
                    await Schedule_Job_Material.Operations.Create(_Schedule_Context, scheduleMaterial);
                }

            }
             
            if (schedule["workers"] != null)
            {
                var workers = schedule["workers"];
                // clear all worker
                var scheduleJobWorkers = _Schedule_Context.Schedule_Job_Worker.Where(x => x.sjw_schedule_job_id.Equals(schedule_job_id));
                _Schedule_Context.Schedule_Job_Worker.RemoveRange(scheduleJobWorkers);
                _Schedule_Context.SaveChanges();
                Schedule_Job_Worker.Dto.Post scheduleWorker = new Schedule_Job_Worker.Dto.Post();
                for (int i = 0; i < workers.Count; i++)
                {
                    scheduleWorker = new Schedule_Job_Worker.Dto.Post();
                    scheduleWorker.sjw_id = new Guid();
                    scheduleWorker.sjw_worker_id = workers[i]["user_id"];
                    scheduleWorker.sjw_schedule_job_id = schedule_job_id;
                    scheduleWorker.sjw_status = "Active";
                    scheduleWorker.sjw_created_at = DateTime.Now;
                    await Schedule_Job_Worker.Operations.Create(_Schedule_Context, scheduleWorker);
                }

            }

            string remarks = schedule["remarks"].ToString(); 
            Schedule_Job scheduleJob = _Schedule_Context.Schedule_Job.Where(x => x.schedule_job_id.Equals(schedule_job_id)).FirstOrDefault();
            scheduleJob.schedule_job_remark = remarks;
            await Schedule_Job.Operations.Update(_Schedule_Context, (Schedule_Job.Dto.Put)scheduleJob);
             
            return StatusCode(200, true);
            /*bool status = await Schedule.Operations.Update(_Schedule_Context, scheduleScheme);

            if (status)
            {
                return StatusCode(200, scheduleScheme);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }*/
        }
        [HttpPut]
        [Route("scheduleJobs")]
        public async Task<IActionResult> updateScheduleJobs(string schedule_id, string jsonSchedule)
        {
           // jsonSchedule = "{\"CAR0001\":[{\"schedule_job_id\":\"4924c661-f9a4-4b0c-9238-b788940d0ff2\",\"schedule_job_schedule_id\":\"1635bb02-bd99-4bbc-0bcf-08dae262d71a\",\"schedule_job_job_id\":\"945f1c70-2108-47ad-9fe3-08daad987323\",\"schedule_job_order\":1,\"schedule_job_vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T16:19:09.523\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"vehicle_plat_no\":\"CAR0001\",\"vehicle_model\":\"Toyota2\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:42:31.737\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":\"2022-10-24T17:53:57.407\",\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"job_id\":\"945f1c70-2108-47ad-9fe3-08daad987323\",\"job_no\":\"JOB-00003\",\"job_quotation_no\":\"QT-000122\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T11:59:40.893\",\"job_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"259405\",\"job_address\":\"10 Draycott Park #11-08 Condo\",\"job_customer_name\":\"L'rey Associates\",\"job_customer_code\":\"300-L001\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.832182029022\",\"job_latitude\":\"1.31138841261308\",\"job_end_date\":\"2022-10-25T00:00:00\",\"job_start_date\":\"2022-10-14T00:00:00\",\"job_primary_staff\":null,\"tools\":[],\"materials\":[],\"workers\":[]},{\"schedule_job_id\":\"7256d92c-ccc5-4b98-ba98-54217437722d\",\"schedule_job_schedule_id\":\"1635bb02-bd99-4bbc-0bcf-08dae262d71a\",\"schedule_job_job_id\":\"dad4e473-b9a4-47fc-f568-08daada727eb\",\"schedule_job_order\":2,\"schedule_job_vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T16:19:09.523\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"vehicle_plat_no\":\"CAR0001\",\"vehicle_model\":\"Toyota2\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:42:31.737\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":\"2022-10-24T17:53:57.407\",\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"job_id\":\"dad4e473-b9a4-47fc-f568-08daada727eb\",\"job_no\":\"JOB-00004\",\"job_quotation_no\":\"QT-000132\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T13:44:26.71\",\"job_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"228266\",\"job_address\":\"22 Saunders Rd\",\"job_customer_name\":\"Apt Atelier Pte Ltd\",\"job_customer_code\":\"300-A026\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.838917040304\",\"job_latitude\":\"1.30387980905309\",\"job_end_date\":\"2022-10-19T00:00:00\",\"job_start_date\":\"2022-10-14T00:00:00\",\"job_primary_staff\":null,\"tools\":[],\"workers\":[],\"materials\":[]},{\"schedule_job_id\":\"af714d20-e13d-4d35-abc9-25e95c869581\",\"schedule_job_schedule_id\":\"1635bb02-bd99-4bbc-0bcf-08dae262d71a\",\"schedule_job_job_id\":\"365457bd-2fa8-44b4-f569-08daada727eb\",\"schedule_job_order\":3,\"schedule_job_vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T16:19:09.523\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"vehicle_plat_no\":\"CAR0001\",\"vehicle_model\":\"Toyota2\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:42:31.737\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":\"2022-10-24T17:53:57.407\",\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"job_id\":\"365457bd-2fa8-44b4-f569-08daada727eb\",\"job_no\":\"JOB-00005\",\"job_quotation_no\":\"QT-000007\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T13:44:44.203\",\"job_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"048621\",\"job_address\":\"24 Raffles Place #25-05 \\r\\nClifford Centre Singapore 048621\",\"job_customer_name\":\"37 Reno Pte Ltd\",\"job_customer_code\":\"300-3002\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.852130103244\",\"job_latitude\":\"1.28385582828327\",\"job_end_date\":\"2022-11-03T00:00:00\",\"job_start_date\":\"2022-10-14T00:00:00\",\"job_primary_staff\":null,\"tools\":[],\"materials\":[],\"workers\":[]},{\"schedule_job_id\":\"ef5665b6-6037-4b39-9dd2-ad27f2f38cf1\",\"schedule_job_schedule_id\":\"1635bb02-bd99-4bbc-0bcf-08dae262d71a\",\"schedule_job_job_id\":\"fdda28d7-b3aa-47dd-f56a-08daada727eb\",\"schedule_job_order\":4,\"schedule_job_vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T16:19:09.523\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"vehicle_plat_no\":\"CAR0001\",\"vehicle_model\":\"Toyota2\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:42:31.737\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":\"2022-10-24T17:53:57.407\",\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"job_id\":\"fdda28d7-b3aa-47dd-f56a-08daada727eb\",\"job_no\":\"JOB-00006\",\"job_quotation_no\":\"QT-000031\",\"job_remark\":\"qwe\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T13:44:59.83\",\"job_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"079903\",\"job_address\":\"10 Anson Rd #09-14\",\"job_customer_name\":\"Apcon Pte Ltd\",\"job_customer_code\":\"300-A006\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.845923793168\",\"job_latitude\":\"1.27588674266836\",\"job_end_date\":\"2022-10-28T00:00:00\",\"job_start_date\":\"2022-10-14T00:00:00\",\"job_primary_staff\":null,\"materials\":[],\"workers\":[],\"tools\":[]},{\"schedule_job_id\":\"499c79de-7499-4518-a86d-2b2daafba04c\",\"schedule_job_schedule_id\":\"1635bb02-bd99-4bbc-0bcf-08dae262d71a\",\"schedule_job_job_id\":\"35f7dfbf-18fe-4892-f56b-08daada727eb\",\"schedule_job_order\":5,\"schedule_job_vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T16:19:09.523\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"vehicle_plat_no\":\"CAR0001\",\"vehicle_model\":\"Toyota2\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:42:31.737\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":\"2022-10-24T17:53:57.407\",\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"job_id\":\"35f7dfbf-18fe-4892-f56b-08daada727eb\",\"job_no\":\"JOB-00007\",\"job_quotation_no\":\"QT-000043\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T13:45:56.457\",\"job_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"544688\",\"job_address\":\"91 Compassvale Bow #16-38 Jewel@Buangkok\",\"job_customer_name\":\"Vitas Design Pte Ltd\",\"job_customer_code\":\"300-V005\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.892368596692\",\"job_latitude\":\"1.3805811743311\",\"job_end_date\":\"2022-10-26T00:00:00\",\"job_start_date\":\"2022-10-14T00:00:00\",\"job_primary_staff\":null,\"materials\":[],\"tools\":[],\"workers\":[]}],\"CAR0002\":[{\"schedule_job_id\":\"861299eb-4e19-4264-b931-984334e098f8\",\"schedule_job_schedule_id\":\"1635bb02-bd99-4bbc-0bcf-08dae262d71a\",\"schedule_job_job_id\":\"02c7109e-0a19-4daf-9fe2-08daad987323\",\"schedule_job_order\":0,\"schedule_job_vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T16:19:09.523\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"vehicle_plat_no\":\"CAR0001\",\"vehicle_model\":\"Toyota2\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:42:31.737\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":\"2022-10-24T17:53:57.407\",\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"job_id\":\"02c7109e-0a19-4daf-9fe2-08daad987323\",\"job_no\":\"JOB-00002\",\"job_quotation_no\":\"QT-000120\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T11:59:27.183\",\"job_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_updated_at\":\"2022-10-14T20:48:29.4\",\"job_updated_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_postal_code\":\"650291\",\"job_address\":\"IRVINS Messy@Wisma Atria #B1-59\",\"job_customer_name\":\"Yi Plasters Pte Ltd\",\"job_customer_code\":\"300-Y006\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.75605170306\",\"job_latitude\":\"1.34366766259088\",\"job_end_date\":\"2022-10-28T00:00:00\",\"job_start_date\":\"2022-10-18T00:00:00\",\"job_primary_staff\":null,\"tools\":[],\"materials\":[],\"workers\":[]},{\"schedule_job_id\":\"8a60d1ff-7919-46b4-ab1f-6bac3116077d\",\"schedule_job_schedule_id\":\"1635bb02-bd99-4bbc-0bcf-08dae262d71a\",\"schedule_job_job_id\":\"01538cfb-921d-4f3e-f56c-08daada727eb\",\"schedule_job_order\":0,\"schedule_job_vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T16:19:09.523\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"vehicle_plat_no\":\"CAR0002\",\"vehicle_model\":\"Honda\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:56:36.7\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":null,\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"00000000-0000-0000-0000-000000000000\",\"job_id\":\"01538cfb-921d-4f3e-f56c-08daada727eb\",\"job_no\":\"JOB-00008\",\"job_quotation_no\":\"QT-000124\",\"job_remark\":\"asdads\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T13:46:06.343\",\"job_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"529482\",\"job_address\":\"Tampines Telepark #01-01\",\"job_customer_name\":\"Alpha Werkz\",\"job_customer_code\":\"300-A010\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.94211744564\",\"job_latitude\":\"1.35351077725973\",\"job_end_date\":\"2022-10-27T00:00:00\",\"job_start_date\":\"2022-10-14T00:00:00\",\"job_primary_staff\":null,\"tools\":[],\"materials\":[],\"workers\":[]},{\"schedule_job_id\":\"19a96f8c-5822-44e0-93ae-d1b4fcfc43bd\",\"schedule_job_schedule_id\":\"1635bb02-bd99-4bbc-0bcf-08dae262d71a\",\"schedule_job_job_id\":\"f27d0eb6-09fa-4682-5247-08daadd55e03\",\"schedule_job_order\":1,\"schedule_job_vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T16:19:09.523\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"vehicle_plat_no\":\"CAR0002\",\"vehicle_model\":\"Honda\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:56:36.7\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":null,\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"00000000-0000-0000-0000-000000000000\",\"job_id\":\"f27d0eb6-09fa-4682-5247-08daadd55e03\",\"job_no\":\"JOB-00009\",\"job_quotation_no\":\"QT-000122\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T19:15:14.327\",\"job_created_by\":\"90b243a0-3ad5-4265-be03-08daad17f007\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"259405\",\"job_address\":\"10 Draycott Park #11-08 Condo\",\"job_customer_name\":\"L'rey Associates\",\"job_customer_code\":\"300-L001\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.832182029022\",\"job_latitude\":\"1.31138841261308\",\"job_end_date\":\"2022-10-24T00:00:00\",\"job_start_date\":\"2022-10-15T00:00:00\",\"job_primary_staff\":null,\"tools\":[],\"materials\":[],\"workers\":[]},{\"schedule_job_id\":\"29ccbaa0-11a1-4dc3-8dcb-d4eb95ff5f99\",\"schedule_job_schedule_id\":\"1635bb02-bd99-4bbc-0bcf-08dae262d71a\",\"schedule_job_job_id\":\"2413ff11-eb42-4d02-524a-08daadd55e03\",\"schedule_job_order\":2,\"schedule_job_vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T16:19:09.523\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"vehicle_plat_no\":\"CAR0002\",\"vehicle_model\":\"Honda\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:56:36.7\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":null,\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"00000000-0000-0000-0000-000000000000\",\"job_id\":\"2413ff11-eb42-4d02-524a-08daadd55e03\",\"job_no\":\"JOB-00012\",\"job_quotation_no\":\"QT-000105\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T19:24:12.417\",\"job_created_by\":\"90b243a0-3ad5-4265-be03-08daad17f007\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"536050\",\"job_address\":\"25 Paya Lebar Cres\",\"job_customer_name\":\"d'Phenomenal Pte Ltd\",\"job_customer_code\":\"300-D017\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.882210328557\",\"job_latitude\":\"1.3499801733955\",\"job_end_date\":\"2022-12-13T00:00:00\",\"job_start_date\":\"2022-12-12T00:00:00\",\"job_primary_staff\":null,\"tools\":[],\"workers\":[],\"materials\":[]},{\"schedule_job_id\":\"7b6cc2be-86fa-4b6d-a435-fcbf334d2d1e\",\"schedule_job_schedule_id\":\"1635bb02-bd99-4bbc-0bcf-08dae262d71a\",\"schedule_job_job_id\":\"09dddd7c-feb2-4698-524c-08daadd55e03\",\"schedule_job_order\":3,\"schedule_job_vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T16:19:09.523\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"vehicle_plat_no\":\"CAR0002\",\"vehicle_model\":\"Honda\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:56:36.7\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":null,\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"00000000-0000-0000-0000-000000000000\",\"job_id\":\"09dddd7c-feb2-4698-524c-08daadd55e03\",\"job_no\":\"JOB-00014\",\"job_quotation_no\":\"QT-000119\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T20:01:12.963\",\"job_created_by\":\"90b243a0-3ad5-4265-be03-08daad17f007\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"569880\",\"job_address\":\"7030 Ang Mo Kio Ave 5 #B1-01 Northstart\",\"job_customer_name\":\"Darwin Design & Developments Pte Ltd\",\"job_customer_code\":\"300-D004\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.875389838376\",\"job_latitude\":\"1.37792345221534\",\"job_end_date\":\"2022-10-20T00:00:00\",\"job_start_date\":\"2022-10-17T00:00:00\",\"job_primary_staff\":null,\"materials\":[],\"workers\":[],\"tools\":[]}],\"CAR0003\":[{\"schedule_job_id\":\"d4ac0d3e-92d8-4931-a38c-6a95301a6436\",\"schedule_job_schedule_id\":\"1635bb02-bd99-4bbc-0bcf-08dae262d71a\",\"schedule_job_job_id\":\"a1a50348-8c62-4257-5249-08daadd55e03\",\"schedule_job_order\":0,\"schedule_job_vehicle_id\":\"3489a63b-63b1-4b5f-53a0-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T16:19:09.523\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"3489a63b-63b1-4b5f-53a0-08daad961fdb\",\"vehicle_plat_no\":\"CAR0003\",\"vehicle_model\":\"Suzuki\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:56:48.847\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":null,\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"00000000-0000-0000-0000-000000000000\",\"job_id\":\"a1a50348-8c62-4257-5249-08daadd55e03\",\"job_no\":\"JOB-00011\",\"job_quotation_no\":\"QT-000106\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T19:23:22.443\",\"job_created_by\":\"90b243a0-3ad5-4265-be03-08daad17f007\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"650283\",\"job_address\":\"9 River Sound #01-26 Condo\",\"job_customer_name\":\"First SM Construction Pte Ltd\",\"job_customer_code\":\"300-F002\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.757539380939\",\"job_latitude\":\"1.34694365753899\",\"job_end_date\":\"2022-12-13T00:00:00\",\"job_start_date\":\"2022-12-08T00:00:00\",\"job_primary_staff\":null,\"tools\":[],\"materials\":[],\"workers\":[]},{\"schedule_job_id\":\"28abc0ed-2eb1-4f51-b20e-2a7a15976457\",\"schedule_job_schedule_id\":\"1635bb02-bd99-4bbc-0bcf-08dae262d71a\",\"schedule_job_job_id\":\"693996da-7549-495c-524b-08daadd55e03\",\"schedule_job_order\":1,\"schedule_job_vehicle_id\":\"3489a63b-63b1-4b5f-53a0-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T16:19:09.523\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"3489a63b-63b1-4b5f-53a0-08daad961fdb\",\"vehicle_plat_no\":\"CAR0003\",\"vehicle_model\":\"Suzuki\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:56:48.847\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":null,\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"00000000-0000-0000-0000-000000000000\",\"job_id\":\"693996da-7549-495c-524b-08daadd55e03\",\"job_no\":\"JOB-00013\",\"job_quotation_no\":\"QT-000104\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T19:25:03.29\",\"job_created_by\":\"90b243a0-3ad5-4265-be03-08daad17f007\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"729935\",\"job_address\":\"25A Mandai Estate #05-04/05 Innovation Place\",\"job_customer_name\":\"Wee Guan Construction Pte Ltd\",\"job_customer_code\":\"300-W012\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.759580459746\",\"job_latitude\":\"1.40631935420444\",\"job_end_date\":\"2022-12-15T00:00:00\",\"job_start_date\":\"2022-12-12T00:00:00\",\"job_primary_staff\":null,\"tools\":[],\"materials\":[],\"workers\":[]}]}";
            //jsonSchedule = "{\"CAR0001\":[{\"schedule_job_id\":\"2bc32eba-a740-41b9-9a88-2de01aefbc30\",\"schedule_job_schedule_id\":\"fe7a4998-f297-43b0-7c35-08dae23b31a3\",\"schedule_job_job_id\":\"945f1c70-2108-47ad-9fe3-08daad987323\",\"schedule_job_order\":1,\"schedule_job_vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T11:35:20.807\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"vehicle_plat_no\":\"CAR0001\",\"vehicle_model\":\"Toyota2\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:42:31.737\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":\"2022-10-24T17:53:57.407\",\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"job_id\":\"945f1c70-2108-47ad-9fe3-08daad987323\",\"job_no\":\"JOB-00003\",\"job_quotation_no\":\"QT-000122\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T11:59:40.893\",\"job_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"259405\",\"job_address\":\"10 Draycott Park #11-08 Condo\",\"job_customer_name\":\"L'rey Associates\",\"job_customer_code\":\"300-L001\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.832182029022\",\"job_latitude\":\"1.31138841261308\",\"job_end_date\":\"2022-10-25T00:00:00\",\"job_start_date\":\"2022-10-14T00:00:00\",\"job_primary_staff\":null},{\"schedule_job_id\":\"9f5db65c-4f69-474c-94f7-2b72232f4871\",\"schedule_job_schedule_id\":\"fe7a4998-f297-43b0-7c35-08dae23b31a3\",\"schedule_job_job_id\":\"dad4e473-b9a4-47fc-f568-08daada727eb\",\"schedule_job_order\":2,\"schedule_job_vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T11:35:20.807\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"vehicle_plat_no\":\"CAR0001\",\"vehicle_model\":\"Toyota2\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:42:31.737\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":\"2022-10-24T17:53:57.407\",\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"job_id\":\"dad4e473-b9a4-47fc-f568-08daada727eb\",\"job_no\":\"JOB-00004\",\"job_quotation_no\":\"QT-000132\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T13:44:26.71\",\"job_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"228266\",\"job_address\":\"22 Saunders Rd\",\"job_customer_name\":\"Apt Atelier Pte Ltd\",\"job_customer_code\":\"300-A026\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.838917040304\",\"job_latitude\":\"1.30387980905309\",\"job_end_date\":\"2022-10-19T00:00:00\",\"job_start_date\":\"2022-10-14T00:00:00\",\"job_primary_staff\":null},{\"schedule_job_id\":\"3e6c3556-60be-467b-8f98-ad4f9449e63a\",\"schedule_job_schedule_id\":\"fe7a4998-f297-43b0-7c35-08dae23b31a3\",\"schedule_job_job_id\":\"365457bd-2fa8-44b4-f569-08daada727eb\",\"schedule_job_order\":3,\"schedule_job_vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T11:35:20.807\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"vehicle_plat_no\":\"CAR0001\",\"vehicle_model\":\"Toyota2\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:42:31.737\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":\"2022-10-24T17:53:57.407\",\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"job_id\":\"365457bd-2fa8-44b4-f569-08daada727eb\",\"job_no\":\"JOB-00005\",\"job_quotation_no\":\"QT-000007\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T13:44:44.203\",\"job_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"048621\",\"job_address\":\"24 Raffles Place #25-05 \\r\\nClifford Centre Singapore 048621\",\"job_customer_name\":\"37 Reno Pte Ltd\",\"job_customer_code\":\"300-3002\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.852130103244\",\"job_latitude\":\"1.28385582828327\",\"job_end_date\":\"2022-11-03T00:00:00\",\"job_start_date\":\"2022-10-14T00:00:00\",\"job_primary_staff\":null},{\"schedule_job_id\":\"1743333c-1ebb-4588-84c7-94e8e52f8d58\",\"schedule_job_schedule_id\":\"fe7a4998-f297-43b0-7c35-08dae23b31a3\",\"schedule_job_job_id\":\"fdda28d7-b3aa-47dd-f56a-08daada727eb\",\"schedule_job_order\":4,\"schedule_job_vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T11:35:20.807\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"vehicle_plat_no\":\"CAR0001\",\"vehicle_model\":\"Toyota2\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:42:31.737\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":\"2022-10-24T17:53:57.407\",\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"job_id\":\"fdda28d7-b3aa-47dd-f56a-08daada727eb\",\"job_no\":\"JOB-00006\",\"job_quotation_no\":\"QT-000031\",\"job_remark\":\"qwe\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T13:44:59.83\",\"job_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"079903\",\"job_address\":\"10 Anson Rd #09-14\",\"job_customer_name\":\"Apcon Pte Ltd\",\"job_customer_code\":\"300-A006\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.845923793168\",\"job_latitude\":\"1.27588674266836\",\"job_end_date\":\"2022-10-28T00:00:00\",\"job_start_date\":\"2022-10-14T00:00:00\",\"job_primary_staff\":null},{\"schedule_job_id\":\"2ddea8a4-d711-46d8-94f2-2429e957bec1\",\"schedule_job_schedule_id\":\"fe7a4998-f297-43b0-7c35-08dae23b31a3\",\"schedule_job_job_id\":\"35f7dfbf-18fe-4892-f56b-08daada727eb\",\"schedule_job_order\":5,\"schedule_job_vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T11:35:20.807\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"vehicle_plat_no\":\"CAR0001\",\"vehicle_model\":\"Toyota2\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:42:31.737\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":\"2022-10-24T17:53:57.407\",\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"job_id\":\"35f7dfbf-18fe-4892-f56b-08daada727eb\",\"job_no\":\"JOB-00007\",\"job_quotation_no\":\"QT-000043\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T13:45:56.457\",\"job_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"544688\",\"job_address\":\"91 Compassvale Bow #16-38 Jewel@Buangkok\",\"job_customer_name\":\"Vitas Design Pte Ltd\",\"job_customer_code\":\"300-V005\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.892368596692\",\"job_latitude\":\"1.3805811743311\",\"job_end_date\":\"2022-10-26T00:00:00\",\"job_start_date\":\"2022-10-14T00:00:00\",\"job_primary_staff\":null}],\"CAR0002\":[{\"schedule_job_id\":\"a0dc1aab-67e3-4074-ba3c-d3900c9bfcff\",\"schedule_job_schedule_id\":\"fe7a4998-f297-43b0-7c35-08dae23b31a3\",\"schedule_job_job_id\":\"02c7109e-0a19-4daf-9fe2-08daad987323\",\"schedule_job_order\":0,\"schedule_job_vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T11:35:20.807\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"vehicle_plat_no\":\"CAR0001\",\"vehicle_model\":\"Toyota2\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:42:31.737\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":\"2022-10-24T17:53:57.407\",\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"78e8bb48-72dc-4599-539e-08daad961fdb\",\"job_id\":\"02c7109e-0a19-4daf-9fe2-08daad987323\",\"job_no\":\"JOB-00002\",\"job_quotation_no\":\"QT-000120\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T11:59:27.183\",\"job_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_updated_at\":\"2022-10-14T20:48:29.4\",\"job_updated_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_postal_code\":\"650291\",\"job_address\":\"IRVINS Messy@Wisma Atria #B1-59\",\"job_customer_name\":\"Yi Plasters Pte Ltd\",\"job_customer_code\":\"300-Y006\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.75605170306\",\"job_latitude\":\"1.34366766259088\",\"job_end_date\":\"2022-10-28T00:00:00\",\"job_start_date\":\"2022-10-18T00:00:00\",\"job_primary_staff\":null},{\"schedule_job_id\":\"94b6268d-c5fe-42a4-a2b4-9b49ae0431fa\",\"schedule_job_schedule_id\":\"fe7a4998-f297-43b0-7c35-08dae23b31a3\",\"schedule_job_job_id\":\"01538cfb-921d-4f3e-f56c-08daada727eb\",\"schedule_job_order\":0,\"schedule_job_vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T11:35:20.807\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"vehicle_plat_no\":\"CAR0002\",\"vehicle_model\":\"Honda\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:56:36.7\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":null,\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"00000000-0000-0000-0000-000000000000\",\"job_id\":\"01538cfb-921d-4f3e-f56c-08daada727eb\",\"job_no\":\"JOB-00008\",\"job_quotation_no\":\"QT-000124\",\"job_remark\":\"asdads\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T13:46:06.343\",\"job_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"529482\",\"job_address\":\"Tampines Telepark #01-01\",\"job_customer_name\":\"Alpha Werkz\",\"job_customer_code\":\"300-A010\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.94211744564\",\"job_latitude\":\"1.35351077725973\",\"job_end_date\":\"2022-10-27T00:00:00\",\"job_start_date\":\"2022-10-14T00:00:00\",\"job_primary_staff\":null},{\"schedule_job_id\":\"21ca8d4d-43ec-4383-ace9-f59f5a278ad4\",\"schedule_job_schedule_id\":\"fe7a4998-f297-43b0-7c35-08dae23b31a3\",\"schedule_job_job_id\":\"f27d0eb6-09fa-4682-5247-08daadd55e03\",\"schedule_job_order\":1,\"schedule_job_vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T11:35:20.807\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"vehicle_plat_no\":\"CAR0002\",\"vehicle_model\":\"Honda\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:56:36.7\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":null,\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"00000000-0000-0000-0000-000000000000\",\"job_id\":\"f27d0eb6-09fa-4682-5247-08daadd55e03\",\"job_no\":\"JOB-00009\",\"job_quotation_no\":\"QT-000122\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T19:15:14.327\",\"job_created_by\":\"90b243a0-3ad5-4265-be03-08daad17f007\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"259405\",\"job_address\":\"10 Draycott Park #11-08 Condo\",\"job_customer_name\":\"L'rey Associates\",\"job_customer_code\":\"300-L001\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.832182029022\",\"job_latitude\":\"1.31138841261308\",\"job_end_date\":\"2022-10-24T00:00:00\",\"job_start_date\":\"2022-10-15T00:00:00\",\"job_primary_staff\":null},{\"schedule_job_id\":\"801bfbd8-82c8-4f6f-aa80-75229d27561a\",\"schedule_job_schedule_id\":\"fe7a4998-f297-43b0-7c35-08dae23b31a3\",\"schedule_job_job_id\":\"2413ff11-eb42-4d02-524a-08daadd55e03\",\"schedule_job_order\":2,\"schedule_job_vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T11:35:20.807\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"vehicle_plat_no\":\"CAR0002\",\"vehicle_model\":\"Honda\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:56:36.7\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":null,\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"00000000-0000-0000-0000-000000000000\",\"job_id\":\"2413ff11-eb42-4d02-524a-08daadd55e03\",\"job_no\":\"JOB-00012\",\"job_quotation_no\":\"QT-000105\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T19:24:12.417\",\"job_created_by\":\"90b243a0-3ad5-4265-be03-08daad17f007\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"536050\",\"job_address\":\"25 Paya Lebar Cres\",\"job_customer_name\":\"d'Phenomenal Pte Ltd\",\"job_customer_code\":\"300-D017\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.882210328557\",\"job_latitude\":\"1.3499801733955\",\"job_end_date\":\"2022-12-13T00:00:00\",\"job_start_date\":\"2022-12-12T00:00:00\",\"job_primary_staff\":null},{\"schedule_job_id\":\"7610c4f0-9429-43ea-be32-cef23f7f0ae1\",\"schedule_job_schedule_id\":\"fe7a4998-f297-43b0-7c35-08dae23b31a3\",\"schedule_job_job_id\":\"09dddd7c-feb2-4698-524c-08daadd55e03\",\"schedule_job_order\":3,\"schedule_job_vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T11:35:20.807\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"2b0a8c06-f871-4969-539f-08daad961fdb\",\"vehicle_plat_no\":\"CAR0002\",\"vehicle_model\":\"Honda\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:56:36.7\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":null,\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"00000000-0000-0000-0000-000000000000\",\"job_id\":\"09dddd7c-feb2-4698-524c-08daadd55e03\",\"job_no\":\"JOB-00014\",\"job_quotation_no\":\"QT-000119\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T20:01:12.963\",\"job_created_by\":\"90b243a0-3ad5-4265-be03-08daad17f007\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"569880\",\"job_address\":\"7030 Ang Mo Kio Ave 5 #B1-01 Northstart\",\"job_customer_name\":\"Darwin Design & Developments Pte Ltd\",\"job_customer_code\":\"300-D004\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.875389838376\",\"job_latitude\":\"1.37792345221534\",\"job_end_date\":\"2022-10-20T00:00:00\",\"job_start_date\":\"2022-10-17T00:00:00\",\"job_primary_staff\":null}],\"CAR0003\":[{\"schedule_job_id\":\"3e765048-4ef4-45f5-a3dd-e7f8bd775ddb\",\"schedule_job_schedule_id\":\"fe7a4998-f297-43b0-7c35-08dae23b31a3\",\"schedule_job_job_id\":\"a1a50348-8c62-4257-5249-08daadd55e03\",\"schedule_job_order\":0,\"schedule_job_vehicle_id\":\"3489a63b-63b1-4b5f-53a0-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T11:35:20.807\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"3489a63b-63b1-4b5f-53a0-08daad961fdb\",\"vehicle_plat_no\":\"CAR0003\",\"vehicle_model\":\"Suzuki\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:56:48.847\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":null,\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"00000000-0000-0000-0000-000000000000\",\"job_id\":\"a1a50348-8c62-4257-5249-08daadd55e03\",\"job_no\":\"JOB-00011\",\"job_quotation_no\":\"QT-000106\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T19:23:22.443\",\"job_created_by\":\"90b243a0-3ad5-4265-be03-08daad17f007\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"650283\",\"job_address\":\"9 River Sound #01-26 Condo\",\"job_customer_name\":\"First SM Construction Pte Ltd\",\"job_customer_code\":\"300-F002\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.757539380939\",\"job_latitude\":\"1.34694365753899\",\"job_end_date\":\"2022-12-13T00:00:00\",\"job_start_date\":\"2022-12-08T00:00:00\",\"job_primary_staff\":null},{\"schedule_job_id\":\"fdbb932f-b610-492c-9a5a-8b3496c230f7\",\"schedule_job_schedule_id\":\"fe7a4998-f297-43b0-7c35-08dae23b31a3\",\"schedule_job_job_id\":\"693996da-7549-495c-524b-08daadd55e03\",\"schedule_job_order\":1,\"schedule_job_vehicle_id\":\"3489a63b-63b1-4b5f-53a0-08daad961fdb\",\"schedule_job_created_at\":\"2022-12-20T11:35:20.807\",\"schedule_job_created_by\":\"\",\"schedule_job_updated_at\":null,\"schedule_job_updated_by\":null,\"schedule_job_remark\":null,\"vehicle_id\":\"3489a63b-63b1-4b5f-53a0-08daad961fdb\",\"vehicle_plat_no\":\"CAR0003\",\"vehicle_model\":\"Suzuki\",\"vehicle_created_by\":\"5dd55afc-4138-4fd8-bcac-3dc0eedd243a\",\"vehicle_created_at\":\"2022-10-14T11:56:48.847\",\"vehicle_updated_by\":null,\"vehicle_updated_at\":null,\"vehicle_status\":\"Active\",\"vehicle_driver_id\":\"00000000-0000-0000-0000-000000000000\",\"job_id\":\"693996da-7549-495c-524b-08daadd55e03\",\"job_no\":\"JOB-00013\",\"job_quotation_no\":\"QT-000104\",\"job_remark\":\"undefined\",\"job_status\":\"Active\",\"job_created_at\":\"2022-10-14T19:25:03.29\",\"job_created_by\":\"90b243a0-3ad5-4265-be03-08daad17f007\",\"job_updated_at\":null,\"job_updated_by\":null,\"job_postal_code\":\"729935\",\"job_address\":\"25A Mandai Estate #05-04/05 Innovation Place\",\"job_customer_name\":\"Wee Guan Construction Pte Ltd\",\"job_customer_code\":\"300-W012\",\"job_entity_name\":\"Fui Builder\",\"job_longtitude\":\"103.759580459746\",\"job_latitude\":\"1.40631935420444\",\"job_end_date\":\"2022-12-15T00:00:00\",\"job_start_date\":\"2022-12-12T00:00:00\",\"job_primary_staff\":null}]}";
            Dictionary<string, List<dynamic>> schedules = JsonConvert.DeserializeObject<Dictionary<string, List<dynamic>>>(jsonSchedule);

            // set all job id to deleted
            var scheduleJobs = _Schedule_Context.Schedule_Job.Where(x => x.schedule_job_schedule_id.Equals(Guid.Parse(schedule_id)));
            if (scheduleJobs.Count() > 0)
            {
                _Schedule_Context.Schedule_Job.RemoveRange(scheduleJobs);
                _Schedule_Context.SaveChanges();
            }

            Schedule_Job.Dto.Post newScheduleJob = new Schedule_Job.Dto.Post();
            foreach(KeyValuePair<string,List<dynamic>> schedule in schedules)
            {
                newScheduleJob = new Schedule_Job.Dto.Post();
               
                Vehicle.Dto.Get vehicle = await Vehicle.Operations.ReadSingleByPlatNo(_Vehicle_Context, schedule.Key);
                newScheduleJob.schedule_job_vehicle_id = vehicle.vehicle_id;
                newScheduleJob.schedule_job_schedule_id = Guid.Parse(schedule_id);
                newScheduleJob.schedule_job_created_at = DateTime.Now;
                
                for (int i = 0; i < schedule.Value.Count(); i++)
                {
                    if (schedule.Value[i].job_id != null)
                    {
                        newScheduleJob.schedule_job_id = new Guid();
                        newScheduleJob.schedule_job_job_id = schedule.Value[i].job_id;
                        newScheduleJob.schedule_job_order = i;
                        newScheduleJob.schedule_job_schedule_id = Guid.Parse(schedule_id);
                        newScheduleJob.schedule_job_remark = schedule.Value[i].schedule_job_remark ?? "";
                        await Schedule_Job.Operations.Create(_Schedule_Context, newScheduleJob);
                        dynamic _schedule = schedule.Value[i];
                        Guid schedule_job_id = Guid.Parse(_schedule["schedule_job_id"].ToString());

                        if (_schedule["tools"] != null)
                        {
                            var tools = _schedule["tools"];
                            // clear all tools
                            var scheduleJobTools = _Schedule_Context.Schedule_Job_Tool.Where(x => x.sjt_schedule_job_id.Equals(schedule_job_id));
                            _Schedule_Context.Schedule_Job_Tool.RemoveRange(scheduleJobTools);
                            _Schedule_Context.SaveChanges();
                            Schedule_Job_Tool.Dto.Post scheduleTool = new Schedule_Job_Tool.Dto.Post();

                            for (int i1 = 0; i1 < tools.Count; i1++)
                            {
                                scheduleTool = new Schedule_Job_Tool.Dto.Post();
                                scheduleTool.sjt_id = new Guid();
                                scheduleTool.sjt_tool_id = tools[i1]["tool_id"];
                                scheduleTool.sjt_schedule_job_id = schedule_job_id;
                                scheduleTool.sjt_status = "Active";
                                scheduleTool.sjt_created_at = DateTime.Now;
                                await Schedule_Job_Tool.Operations.Create(_Schedule_Context, scheduleTool);
                            }
                        }

                        if (_schedule["materials"] != null)
                        {
                            var materials = _schedule["materials"];
                            // clear all material
                            var scheduleJobMaterials = _Schedule_Context.Schedule_Job_Material.Where(x => x.sjm_schedule_job_id.Equals(schedule_job_id));
                            _Schedule_Context.Schedule_Job_Material.RemoveRange(scheduleJobMaterials);
                            _Schedule_Context.SaveChanges();
                            Schedule_Job_Material.Dto.Post scheduleMaterial = new Schedule_Job_Material.Dto.Post();
                            for (int i2 = 0; i2 < materials.Count; i2++)
                            {
                                scheduleMaterial = new Schedule_Job_Material.Dto.Post();
                                scheduleMaterial.sjm_id = new Guid();
                                scheduleMaterial.sjm_material_id = materials[i2]["material_id"];
                                scheduleMaterial.sjm_schedule_job_id = schedule_job_id;
                                scheduleMaterial.sjm_status = "Active";
                                scheduleMaterial.sjm_created_at = DateTime.Now;
                                scheduleMaterial.sjm_quantity = materials[i2]["material_quantity"];
                                await Schedule_Job_Material.Operations.Create(_Schedule_Context, scheduleMaterial);
                            }

                        }

                        if (_schedule["workers"] != null)
                        {
                            var workers = _schedule["workers"];
                            // clear all worker
                            var scheduleJobWorkers = _Schedule_Context.Schedule_Job_Worker.Where(x => x.sjw_schedule_job_id.Equals(schedule_job_id));
                            _Schedule_Context.Schedule_Job_Worker.RemoveRange(scheduleJobWorkers);
                            _Schedule_Context.SaveChanges();
                            Schedule_Job_Worker.Dto.Post scheduleWorker = new Schedule_Job_Worker.Dto.Post();
                            for (int i3 = 0; i3 < workers.Count; i3++)
                            {
                                scheduleWorker = new Schedule_Job_Worker.Dto.Post();
                                scheduleWorker.sjw_id = new Guid();
                                scheduleWorker.sjw_worker_id = workers[i3]["user_id"];
                                scheduleWorker.sjw_schedule_job_id = schedule_job_id;
                                scheduleWorker.sjw_status = "Active";
                                scheduleWorker.sjw_created_at = DateTime.Now;
                                await Schedule_Job_Worker.Operations.Create(_Schedule_Context, scheduleWorker);
                            }

                        }

                       /* string remarks = _schedule["remarks"].ToString();
                        Schedule_Job scheduleJob = _Schedule_Context.Schedule_Job.Where(x => x.schedule_job_id.Equals(schedule_job_id)).FirstOrDefault();
                        scheduleJob.schedule_job_remark = remarks;
                        await Schedule_Job.Operations.Update(_Schedule_Context, (Schedule_Job.Dto.Put)scheduleJob);*/

                    }

                }
            }
            return StatusCode(200, true); 
            /*bool status = await Schedule.Operations.Update(_Schedule_Context, scheduleScheme);

            if (status)
            {
                return StatusCode(200, scheduleScheme);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }*/
        }

        #endregion

        #region schedule job material
        [HttpGet]
        [Route("schedulejobmaterialswithdetails")]
        public async Task<IActionResult> getScheduleJobMaterialsWithDetails(string schedule_job_id)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from schedule_job_material inner join material on material_id=sjm_material_id where sjm_schedule_job_id=@schedule_job_id", connection);
                    cm.Parameters.AddWithValue("@schedule_job_id", schedule_job_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    List<dynamic> schedulejobmaterials = new List<dynamic>();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<dynamic>();
                            dynamic schedulejobmaterial = parser(sdr);
                            schedulejobmaterials.Add(schedulejobmaterial);
                        }
                    }
                    return new JsonResult(schedulejobmaterials);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }
        }
        [HttpPut]
        [Route("removeScheduleJobMaterial")]
        public async Task<IActionResult> removeScheduleJobMaterial(string sjm_id,string sjm_updated_by)
        {
            Schedule_Job_Material.Dto.Put scheduleJobMaterial = new Schedule_Job_Material.Dto.Put();
            scheduleJobMaterial.sjm_status = "Deleted";
            scheduleJobMaterial.sjm_id = Guid.Parse(sjm_id);
            scheduleJobMaterial.sjm_updated_at = DateTime.Now;
            scheduleJobMaterial.sjm_updated_by = sjm_updated_by;
            
            bool status = await Schedule_Job_Material.Operations.Update(_Schedule_Context, scheduleJobMaterial);

            if (status)
            {
                return StatusCode(200, scheduleJobMaterial);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        [HttpPost]
        [Route("addScheduleJobMaterial")]
        public async Task<IActionResult> addScheduleJobMaterial(Schedule_Job_Material.Dto.Post scheduleJobMaterial)
        {  
            bool status = await Schedule_Job_Material.Operations.Create(_Schedule_Context, scheduleJobMaterial);

            if (status)
            {
                return StatusCode(200, scheduleJobMaterial);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        #endregion
        #region schedule job tool
        [HttpGet]
        [Route("schedulejobtoolswithdetails")]
        public async Task<IActionResult> getScheduleJobToolsWithDetails(string schedule_job_id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from schedule_job_tool inner join tool on tool_id=sjt_tool_id where sjt_schedule_job_id=@schedule_job_id", connection);
                    cm.Parameters.AddWithValue("@schedule_job_id", schedule_job_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    List<dynamic> schedulejobtools = new List<dynamic>();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<dynamic>();
                            dynamic schedulejobtool = parser(sdr);
                            schedulejobtools.Add(schedulejobtool);
                        }
                    }
                    return new JsonResult(schedulejobtools);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            } 
        }
        [HttpPut]
        [Route("removeScheduleJobTool")]
        public async Task<IActionResult> removeScheduleJobTool(string sjt_id, string sjt_updated_by)
        {
            Schedule_Job_Tool.Dto.Put scheduleJobTool = new Schedule_Job_Tool.Dto.Put();
            scheduleJobTool.sjt_status = "Deleted";
            scheduleJobTool.sjt_id = Guid.Parse(sjt_id);
            scheduleJobTool.sjt_updated_at = DateTime.Now;
            scheduleJobTool.sjt_updated_by = sjt_updated_by;

            bool status = await Schedule_Job_Tool.Operations.Update(_Schedule_Context, scheduleJobTool);

            if (status)
            {
                return StatusCode(200, scheduleJobTool);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }
        [HttpPost]
        [Route("addScheduleJobTool")]
        public async Task<IActionResult> addScheduleJobTool(Schedule_Job_Tool.Dto.Post scheduleJobTool)
        {
            bool status = await Schedule_Job_Tool.Operations.Create(_Schedule_Context, scheduleJobTool);

            if (status)
            {
                return StatusCode(200, scheduleJobTool);
            }
            else
            {
                return StatusCode(404, string.Format("Could not find config"));
            }
        }

        #endregion
        #region schedule job worker
        [HttpGet]
        [Route("schedulejobworkerswithdetails")]
        public async Task<IActionResult> getScheduleJobWorkersWithDetails(string schedule_job_id)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from schedule_job_worker inner join [user] on [user].user_id=sjw_worker_id where sjw_schedule_job_id=@schedule_job_id", connection);
                    cm.Parameters.AddWithValue("@schedule_job_id", schedule_job_id);
                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    List<dynamic> schedulejobworkers = new List<dynamic>();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<dynamic>();
                            dynamic schedulejobworker = parser(sdr);
                            schedulejobworkers.Add(schedulejobworker);
                        }
                    }
                    return new JsonResult(schedulejobworkers);
                }
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }
        }
        #endregion
        [HttpGet]
        [Route("generateRoute")]
        public IActionResult generateRoute(string schedule_id, string generationType)
        { 
            try
            {
                #region get all active job
                List<Job> jobs = new List<Job>();
                List<Vehicle> vehicles = new List<Vehicle>();
                Dictionary<string, Dictionary<string, string>> carLastPoints= new Dictionary<string, Dictionary<string, string>>();
                Dictionary<string, List<string>> carJobs = new Dictionary<string, List<string>>();
                Dictionary<string, Guid> vehicleIDs = new Dictionary<string, Guid>();
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [job] where job_status='Active'", connection);

                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<Job>(typeof(Job));
                            Job job = parser(sdr);
                            jobs.Add(job);
                        }
                    }
                }
                #endregion
                #region get all active vehicle
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    // Creating SqlCommand objcet   
                    SqlCommand cm = new SqlCommand("select * from [vehicle] where vehicle_status='Active'", connection);

                    // Opening Connection  
                    connection.Open();
                    // Executing the SQL query  
                    SqlDataReader sdr = cm.ExecuteReader();
                    
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            var parser = sdr.GetRowParser<Vehicle>(typeof(Vehicle));
                            Vehicle vehicle = parser(sdr);
                            vehicles.Add(vehicle);
                        }
                    }
                }
                #endregion

                int maxJobTake = (jobs.Count / vehicles.Count) + 1;

                    
                //729908 office postal code
                string token = this.getOneMapToken();
                dynamic coords = this.getLongLat("729908");

                for (int i = 0; i < vehicles.Count; i++)
                {
                    Dictionary<string, string> carLastPoint = new Dictionary<string, string>();
                    if (!carLastPoints.ContainsKey(vehicles[i].vehicle_plat_no))
                    {
                        carLastPoint.Add("latitude", coords.Value.latitude);
                        carLastPoint.Add("longtitude", coords.Value.longtitude);
                        carLastPoints.Add(vehicles[i].vehicle_plat_no, carLastPoint);
                        vehicleIDs.Add(vehicles[i].vehicle_plat_no, vehicles[i].vehicle_id);
                    }
                }
                for (int k = 0; k < jobs.Count; k++)
                {
                    string jobPostalCode = jobs[k].job_postal_code;
                    dynamic jobCoords = this.getLongLat(jobPostalCode);
                    string endPoint = jobCoords.Value.latitude + "," + jobCoords.Value.longtitude;
                    float shortest = 0;
                    string winnerPlatNo = ""; 
                      
                    foreach (KeyValuePair<string, Dictionary<string, string>> car in carLastPoints)
                    {
                        string carPlat = car.Key;
                        if (carJobs.ContainsKey(carPlat) && carJobs[carPlat].Count > maxJobTake)
                        {
                            continue;
                        }
                            string longtitude = car.Value["longtitude"];
                            string latitude = car.Value["latitude"];

                            string startPoint = car.Value["latitude"] + "," + car.Value["longtitude"];
                            dynamic calculatedDistance = this.getSiteDistance(startPoint, endPoint, token, generationType);
                            if (shortest == 0 || (float.Parse(calculatedDistance.Value.distance) < shortest))
                            {
                                shortest = float.Parse(calculatedDistance.Value.distance);
                                winnerPlatNo = carPlat;
                            }
                        
                    }

                    if (carJobs.ContainsKey(winnerPlatNo))
                    {
                        List<string> carJob = carJobs[winnerPlatNo];
                        carJob.Add(jobs[k].job_id.ToString());
                        
                    }
                    else
                    {
                        List<string> carJob = new List<string> ();
                        carJob.Add(jobs[k].job_id.ToString());
                        carJobs.Add(winnerPlatNo, carJob);
                    }
                    Dictionary<string, string> carLastPoint = carLastPoints[winnerPlatNo];
                    carLastPoint["latitude"] = jobCoords.Value.latitude;
                    carLastPoint["longtitude"] = jobCoords.Value.longtitude;
                    carLastPoints[winnerPlatNo] = carLastPoint;

                }


                #region loop to insert job
                if (carJobs.Count > 0)
                {
                    DateTime currentDt = DateTime.Now;
                    foreach(KeyValuePair<string,List<string>> car in carJobs)
                    {
                        for (int z = 0; z < car.Value.Count; z++)
                        {
                            using (SqlConnection connection = new SqlConnection(_connStr))
                            {
                                // Creating SqlCommand objcet   
                                SqlCommand cm = new SqlCommand("insert into [schedule_job] " +
                                    "(schedule_job_id,schedule_job_schedule_id, schedule_job_job_id, schedule_job_order, schedule_job_vehicle_id, schedule_job_created_by, schedule_job_created_at) values " +
                                    "(NEWID(),@schedule_job_schedule_id, @schedule_job_job_id, @schedule_job_order, @schedule_job_vehicle_id, @schedule_job_created_by, @schedule_job_created_at)", connection);
                                 
                                cm.Parameters.AddWithValue("@schedule_job_schedule_id", schedule_id);
                                cm.Parameters.AddWithValue("@schedule_job_job_id", car.Value[z]);
                                cm.Parameters.AddWithValue("@schedule_job_order", z);
                                cm.Parameters.AddWithValue("@schedule_job_vehicle_id", vehicleIDs[car.Key]); 
                                cm.Parameters.AddWithValue("@schedule_job_created_at", currentDt);
                                cm.Parameters.AddWithValue("@schedule_job_created_by", "");

                                // Opening Connection  
                                connection.Open();
                                // Executing the SQL query  
                                int result = cm.ExecuteNonQuery();
                                if (result > 0)
                                {
                                    
                                }
                                else
                                {
                                    return new JsonResult("Error inserting");
                                }

                            }
                        }
                    }

                    Schedule schedule = _Schedule_Context.Schedule.Where(x => x.schedule_id.Equals(Guid.Parse(schedule_id))).FirstOrDefault();
                    schedule.schedule_status = "Completed";
                    schedule.schedule_updated_by = "";
                    schedule.schedule_updated_at = DateTime.Now;
                    _Schedule_Context.Update(schedule);
                    _Schedule_Context.SaveChanges();
                }
                return new JsonResult(carJobs);

                #endregion
            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }

        }


        [HttpGet]
        [Route("getSiteDistance")]
        public IActionResult getSiteDistance(string startPoint, string endPoint, string token, string returnType)
        {
            // get user & Password
            try
            {
                var url = "https://developers.onemap.sg/privateapi/routingsvc/route";
                url += "?start=" + startPoint + "&end=" + endPoint + "&routeType=drive&token=" + token;

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);

                httpRequest.Accept = "application/json";


                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var jResult = JsonConvert.DeserializeObject<dynamic>(result);
                    if (returnType == "distance")
                    {
                        return new JsonResult(new { distance = jResult.route_summary.total_distance.ToString() });
                    }
                    else
                    {
                        return new JsonResult(new { time = jResult.route_summary.total_time.ToString() });
                    }
                }


            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }

        }
         
        private IActionResult getLongLat(string postalCode)
        {
            // get user & Password
            try
            {
                var url = "https://developers.onemap.sg/commonapi/search?searchVal=" + postalCode + "&returnGeom=Y&getAddrDetails=Y&pageNum=1";

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);

                httpRequest.Accept = "application/json";


                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var jResult = JsonConvert.DeserializeObject<dynamic>(result);
                    if (jResult.results.Count > 0)
                    {
                        return new JsonResult(new
                        {
                            latitude = jResult.results[0]["LATITUDE"].ToString(),
                            longtitude = jResult.results[0]["LONGTITUDE"].ToString(),
                        });
                    }

                    else
                    {
                        return new JsonResult(new { postal_code = "Address not found" });
                    }

                }


            }
            catch (Exception e)
            {
                return new JsonResult("OOPs, something went wrong.\n" + e);
            }

        }

         
        private string getOneMapToken()
        {
            try
            {
                var url = "https://developers.onemap.sg/privateapi/auth/post/getToken";

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                var postData = "email=" + Uri.EscapeDataString("fuibuilderspl@gmail.com");
                postData += "&password=" + Uri.EscapeDataString("z3Kc2buYTZ");
                var data = Encoding.ASCII.GetBytes(postData);

                httpRequest.Method = "POST";

                httpRequest.ContentType = "application/x-www-form-urlencoded";
                httpRequest.ContentLength = data.Length;
                using (var stream = httpRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    var jResult = JsonConvert.DeserializeObject<dynamic>(result);
                    if (!string.IsNullOrEmpty(jResult.access_token.ToString()))
                    {
                        return jResult.access_token;
                    }
                    else
                    {
                        return "";
                    }

                }


            }
            catch (Exception e)
            {
                return "OOPs, something went wrong.\n" + e;
            }
        }


    }
}
