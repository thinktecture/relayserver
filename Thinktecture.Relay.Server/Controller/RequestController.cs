using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using NLog;
using Thinktecture.Relay.Server.SignalR;

namespace Thinktecture.Relay.Server.Controller
{
	[Authorize(Roles = "OnPremise")]
	[OnPremiseConnectionModuleBindingFilter]
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
			_logger?.Trace("Getting request data. request-id={0}", requestId);

			var stream = _temporaryStore.GetRequestStream(requestId);
			if (stream == null)
			{
				_logger?.Warn("No request data found for request id {0}", requestId);
				return NotFound();
			}

			return new ResponseMessageResult(new HttpResponseMessage() { Content = new StreamContent(stream) });
		}
	}
}
