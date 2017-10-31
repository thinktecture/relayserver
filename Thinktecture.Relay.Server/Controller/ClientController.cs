using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Serilog;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.Http;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Interceptor;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Controller
{
	[AllowAnonymous]
	[RelayModuleBindingFilter]
	public class ClientController : ApiController
	{
		private readonly IBackendCommunication _backendCommunication;
		private readonly ILogger _logger;
		private readonly ILinkRepository _linkRepository;
		private readonly IRequestLogger _requestLogger;
		private readonly IHttpResponseMessageBuilder _httpResponseMessageBuilder;
		private readonly IOnPremiseRequestBuilder _onPremiseRequestBuilder;
		private readonly IPathSplitter _pathSplitter;
		private readonly ITraceManager _traceManager;
		private readonly IInterceptorManager _interceptorManager;

		public ClientController(IBackendCommunication backendCommunication, ILogger logger, ILinkRepository linkRepository, IRequestLogger requestLogger,
			IHttpResponseMessageBuilder httpResponseMessageBuilder, IOnPremiseRequestBuilder onPremiseRequestBuilder, IPathSplitter pathSplitter,
			ITraceManager traceManager, IInterceptorManager interceptorManager)
		{
			_backendCommunication = backendCommunication ?? throw new ArgumentNullException(nameof(backendCommunication));
			_logger = logger;
			_linkRepository = linkRepository ?? throw new ArgumentNullException(nameof(linkRepository));
			_requestLogger = requestLogger ?? throw new ArgumentNullException(nameof(requestLogger));
			_httpResponseMessageBuilder = httpResponseMessageBuilder ?? throw new ArgumentNullException(nameof(httpResponseMessageBuilder));
			_onPremiseRequestBuilder = onPremiseRequestBuilder ?? throw new ArgumentNullException(nameof(onPremiseRequestBuilder));
			_pathSplitter = pathSplitter ?? throw new ArgumentNullException(nameof(pathSplitter));
			_traceManager = traceManager ?? throw new ArgumentNullException(nameof(traceManager));
			_interceptorManager = interceptorManager ?? throw new ArgumentNullException(nameof(interceptorManager));
		}

		[HttpOptions]
		[HttpPost]
		[HttpGet]
		[HttpPut]
		[HttpPatch]
		[HttpDelete]
		[HttpHead]
		public async Task<HttpResponseMessage> Relay(string path)
		{
			_logger?.Debug("Relaying {request-path} {request-method}", path, ControllerContext.Request.Method);

			if (path == null)
			{
				_logger?.Information("Path is not set");
				return NotFound();
			}

			var pathInformation = _pathSplitter.Split(path);
			var link = _linkRepository.GetLink(pathInformation.UserName);

			if (!CanRequestBeHandled(path, pathInformation, link))
			{
				return NotFound();
			}

			var request = await _onPremiseRequestBuilder.BuildFromHttpRequest(Request, _backendCommunication.OriginId, pathInformation.PathWithoutUserName).ConfigureAwait(false);

			var statusCode = HttpStatusCode.GatewayTimeout;
			IOnPremiseConnectorResponse response = null;
			try
			{
				request = _interceptorManager.HandleRequest(request, Request, out var message);
				if (message != null)
				{
					_logger?.Verbose("Interceptor caused direct answering of request. request-id={request-id}, status-code={response-status-code}", request.RequestId, message.StatusCode);

					statusCode = message.StatusCode;
					return message;
				}

				var task = _backendCommunication.GetResponseAsync(request.RequestId);

				_logger?.Verbose("Sending on premise connector request. request-id={request-id}, link-id={link-id}", request.RequestId, link.Id);
				await _backendCommunication.SendOnPremiseConnectorRequest(link.Id, request).ConfigureAwait(false);

				_logger?.Verbose("Waiting for response. request-id={request-id}, link-id={link-id}", request.RequestId, link.Id);
				response = await task.ConfigureAwait(false);

				if (response != null)
				{
					_logger?.Verbose("Response received. request-id={request-id}, link-id={link-id}", request.RequestId, link.Id);
					statusCode = response.StatusCode;
				}
				else
				{
					_logger?.Verbose("On-Premise timeout. request-id={request-id}, link-id={link-id}", request.RequestId, link.Id);
				}

				return _interceptorManager.HandleResponse(request, response) ?? _httpResponseMessageBuilder.BuildFromConnectorResponse(response, link, request.RequestId);
			}
			finally
			{
				FinishRequest(request, response, link.Id, path, statusCode);
			}
		}

		private bool CanRequestBeHandled(string path, PathInformation pathInformation, Link link)
		{
			if (link == null)
			{
				_logger?.Information("Link for path {request-path} not found", path);
				return false;
			}

			if (link.IsDisabled)
			{
				_logger?.Information("Link {link-name} is disabled", link.SymbolicName);
				return false;
			}

			if (String.IsNullOrWhiteSpace(pathInformation.PathWithoutUserName))
			{
				_logger?.Information("Path {request-path} for link {link-name} without user name is not found", path, link.SymbolicName);
				return false;
			}

			if (link.AllowLocalClientRequestsOnly && !Request.IsLocal())
			{
				_logger?.Information("Link {link-name} only allows local requests", link.SymbolicName);
				return false;
			}

			return true;
		}

		private void FinishRequest(IOnPremiseConnectorRequest request, IOnPremiseConnectorResponse response, Guid linkId, string path, HttpStatusCode statusCode)
		{
			request.RequestFinished = DateTime.UtcNow;

			_logger?.Verbose("Finishing request. request-id={request-id}, link-id={link-id}, on-premise-duration={remote-duration}, global-duration={global-duration}", request.RequestId, linkId, response?.RequestFinished - response?.RequestStarted, request.RequestFinished - request.RequestStarted);

			// TODO this may be debounced for e.g. 5 minutes to skip querying on each request in future release
			var currentTraceConfigurationId = _traceManager.GetCurrentTraceConfigurationId(linkId);
			if (currentTraceConfigurationId != null)
			{
				_traceManager.Trace(request, response, currentTraceConfigurationId.Value);
			}

			_requestLogger.LogRequest(request, response, linkId, _backendCommunication.OriginId, path, statusCode);
		}

		private new HttpResponseMessage NotFound()
		{
			return new HttpResponseMessage(HttpStatusCode.NotFound);
		}
	}
}
