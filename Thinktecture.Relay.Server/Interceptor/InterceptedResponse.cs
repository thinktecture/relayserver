using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Interceptor
{
	internal class InterceptedResponse : OnPremiseConnectorResponse, IInterceptedResponse, IInterceptedStream
	{
		private readonly ILogger _logger;

		public Stream Content
		{
			get => this.GetContentStream(_logger);
			set => this.SetContentStream(value, _logger);
		}

		public InterceptedResponse(ILogger logger, IOnPremiseConnectorResponse other)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			RequestId = other.RequestId;
			OriginId = other.OriginId;
			RequestStarted = other.RequestStarted;
			RequestFinished = other.RequestFinished;
			StatusCode = other.StatusCode;
			HttpHeaders = other.HttpHeaders;
			Body = other.Body; // this is because of legacy on-premise connectors (v1)
			Stream = other.Stream;
			ContentLength = other.ContentLength;
		}

		public Dictionary<string, string> CloneHttpHeaders()
		{
			return HttpHeaders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}
	}
}
