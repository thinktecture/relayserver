using System;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using NLog;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Controller
{
	[Authorize(Roles = "OnPremise")]
	[OnPremiseConnectionModuleBindingFilter]
	public class ResponseController : ApiController
	{
		private readonly ILogger _logger;
		private readonly IBackendCommunication _backendCommunication;

		public ResponseController(IBackendCommunication backendCommunication, ILogger logger)
		{
			_logger = logger;
			_backendCommunication = backendCommunication ?? throw new ArgumentNullException(nameof(backendCommunication));
		}

		public async Task<IHttpActionResult> Forward(JToken message)
		{
			var onPremiseTargetResponse = message.ToObject<OnPremiseConnectorResponse>();

			_logger?.Trace("{0}.{1} - Forwarding RequestId: {2}, OriginId: {3}, RequestStarted: {4}, RequestFinished: {5}, StatusCode: {6}, HttpHeaders: {7}",
				nameof(ResponseController),
				nameof(Forward),
				onPremiseTargetResponse.RequestId,
				onPremiseTargetResponse.OriginId,
				onPremiseTargetResponse.RequestStarted,
				onPremiseTargetResponse.RequestFinished,
				onPremiseTargetResponse.StatusCode,
				onPremiseTargetResponse.HttpHeaders
			);

			await _backendCommunication.SendOnPremiseTargetResponse(onPremiseTargetResponse.OriginId, onPremiseTargetResponse).ConfigureAwait(false);

			return Ok();
		}
	}
}
