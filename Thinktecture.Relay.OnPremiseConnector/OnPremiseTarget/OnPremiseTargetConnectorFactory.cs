using System;
using Serilog;
using Thinktecture.Relay.OnPremiseConnector.Net.Http;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseTargetConnectorFactory : IOnPremiseTargetConnectorFactory
	{
		private readonly ILogger _logger;
		private readonly Func<IOnPremiseWebTargetRequestMessageBuilder> _requestMessageBuilderFactory;
		private readonly IHttpClientFactory _httpClientFactory;

		public OnPremiseTargetConnectorFactory(ILogger logger, Func<IOnPremiseWebTargetRequestMessageBuilder> requestMessageBuilderFactory, IHttpClientFactory httpClientFactory)
		{
			_logger = logger;
			_requestMessageBuilderFactory = requestMessageBuilderFactory ?? throw new ArgumentNullException(nameof(requestMessageBuilderFactory));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		}

		public IOnPremiseTargetConnector Create(Uri baseUri, TimeSpan requestTimeout, bool followRedirects = true)
		{
			return new OnPremiseWebTargetConnector(baseUri, requestTimeout, _logger, _requestMessageBuilderFactory(), _httpClientFactory, followRedirects);
		}

		public IOnPremiseTargetConnector Create(Type handlerType, TimeSpan requestTimeout)
		{
			return new OnPremiseInProcTargetConnector(_logger, requestTimeout, handlerType);
		}

		public IOnPremiseTargetConnector Create(Func<IOnPremiseInProcHandler> handlerFactory, TimeSpan requestTimeout)
		{
			return new OnPremiseInProcTargetConnector(_logger, requestTimeout, handlerFactory);
		}

		public IOnPremiseTargetConnector Create<T>(TimeSpan requestTimeout)
			where T : IOnPremiseInProcHandler, new()
		{
			return new OnPremiseInProcTargetConnector<T>(requestTimeout, _logger);
		}
	}
}
