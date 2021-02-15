using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Http.Filters;
using Thinktecture.Relay.Server.IO;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Http;
using System.Security.Claims;
using Thinktecture.Relay.Server.Security;

namespace Thinktecture.Relay.Server.Controller
{
	[Authorize(Roles = "OnPremise")]
	[OnPremiseConnectionModuleBindingFilter]
	[NoCache]
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

		public async Task<IHttpActionResult> Forward()
		{
			var claimsPrincipal = (ClaimsPrincipal)RequestContext.Principal;
			var onPremiseId = claimsPrincipal.FindFirst(AuthorizationServerProvider.OnPremiseIdClaimName)?.Value;
			Request.GetCallCancelled().Register(() => _logger?.Warning("Disconnect during receiving on-premise response detected. link-id={LinkId}", onPremiseId));

			var requestStream = await Request.Content.ReadAsStreamAsync().ConfigureAwait(false);

			OnPremiseConnectorResponse response = null;
			if (Request.Headers.TryGetValues("X-TTRELAY-METADATA", out var headerValues))
			{
				response = JToken.Parse(headerValues.First()).ToObject<OnPremiseConnectorResponse>();

				using (var stream = _postDataTemporaryStore.CreateResponseStream(response.RequestId))
				{
					await requestStream.CopyToAsync(stream).ConfigureAwait(false);
					response.ContentLength = stream.Length;
				}
			}
			else
			{
				// this is a legacy on-premise connector (v1)
				response = await ForwardLegacyResponse(Encoding.UTF8, requestStream).ConfigureAwait(false);
			}

			_logger?.Verbose("Received on-premise response. request-id={RequestId}, content-length={ResponseContentLength}", response.RequestId, response.ContentLength);

			_backendCommunication.SendOnPremiseTargetResponse(response.OriginId, response);

			return Ok();
		}

		private async Task<OnPremiseConnectorResponse> ForwardLegacyResponse(Encoding encoding, Stream requestStream)
		{
			using (var stream = new LegacyResponseStream(requestStream, _postDataTemporaryStore, _logger))
			{
				var temporaryId = await stream.ExtractBodyAsync().ConfigureAwait(false);

				using (var reader = new JsonTextReader(new StreamReader(stream)))
				{
					var message = await JToken.ReadFromAsync(reader).ConfigureAwait(false);
					var response = message.ToObject<OnPremiseConnectorResponse>();

					response.Body = null;
					response.ContentLength = _postDataTemporaryStore.RenameResponseStream(temporaryId, response.RequestId);

					_logger?.Verbose("Extracted legacy on-premise response. request-id={RequestId}", response.RequestId);

					return response;
				}
			}
		}
	}
}
