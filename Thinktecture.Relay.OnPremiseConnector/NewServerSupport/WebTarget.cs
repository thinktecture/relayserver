using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector;
using Thinktecture.Relay.Connector.RelayTargets;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.OnPremiseConnector.NewServerSupport
{
	/// <inheritdoc />
	public class WebTarget : RelayWebTarget
	{
		private readonly ILogger<RelayWebTarget<ClientRequest, TargetResponse>> _logger;

		/// <inheritdoc />
		public WebTarget(ILogger<RelayWebTarget<ClientRequest, TargetResponse>> logger, IRelayTargetResponseFactory<TargetResponse> responseFactory, IHttpClientFactory clientFactory, Uri baseAddress)
			: base(logger, responseFactory, clientFactory, baseAddress)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public WebTarget(ILogger<RelayWebTarget<ClientRequest, TargetResponse>> logger, IRelayTargetResponseFactory<TargetResponse> responseFactory, IHttpClientFactory clientFactory, Dictionary<String, String> parameters)
			: base(logger, responseFactory, clientFactory, parameters)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
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

			if (!String.IsNullOrWhiteSpace(NewServerConnection._relayedRequestHeader))
			{
				message.Headers.Add(NewServerConnection._relayedRequestHeader, "true");
			}

			return message;
		}
	}
}
