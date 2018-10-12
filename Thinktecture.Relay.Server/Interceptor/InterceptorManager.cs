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
		private readonly IOnPremiseRequestInterceptor _requestInceptor;
		private readonly IOnPremiseResponseInterceptor _responseInterceptor;

		public InterceptorManager(ILogger logger, IOnPremiseRequestInterceptor requestInceptor = null, IOnPremiseResponseInterceptor responseInterceptor = null)
		{
			_logger = logger;
			_requestInceptor = requestInceptor;
			_responseInterceptor = responseInterceptor;
		}

		public IOnPremiseConnectorRequest HandleRequest(IOnPremiseConnectorRequest request, HttpRequestMessage message, IPrincipal clientUser, out HttpResponseMessage immediateResponse)
		{
			immediateResponse = null;

			if (_requestInceptor == null)
				return request;

			_logger?.Verbose("Handling request. request-id={RequestId}", request.RequestId);


			try
			{
				var interceptedRequest = CreateInterceptedRequest(request, message, clientUser);

				immediateResponse = _requestInceptor.OnRequestReceived(interceptedRequest);

				if (immediateResponse != null)
				{
					immediateResponse.RequestMessage = message;
				}

				return interceptedRequest;
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error while executing the request interceptor. type-name={InterceptorType}, request-id={RequestId}", _requestInceptor?.GetType().Name, request.RequestId);
			}

			return request;
		}

		private InterceptedRequest CreateInterceptedRequest(IOnPremiseConnectorRequest request, HttpRequestMessage message, IPrincipal clientUser)
		{
			return new InterceptedRequest(request)
			{
				ClientIpAddress = GetRemoteIpAddress(request, message),
				ClientUser = clientUser,
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
				_logger?.Warning(ex, "Could not fetch remote IP address. request-id={RequestId}", request.RequestId);
			}

			return null;
		}

		public HttpResponseMessage HandleResponse(IOnPremiseConnectorRequest request, HttpRequestMessage message, IPrincipal clientUser, IOnPremiseConnectorResponse response)
		{
			if (_responseInterceptor == null)
				return null;

			_logger?.Verbose("Handling response. request-id={RequestId}", request.RequestId);

			try
			{
				var interceptedRequest = CreateInterceptedRequest(request, message, clientUser);

				var immediateResponse = response == null ? _responseInterceptor.OnResponseFailed(interceptedRequest) : _responseInterceptor.OnResponseReceived(interceptedRequest, new InterceptedResponse(response));

				if (immediateResponse != null)
				{
					immediateResponse.RequestMessage = message;
				}

				return immediateResponse;
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error while executing the response interceptor. type-name={InterceptorType}, request-id={RequestId}", _requestInceptor?.GetType().Name, request.RequestId);
				return null;
			}
		}
	}
}
