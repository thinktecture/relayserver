using System;
using System.Collections.Generic;
using System.Net.Http;
using NLog;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Plugins
{
	internal class PluginManager : IPluginManager
	{
		private readonly ILogger _logger;

		private readonly IRequestMethodManipulator _requestMethodManipulator;
		private readonly IRequestUrlManipulator _requestUrlManipulator;
		private readonly IRequestHeaderManipulator _requestHeaderManipulator;
		private readonly IRequestBodyManipulator _requestBodyManipulator;

		private readonly IResponseStatusCodeManipulator _responseStatusCodeManipulator;
		private readonly IResponseHeaderManipulator _responseHeaderManipulator;
		private readonly IResponseBodyManipulator _responseBodyManipulator;

		public PluginManager(
			ILogger logger,
			IRequestMethodManipulator requestMethodManipulator = null,
			IRequestUrlManipulator requestUrlManipulator = null,
			IRequestHeaderManipulator requestHeaderManipulator = null,
			IRequestBodyManipulator requestBodyManipulator = null,
			IResponseStatusCodeManipulator responseStatusCodeManipulator = null,
			IResponseHeaderManipulator responseHeaderManipulator = null,
			IResponseBodyManipulator responseBodyManipulator = null
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			_requestMethodManipulator = requestMethodManipulator;
			_requestUrlManipulator = requestUrlManipulator;
			_requestHeaderManipulator = requestHeaderManipulator;
			_requestBodyManipulator = requestBodyManipulator;

			_responseStatusCodeManipulator = responseStatusCodeManipulator;
			_responseHeaderManipulator = responseHeaderManipulator;
			_responseBodyManipulator = responseBodyManipulator;
		}

		public OnPremiseConnectorRequest HandleRequest(OnPremiseConnectorRequest onPremiseConnectorRequest, out HttpResponseMessage response)
		{
			response = null;
			_logger.Trace($"{nameof(PluginManager)}: handling request {{0}}", onPremiseConnectorRequest.RequestId);

			HandleRequestMethod(onPremiseConnectorRequest, out response);
			if (response != null)
				return onPremiseConnectorRequest;

			HandleRequestUrl(onPremiseConnectorRequest, out response);
			if (response != null)
				return onPremiseConnectorRequest;

			HandleRequestHeaders(onPremiseConnectorRequest, out response);
			if (response != null)
				return onPremiseConnectorRequest;

			HandleRequestBody(onPremiseConnectorRequest, out response);
			if (response != null)
				return onPremiseConnectorRequest;

			return onPremiseConnectorRequest;
		}

		public OnPremiseTargetResponse HandleResponse(OnPremiseTargetResponse onPremiseTargetResponse, IOnPremiseConnectorRequest onPremiseConnectorRequest, out HttpResponseMessage response)
		{
			response = null;
			_logger.Trace($"{nameof(PluginManager)}: handling response for request {{0}}", onPremiseConnectorRequest.RequestId);

			HandleResponseStatusCode(onPremiseTargetResponse, onPremiseConnectorRequest, out response);
			if (response != null)
				return onPremiseTargetResponse;

			HandleResponseHeader(onPremiseTargetResponse, onPremiseConnectorRequest, out response);
			if (response != null)
				return onPremiseTargetResponse;

			HandleResponseBody(onPremiseTargetResponse, onPremiseConnectorRequest, out response);
			if (response != null)
				return onPremiseTargetResponse;

			return onPremiseTargetResponse;
		}

		private void HandleRequestMethod(OnPremiseConnectorRequest onPremiseConnectorRequest, out HttpResponseMessage response)
		{
			response = null;

			try
			{
				var method = _requestMethodManipulator?.HandleMethod(onPremiseConnectorRequest, out response);

				if (!string.IsNullOrWhiteSpace(method))
				{
					_logger.Trace($"{nameof(PluginManager)}: The {nameof(IRequestMethodManipulator)} modified the request method for request {{0}}", onPremiseConnectorRequest.RequestId);
					onPremiseConnectorRequest.HttpMethod = method;
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"{nameof(PluginManager)}: Error while executing the {nameof(IRequestMethodManipulator)} plugin for request {{0}}", onPremiseConnectorRequest.RequestId);
			}
		}

		private void HandleRequestUrl(OnPremiseConnectorRequest onPremiseConnectorRequest, out HttpResponseMessage response)
		{
			response = null;

			try
			{
				var url = _requestUrlManipulator?.HandleUrl(onPremiseConnectorRequest, out response);

				if (!string.IsNullOrWhiteSpace(url))
				{
					_logger.Trace($"{nameof(PluginManager)}: The {nameof(IRequestUrlManipulator)} modified the request url for request {{0}}", onPremiseConnectorRequest.RequestId);
					onPremiseConnectorRequest.Url = url;
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"{nameof(PluginManager)}: Error while executing the {nameof(IRequestUrlManipulator)} plugin for request {{0}}", onPremiseConnectorRequest.RequestId);
			}
		}

		private void HandleRequestHeaders(OnPremiseConnectorRequest onPremiseConnectorRequest, out HttpResponseMessage response)
		{
			response = null;

			try
			{
				var headers = _requestHeaderManipulator?.HandleHeaders(onPremiseConnectorRequest, out response);

				if (headers != null)
				{
					_logger.Trace($"{nameof(PluginManager)}: The  {nameof(IRequestHeaderManipulator)} modified the request headers for request {{0}}", onPremiseConnectorRequest.RequestId);
					onPremiseConnectorRequest.HttpHeaders = new Dictionary<string, string>(headers);
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"{nameof(PluginManager)}: Error while executing the {nameof(IRequestHeaderManipulator)} plugin for request {{0}}", onPremiseConnectorRequest.RequestId);
			}
		}

		private void HandleRequestBody(OnPremiseConnectorRequest onPremiseConnectorRequest, out HttpResponseMessage response)
		{
			response = null;

			try
			{
				var body = _requestBodyManipulator?.HandleBody(onPremiseConnectorRequest, out response);

				if (body != null)
				{
					_logger.Trace($"{nameof(PluginManager)}: The {nameof(IRequestBodyManipulator)} modified the request body for request {{0}}", onPremiseConnectorRequest.RequestId);
					onPremiseConnectorRequest.Body = body;
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"{nameof(PluginManager)}: Error while executing the {nameof(IRequestBodyManipulator)} plugin for request {{0}}", onPremiseConnectorRequest.RequestId);
			}
		}

		private void HandleResponseStatusCode(OnPremiseTargetResponse onPremiseTargetResponse, IOnPremiseConnectorRequest onPremiseConnectorRequest, out HttpResponseMessage response)
		{
			response = null;

			try
			{
				var statusCode = _responseStatusCodeManipulator?.HandleStatusCode(onPremiseTargetResponse, onPremiseConnectorRequest, out response);

				if (statusCode != null)
				{
					if (onPremiseTargetResponse != null)
					{
						_logger.Trace($"{nameof(PluginManager)}: The {nameof(IResponseStatusCodeManipulator)} plugin modified the response status code for request {{0}}", onPremiseConnectorRequest.RequestId);
						onPremiseTargetResponse.StatusCode = statusCode.Value;
					}
					else
					{
						_logger.Warn($"{ nameof(PluginManager)}: No response available, but the {nameof(IResponseStatusCodeManipulator)} plugin tried to modify the response status code for request {{0}} allthough no reponse exists to modify", onPremiseConnectorRequest.RequestId);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"{nameof(PluginManager)}: Error while executing the {nameof(IResponseStatusCodeManipulator)} plugin for request {{0}}", onPremiseConnectorRequest.RequestId);
			}
		}

		private void HandleResponseHeader(OnPremiseTargetResponse onPremiseTargetResponse, IOnPremiseConnectorRequest onPremiseConnectorRequest, out HttpResponseMessage response)
		{
			response = null;

			try
			{
				var headers = _responseHeaderManipulator?.HandleHeaders(onPremiseTargetResponse, onPremiseConnectorRequest, out response);

				if (headers != null)
				{
					if (onPremiseTargetResponse != null)
					{
						_logger.Trace($"{nameof(PluginManager)}: The {nameof(IResponseHeaderManipulator)} plugin modified the response headers for request {{0}}", onPremiseConnectorRequest.RequestId);
						onPremiseTargetResponse.HttpHeaders = new Dictionary<string, string>(headers);
					}
					else
					{
						_logger.Warn($"{ nameof(PluginManager)}: No response available, but the {nameof(IResponseHeaderManipulator)} plugin tried to modify the response headers for request {{0}} allthough no reponse exists to modify", onPremiseConnectorRequest.RequestId);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"{nameof(PluginManager)}: Error while executing the {nameof(IResponseHeaderManipulator)} plugin for request {{0}}", onPremiseConnectorRequest.RequestId);
			}
		}

		private void HandleResponseBody(OnPremiseTargetResponse onPremiseTargetResponse, IOnPremiseConnectorRequest onPremiseConnectorRequest, out HttpResponseMessage response)
		{
			response = null;

			try
			{
				var body = _responseBodyManipulator?.HandleBody(onPremiseTargetResponse, onPremiseConnectorRequest, out response);

				if (body != null)
				{
					if (onPremiseTargetResponse != null)
					{
						_logger.Trace($"{nameof(PluginManager)}: The {nameof(IResponseBodyManipulator)} plugin modified the response body for request {{0}}", onPremiseConnectorRequest.RequestId);
						onPremiseTargetResponse.Body = body;
					}
					else
					{
						_logger.Warn($"{ nameof(PluginManager)}: No response available, but the {nameof(IResponseBodyManipulator)} plugin tried to modify the response body for request {{0}} allthough no reponse exists to modify", onPremiseConnectorRequest.RequestId);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"{nameof(PluginManager)}: Error while executing the {nameof(IResponseBodyManipulator)} plugin for request {{0}}", onPremiseConnectorRequest.RequestId);
			}
		}
	}
}
