using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using NLog;
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

		[HttpDelete]
		[HttpGet]
		[HttpHead]
		[HttpPost]
		[HttpPut]
		[HttpOptions]
		public async Task<HttpResponseMessage> Relay(string path)
		{
			_logger?.Debug("Relaying {0} {1}", path, ControllerContext.Request.Method);

			if (path == null)
			{
				_logger?.Info("Path is not set.");
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
				var message = _interceptorManager.HandleRequest(request, Request);
				if (message != null)
				{
					_logger?.Trace("Interceptor caused direct answering of request. request-id={0}, status-code={1}", request.RequestId, message.StatusCode);

					statusCode = message.StatusCode;
					return message;
				}

				var task = _backendCommunication.GetResponseAsync(request.RequestId);

				_logger?.Trace("Sending on premise connector request. request-id={0}, link-id={1}", request.RequestId, link.Id);
				await _backendCommunication.SendOnPremiseConnectorRequest(link.Id, request).ConfigureAwait(false);

				_logger?.Trace("Waiting for response. request-id={0}, link-id={1}", request.RequestId, link.Id);
				response = await task.ConfigureAwait(false);

				if (response != null)
				{
					_logger?.Trace("Response received. request-id={0}, link-id={1}", request.RequestId, link.Id);
					statusCode = response.StatusCode;
				}
				else
				{
					_logger?.Trace("On-Premise timeout. request-id={0}, link-id={1}", request.RequestId, link.Id);
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
				_logger?.Info("Link not found. Path: {0}", path);
				return false;
			}

			if (link.IsDisabled)
			{
				_logger?.Info("Link {0} is disabled", link.SymbolicName);
				return false;
			}

			if (String.IsNullOrWhiteSpace(pathInformation.PathWithoutUserName))
			{
				_logger?.Info("Path for link {0} without user name is not found. Path: {1}", link.SymbolicName, path);
				return false;
			}

			if (link.AllowLocalClientRequestsOnly && !Request.IsLocal())
			{
				_logger?.Info("Link {0} only allows local requests.", link.SymbolicName);
				return false;
			}

			return true;
		}

		private void FinishRequest(IOnPremiseConnectorRequest request, IOnPremiseConnectorResponse response, Guid linkId, string path, HttpStatusCode statusCode)
		{
			_logger?.Trace("Finishing request. request-id={0}, link-id={1}", request.RequestId, linkId);

			request.RequestFinished = DateTime.UtcNow;

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
