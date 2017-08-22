using System.Web.Http;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Controller.ManagementWeb
{
    [Authorize(Roles = "Admin")]
    [ManagementWebModuleBindingFilter]
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
			var result = new Dashboard()
			{
				Logs = _logRepository.GetRecentLogEntries(15),
				ContentBytesChartDataItems = _logRepository.GetContentBytesChartDataItems()
			};

			return Ok(result);
		}
	}
}
