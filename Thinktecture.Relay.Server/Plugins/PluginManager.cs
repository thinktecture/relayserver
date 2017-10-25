using System;
using System.Net.Http;
using NLog;
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
			_logger = logger;
			_requestInceptor = requestInceptor;
			_responseInterceptor = responseInterceptor;
		}

		public HttpResponseMessage HandleRequest(IOnPremiseConnectorRequest request)
		{
			_logger?.Trace($"{nameof(PluginManager)}: handling request {{0}}", request.RequestId);

			try
			{
				return _requestInceptor?.OnRequestReceived(request);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, $"{nameof(PluginManager)}: Error while executing the plugin \"{_requestInceptor?.GetType().Name}.{nameof(IOnPremiseRequestInterceptor.OnRequestReceived)}()\" for request {{0}}", request.RequestId);
				return null;
			}
		}

		public HttpResponseMessage HandleResponse(IOnPremiseConnectorRequest request, IOnPremiseConnectorResponse response)
		{
			_logger?.Trace($"{nameof(PluginManager)}: handling response for request {{0}}", request.RequestId);

			try
			{
				var interceptor = _responseInterceptor;

				if (interceptor != null)
				{
					if (response == null)
						return interceptor.OnResponseReceived(request);

					return interceptor.OnResponseReceived(request, response);
				}
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, $"{nameof(PluginManager)}: Error while executing the plugin \"{_responseInterceptor?.GetType().Name}.{nameof(IOnPremiseResponseInterceptor.OnResponseReceived)}()\" for request {{0}}", request.RequestId);
			}

			return null;
		}
	}
}
