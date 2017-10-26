using Autofac;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
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

		public async Task<IHttpActionResult> Forward()
		{
			var response = Request.Headers.TryGetValues("X-TTRELAY-METADATA", out var headerValues)
				? JsonConvert.DeserializeObject<OnPremiseConnectorResponse>(headerValues.First())
				: JToken.Parse(await Request.Content.ReadAsStringAsync().ConfigureAwait(false)).ToObject<OnPremiseConnectorResponse>();

			if (response.Body != null)
			{
				// this is used by v1 OnPremiseConnectors only
				if (response.Body.Length >= 0x10000)
				{
					_postDataTemporaryStore.SaveResponse(response.RequestId, response.Body);
					response.Body = new byte[0]; // this marks that there is a larger body available in the store
				}
			}
			else
			{
				using (var stream = _postDataTemporaryStore.CreateResponseStream(response.RequestId))
				{
					var requestStream = await Request.Content.ReadAsStreamAsync().ConfigureAwait(false);
					await requestStream.CopyToAsync(stream).ConfigureAwait(false);
				}
			}

			await _backendCommunication.SendOnPremiseTargetResponse(response.OriginId, response).ConfigureAwait(false);

			return Ok();
		}
	
	}
}
