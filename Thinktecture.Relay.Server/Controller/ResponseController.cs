using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
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

		public ResponseController(ILifetimeScope scope, ILogger logger)
		{
			_logger = logger;
			_backendCommunication = scope.Resolve<IBackendCommunication>();
		}

		public async Task<IHttpActionResult> Forward(JToken message)
		{
			_logger.Trace("Forwarding {0}", message);

			var onPremiseTargetResponse = message.ToObject<OnPremiseConnectorResponse>();

			await _backendCommunication.SendOnPremiseTargetResponse(onPremiseTargetResponse.OriginId, onPremiseTargetResponse);

			return Ok();
		}
	}
}
