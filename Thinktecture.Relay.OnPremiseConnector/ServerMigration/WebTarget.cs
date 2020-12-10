using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector.Targets;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.OnPremiseConnector.ServerMigration
{
	internal class WebTarget : RelayWebTarget
	{
		private readonly ILogger<RelayWebTarget> _logger;

		public WebTarget(ILogger<RelayWebTarget> logger, IHttpClientFactory httpClientFactory, Dictionary<string, string> parameters)
			: base(logger, httpClientFactory, parameters)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public WebTarget(ILogger<RelayWebTarget> logger, IHttpClientFactory httpClientFactory, Uri baseAddress, RelayWebTargetOptions options = RelayWebTargetOptions.None)
			: base(logger, httpClientFactory, baseAddress, options)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		protected override HttpRequestMessage CreateHttpRequestMessage(ClientRequest request)
		{
			var message = base.CreateHttpRequestMessage(request);

			if (request.AcknowledgeMode == AcknowledgeMode.Manual)
			{
				_logger.LogTrace("Request needs to be manually acknowledged, adding header. request-id={RequestId}, acknowledgment-mode={AcknowledgmentMode}, acknowledge-id={AcknowledgeId}", request.RequestId, request.AcknowledgeMode, request.RequestId);
				message.Headers.Add("X-TTRELAY-ACKNOWLEDGE-ORIGIN-ID", request.AcknowledgeOriginId.ToString());
				message.Headers.Add("X-TTRELAY-ACKNOWLEDGE-ID", request.RequestId.ToString());
				message.Headers.Add("X-TTRELAY-CONNECTION-ID", request.RequestId.ToString());
			}

			if (!string.IsNullOrWhiteSpace(RelayServerConnector.GetRelayedRequestHeader()))
			{
				message.Headers.Add(RelayServerConnector.GetRelayedRequestHeader(), "true");
			}

			return message;
		}
	}
}
