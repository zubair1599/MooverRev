using Business.Interfaces;
using System.Web.Http;
using WebServiceModels;

namespace Moovers.Webservices.Controllers
{
    public class EmployeeController : ControllerBase
    {
        private IMoverScheduleRepository _scheduleRepo;

        public EmployeeController(IMoverScheduleRepository scheduleRepo)
        {
            this._scheduleRepo = scheduleRepo;
        }

        //[Authorize]
        [HttpGet]
        [Route("employee")]
        // GET /employee
        public EmployeeRepresentation Get()
        {
            var employee = this.GetCurrentUser();
            return new EmployeeRepresentation(employee);
        }
    }
}