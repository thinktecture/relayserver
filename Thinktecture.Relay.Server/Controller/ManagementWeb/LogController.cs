using System;
using System.Web.Http;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Controller.ManagementWeb
{
	public class LogController : ApiController
	{
		private readonly ILogRepository _logRepository;

		public LogController(ILogRepository logRepository)
		{
			_logRepository = logRepository;
		}

		[HttpGet]
		[ActionName("recentlog")]
		public IHttpActionResult GetLatestLogsForLink(Guid id)
		{
			var logs = _logRepository.GetRecentLogEntriesForLink(id, 10);

			return Ok(logs);
		}

		[HttpGet]
		[ActionName("chartcontentbytes")]
		public IHttpActionResult GetContentBytesChartData(Guid id, [FromUri] TimeFrame timeFrame)
		{
			if (timeFrame == null)
			{
				return BadRequest();
			}

			var chartData = _logRepository.GetContentBytesChartDataItemsForLink(id, timeFrame);

			return Ok(chartData);
		}
	}
}
