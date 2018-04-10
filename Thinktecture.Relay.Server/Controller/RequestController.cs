using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Serilog;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Http.Filters;
using Thinktecture.Relay.Server.SignalR;

namespace Thinktecture.Relay.Server.Controller
{
	[Authorize(Roles = "OnPremise")]
	[OnPremiseConnectionModuleBindingFilter]
	[NoCache]
	public class RequestController : ApiController
	{
		private readonly ILogger _logger;
		private readonly IPostDataTemporaryStore _temporaryStore;
		private readonly IBackendCommunication _backendCommunication;

		public RequestController(ILogger logger, IPostDataTemporaryStore temporaryStore, IBackendCommunication backendCommunication)
		{
			_logger = logger;
			_temporaryStore = temporaryStore ?? throw new ArgumentNullException(nameof(temporaryStore));
			_backendCommunication = backendCommunication ?? throw new ArgumentNullException(nameof(backendCommunication));
		}

		public IHttpActionResult Get(string requestId)
		{
			_logger?.Verbose("Getting request data. request-id={RequestId}", requestId);

			var stream = _temporaryStore.GetRequestStream(requestId);
			if (stream == null)
			{
				_logger?.Warning("No request data found. request-id={RequestId}", requestId);
				return NotFound();
			}

			return new ResponseMessageResult(new HttpResponseMessage() { Content = new StreamContent(stream, 0x10000) });
		}

		[HttpGet]
		public IHttpActionResult Acknowledge([FromUri(Name = "oid")] Guid originId, [FromUri(Name = "aid")] string acknowledgeId, [FromUri(Name = "cid")] string connectionId = null)
		{
			_logger?.Verbose("Received acknowledge. origin-id={OriginId}, acknowledge-id={AcknowledgeId}, connection-id={ConnectionId}", originId, acknowledgeId, connectionId);
			_backendCommunication.AcknowledgeOnPremiseConnectorRequestAsync(originId, acknowledgeId, connectionId);

			return Ok();
		}
	}
}
