using System.Web.Http;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Controller.ManagementWeb
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : ApiController
    {
        private readonly ILogRepository _logRepository;

        public DashboardController(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        [HttpGet]
        [ActionName("info")]
        public IHttpActionResult Get()
        {
            var result = new Dashboard();

            result.Logs = _logRepository.GetRecentLogEntries(15);
            result.ContentBytesChartDataItems = _logRepository.GetContentBytesChartDataItems();

            return Ok(result);
        } 
    }
}