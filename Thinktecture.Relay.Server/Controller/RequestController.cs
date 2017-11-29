using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Serilog;
using Thinktecture.Relay.Server.Http.ActionFilters;
using Thinktecture.Relay.Server.SignalR;

namespace Thinktecture.Relay.Server.Controller
{
	[Authorize(Roles = "OnPremise")]
	[OnPremiseConnectionModuleBindingFilter]
	[NoCache]
	public class RequestController : ApiController
	{
		private readonly IPostDataTemporaryStore _temporaryStore;
		private readonly ILogger _logger;

		public RequestController(IPostDataTemporaryStore temporaryStore, ILogger logger)
		{
			_temporaryStore = temporaryStore;
			_logger = logger;
		}

		public IHttpActionResult Get(string requestId)
		{
			_logger?.Verbose("Getting request data. request-id={RequestId}", requestId);

			var stream = _temporaryStore.GetRequestStream(requestId);
			if (stream == null)
			{
				_logger?.Warning("No request data found for request {RequestId}", requestId);
				return NotFound();
			}

			return new ResponseMessageResult(new HttpResponseMessage() { Content = new StreamContent(stream) });
		}
	}
}
