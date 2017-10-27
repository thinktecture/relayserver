using System;
using System.Net;
using System.Net.Http;
using NLog;
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

		public IOnPremiseConnectorRequest HandleRequest(IOnPremiseConnectorRequest request, HttpRequestMessage message, out HttpResponseMessage immidateResponse)
		{
			immidateResponse = null;
			if (_requestInceptor == null)
			{
				return request;
			}

			_logger?.Trace("Handling request. request-id={0}", request.RequestId);

			IPAddress ipAddress = null;
			try
			{
				ipAddress = message.GetRemoteIpAddress();
			}
			catch (Exception ex)
			{
				_logger?.Warn(ex, "Could not fetch remote IP address for request {0}", request.RequestId);
			}

			try
			{
				var replacedRequest = new InterceptedRequest(request) { ClientIpAddress = ipAddress };
				immidateResponse = _requestInceptor.OnRequestReceived(replacedRequest);

				return replacedRequest;
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error while executing the request interceptor. TypeName = {0}, RequestId = {1}", _requestInceptor?.GetType().Name, request.RequestId);
			}

			return request;
		}

		public HttpResponseMessage HandleResponse(IOnPremiseConnectorRequest request, IOnPremiseConnectorResponse response)
		{
			if (_responseInterceptor == null)
			{
				return null;
			}

			_logger?.Trace("Handling response. request-id={0}", request.RequestId);

			try
			{
				if (response == null)
				{
					return _responseInterceptor.OnResponseReceived(new InterceptedRequest(request));
				}

				return _responseInterceptor.OnResponseReceived(new InterceptedRequest(request), new InterceptedResponse(response));
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error while executing the response interceptor. TypeName = {0}, RequestId = {1}", _requestInceptor?.GetType().Name, request.RequestId);
				return null;
			}
		}
	}
}
