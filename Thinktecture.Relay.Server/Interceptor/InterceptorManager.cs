using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using Serilog;
using Thinktecture.Relay.Server.Http;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Interceptor
{
	internal class InterceptorManager : IInterceptorManager
	{
		private readonly ILogger _logger;
		private readonly IHttpResponseMessageBuilder _httpResponseMessageBuilder;
		private readonly IOnPremiseRequestInterceptor _requestInterceptor;
		private readonly IOnPremiseResponseInterceptor _responseInterceptor;

		public InterceptorManager(ILogger logger, IHttpResponseMessageBuilder httpResponseMessageBuilder,
			IOnPremiseRequestInterceptor requestInterceptor = null, IOnPremiseResponseInterceptor responseInterceptor = null)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpResponseMessageBuilder = httpResponseMessageBuilder ?? throw new ArgumentNullException(nameof(httpResponseMessageBuilder));
			_requestInterceptor = requestInterceptor;
			_responseInterceptor = responseInterceptor;
		}

		public IOnPremiseConnectorRequest HandleRequest(IOnPremiseConnectorRequest request, HttpRequestMessage message, IPrincipal clientUser, out HttpResponseMessage immediateResponse)
		{
			immediateResponse = null;

			if (_requestInterceptor == null)
				return request;

			_logger.Verbose("Handling request. request-id={RequestId}", request.RequestId);

			try
			{
				var interceptedRequest = CreateInterceptedRequest(request, message, clientUser);

				immediateResponse = _requestInterceptor.OnRequestReceived(interceptedRequest);

				if (immediateResponse != null)
				{
					immediateResponse.RequestMessage = message;
				}

				return interceptedRequest;
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error while executing the request interceptor. type-name={InterceptorType}, request-id={RequestId}", _requestInterceptor?.GetType().Name, request.RequestId);
			}

			return request;
		}

		private InterceptedRequest CreateInterceptedRequest(IOnPremiseConnectorRequest request, HttpRequestMessage message, IPrincipal clientUser)
		{
			return new InterceptedRequest(_logger.ForContext<InterceptedRequest>(), request)
			{
				ClientIpAddress = GetRemoteIpAddress(request, message),
				ClientUser = clientUser,
				ClientRequestUri = message.RequestUri,
			};
		}

		private IPAddress GetRemoteIpAddress(IOnPremiseConnectorRequest request, HttpRequestMessage message)
		{
			try
			{
				return message.GetRemoteIpAddress();
			}
			catch (Exception ex)
			{
				_logger.Warning(ex, "Could not fetch remote IP address. request-id={RequestId}", request.RequestId);
			}

			return null;
		}

		public HttpResponseMessage HandleResponse(IOnPremiseConnectorRequest request, HttpRequestMessage message, IPrincipal clientUser, IOnPremiseConnectorResponse response, bool forwardOnPremiseTargetErrorResponse)
		{
			if (_responseInterceptor == null)
				return _httpResponseMessageBuilder.BuildFromConnectorResponse(response, forwardOnPremiseTargetErrorResponse, request.RequestId);

			_logger.Verbose("Handling response. request-id={RequestId}", request.RequestId);

			try
			{
				var interceptedRequest = CreateInterceptedRequest(request, message, clientUser);

				HttpResponseMessage immediateResponse = null;
				if (response == null)
				{
					immediateResponse = _responseInterceptor.OnResponseFailed(interceptedRequest);
				}
				else
				{
					var interceptedResponse = new InterceptedResponse(_logger.ForContext<IInterceptedResponse>(), response);
					immediateResponse = _responseInterceptor.OnResponseReceived(interceptedRequest, interceptedResponse);

					if (immediateResponse == null)
					{
						return _httpResponseMessageBuilder.BuildFromConnectorResponse(interceptedResponse, forwardOnPremiseTargetErrorResponse, request.RequestId);
					}
				}

				if (immediateResponse != null)
				{
					immediateResponse.RequestMessage = message;
				}

				return immediateResponse ?? _httpResponseMessageBuilder.BuildFromConnectorResponse(null, forwardOnPremiseTargetErrorResponse, request.RequestId);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error while executing the response interceptor. type-name={InterceptorType}, request-id={RequestId}", _requestInterceptor?.GetType().Name, request.RequestId);
				return null;
			}
		}
	}
}
