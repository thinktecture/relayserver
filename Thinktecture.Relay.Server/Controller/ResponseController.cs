using Autofac;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.SignalR;

namespace Thinktecture.Relay.Server.Controller
{
	[Authorize(Roles = "OnPremise")]
	[OnPremiseConnectionModuleBindingFilter]
	public class ResponseController : ApiController
	{
		private readonly ILogger _logger;
		private readonly IPostDataTemporaryStore _postDataTemporaryStore;
		private readonly IBackendCommunication _backendCommunication;

		public ResponseController(IBackendCommunication backendCommunication, ILogger logger, IPostDataTemporaryStore postDataTemporaryStore)
		{
			_logger = logger;
			_backendCommunication = backendCommunication ?? throw new ArgumentNullException(nameof(backendCommunication));
			_postDataTemporaryStore = postDataTemporaryStore ?? throw new ArgumentNullException(nameof(postDataTemporaryStore));
		}

		public async Task<IHttpActionResult> Forward(JToken message)
		{
			// TODO check for header or fallback to legacy

			// This is needed because the class is internal
			var onPremiseTargetResponse = message.ToObject<OnPremiseConnectorResponse>();

			// Store response to file, if we get one and it is larger than 64 kByte
			if (onPremiseTargetResponse.Body?.Length >= 0x10000)
			{
				_postDataTemporaryStore.SaveResponse(onPremiseTargetResponse.RequestId, onPremiseTargetResponse.Body);
				onPremiseTargetResponse.Body = new byte[0]; // this marks that there is a larger body available
			}

			await _backendCommunication.SendOnPremiseTargetResponse(onPremiseTargetResponse.OriginId, onPremiseTargetResponse).ConfigureAwait(false);

			return Ok();
		}

		public async Task<IHttpActionResult> Upload(string requestId)
		{
			_logger?.Debug($"{nameof(ResponseController)}: Upload body called for request {{0}}", requestId);

			using (var stream = _postDataTemporaryStore.CreateResponseStream(requestId))
			{
				await Request.Content.CopyToAsync(stream);
			}

			return Ok();
		}
	}
}
