using System;
using System.Net.Http;
using NLog;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Plugins
{
	internal class PluginManager : IPluginManager
	{
		private readonly ILogger _logger;

		private readonly IOnPremiseRequestInterceptor _requestInceptor;
		private readonly IOnPremiseResponseInterceptor _responseInterceptor;

		public PluginManager(ILogger logger, IOnPremiseRequestInterceptor requestInceptor = null, IOnPremiseResponseInterceptor responseInterceptor = null)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_requestInceptor = requestInceptor;
			_responseInterceptor = responseInterceptor;
		}

		public HttpResponseMessage HandleRequest(OnPremiseConnectorRequest onPremiseConnectorRequest)
		{
			_logger.Trace($"{nameof(PluginManager)}: handling request {{0}}", onPremiseConnectorRequest.RequestId);

			try
			{
				return _requestInceptor?.OnRequestReceived(onPremiseConnectorRequest);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"{nameof(PluginManager)}: Error while executing the plugin \"{_requestInceptor?.GetType().Name}.{nameof(IOnPremiseRequestInterceptor.OnRequestReceived)}()\" for request {{0}}", onPremiseConnectorRequest.RequestId);
				return null;
			}
		}

		public HttpResponseMessage HandleResponse(IOnPremiseConnectorRequest onPremiseConnectorRequest, OnPremiseTargetResponse onPremiseTargetResponse)
		{
			_logger.Trace($"{nameof(PluginManager)}: handling response for request {{0}}", onPremiseConnectorRequest.RequestId);

			try
			{
				var interceptor = _responseInterceptor;

				if (interceptor != null)
				{
					if (onPremiseTargetResponse == null)
						return interceptor.OnResponseReceived(onPremiseConnectorRequest);

					return interceptor.OnResponseReceived(onPremiseConnectorRequest, onPremiseTargetResponse);
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"{nameof(PluginManager)}: Error while executing the plugin \"{_responseInterceptor?.GetType().Name}.{nameof(IOnPremiseResponseInterceptor.OnResponseReceived)}()\" for request {{0}}", onPremiseConnectorRequest.RequestId);
			}

			return null;
		}
	}
}
