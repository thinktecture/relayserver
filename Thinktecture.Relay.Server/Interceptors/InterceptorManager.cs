using System;
using System.Net.Http;
using NLog;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Interceptors
{
	internal class InterceptorManager : IInterceptorManager
	{
		private readonly ILogger _logger;

		private readonly IOnPremiseRequestInterceptor _requestInceptor;
		private readonly IOnPremiseResponseInterceptor _responseInterceptor;

		public InterceptorManager(ILogger logger, IOnPremiseRequestInterceptor requestInceptor = null, IOnPremiseResponseInterceptor responseInterceptor = null)
		{
			_logger = logger;
			_requestInceptor = requestInceptor;
			_responseInterceptor = responseInterceptor;
		}

		public HttpResponseMessage HandleRequest(IOnPremiseConnectorRequest request)
		{
			_logger?.Trace($"{nameof(InterceptorManager)}: handling request {{0}}", request.RequestId);

			try
			{
				return _requestInceptor?.OnRequestReceived(new InterceptedRequest(request));
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, $"{nameof(InterceptorManager)}: Error while executing the interceptor \"{_requestInceptor?.GetType().Name}.{nameof(IOnPremiseRequestInterceptor.OnRequestReceived)}()\" for request {{0}}", request.RequestId);
				return null;
			}
		}

		public HttpResponseMessage HandleResponse(IOnPremiseConnectorRequest request, IOnPremiseConnectorResponse response)
		{
			_logger?.Trace($"{nameof(InterceptorManager)}: handling response for request {{0}}", request.RequestId);

			try
			{
				var interceptor = _responseInterceptor;

				if (interceptor != null)
				{
					if (response == null)
						return interceptor.OnResponseReceived(new InterceptedRequest(request));

					return interceptor.OnResponseReceived(new InterceptedRequest(request), new InterceptedResponse(response));
				}
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, $"{nameof(InterceptorManager)}: Error while executing the interceptor \"{_responseInterceptor?.GetType().Name}.{nameof(IOnPremiseResponseInterceptor.OnResponseReceived)}()\" for request {{0}}", request.RequestId);
			}

			return null;
		}
	}
}
