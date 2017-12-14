using System;
using Serilog;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseTargetConnectorFactory : IOnPremiseTargetConnectorFactory
	{
		private readonly ILogger _logger;
		private readonly Func<IOnPremiseWebTargetRequestMessageBuilder> _requestMessageBuilderFactory;

		public OnPremiseTargetConnectorFactory(ILogger logger, Func<IOnPremiseWebTargetRequestMessageBuilder> requestMessageBuilderFactory)
		{
			_logger = logger;
			_requestMessageBuilderFactory = requestMessageBuilderFactory ?? throw new ArgumentNullException(nameof(requestMessageBuilderFactory));
		}

		public IOnPremiseTargetConnector Create(Uri baseUri, int requestTimeout)
		{
			return new OnPremiseWebTargetConnector(baseUri, requestTimeout, _logger, _requestMessageBuilderFactory());
		}

		public IOnPremiseTargetConnector Create(Type handlerType, int requestTimeout)
		{
			return new OnPremiseInProcTargetConnector(_logger, requestTimeout, handlerType);
		}

		public IOnPremiseTargetConnector Create(Func<IOnPremiseInProcHandler> handlerFactory, int requestTimeout)
		{
			return new OnPremiseInProcTargetConnector(_logger, requestTimeout, handlerFactory);
		}

		public IOnPremiseTargetConnector Create<T>(int requestTimeout)
			where T : IOnPremiseInProcHandler, new()
		{
			return new OnPremiseInProcTargetConnector<T>(requestTimeout, _logger);
		}
	}
}
