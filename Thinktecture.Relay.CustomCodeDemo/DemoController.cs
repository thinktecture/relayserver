using System.Web.Http;
using Serilog;

namespace Thinktecture.Relay.CustomCodeDemo
{
	public class DemoController : ApiController
	{
		private readonly ILogger _logger;

		public DemoController(ILogger logger)
		{
			_logger = logger;
		}

		[HttpGet]
		[Route("relay/demo")]
		public string DemoAction()
		{
			_logger?.Information("Executing demo action");
			return "Demo works!";
		}
	}
}
