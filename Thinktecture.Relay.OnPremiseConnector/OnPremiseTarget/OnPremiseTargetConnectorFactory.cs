using System;
using System.Net.Http;
using Serilog;

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

		public IOnPremiseTargetConnector Create(Uri baseUri, TimeSpan requestTimeout, bool followRedirects = true, bool logSensitiveData = true)
		{
			return new OnPremiseWebTargetConnector(baseUri, requestTimeout, _logger, _requestMessageBuilderFactory(), _httpClientFactory, followRedirects, logSensitiveData);
		}

		public IOnPremiseTargetConnector Create(Type handlerType, TimeSpan requestTimeout, bool logSensitiveData = true)
		{
			return new OnPremiseInProcTargetConnector(_logger, requestTimeout, handlerType, logSensitiveData);
		}

		public IOnPremiseTargetConnector Create(Func<IOnPremiseInProcHandler> handlerFactory, TimeSpan requestTimeout, bool logSensitiveData = true)
		{
			return new OnPremiseInProcTargetConnector(_logger, requestTimeout, handlerFactory, logSensitiveData);
		}

		public IOnPremiseTargetConnector Create<T>(TimeSpan requestTimeout, bool logSensitiveData = true)
			where T : IOnPremiseInProcHandler, new()
		{
			return new OnPremiseInProcTargetConnector<T>(requestTimeout, _logger, logSensitiveData);
		}
	}
}
