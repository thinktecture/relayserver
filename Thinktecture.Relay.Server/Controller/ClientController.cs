using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using NLog;
using Thinktecture.Relay.OnPremiseConnector;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.Http;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Interceptors;
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
			_logger?.Trace("Relaying http {0} {1}", path, ControllerContext.Request.Method);

			if (path == null)
			{
				_logger?.Info("Path is not set.");
				return NotFound();
			}

			var pathInformation = _pathSplitter.Split(path);
			var link = _linkRepository.GetLink(pathInformation.UserName);

			if (!CanRequestBeHandled(path, pathInformation, link))
				return NotFound();

			_logger?.Trace("{0}: Building on premise connector request. Origin Id: {1}, Path: {2}", link.Id, _backendCommunication.OriginId, path);
			var onPremiseConnectorRequest = await _onPremiseRequestBuilder.BuildFrom(Request, _backendCommunication.OriginId, pathInformation.PathWithoutUserName).ConfigureAwait(false);

			var response = _interceptorManager.HandleRequest(onPremiseConnectorRequest);
			if (response != null)
			{
				_logger?.Debug("Interceptor caused direct answering of request.");
				FinishRequest(onPremiseConnectorRequest, null, response, link.Id, path);
				return response;
			}

			var onPremiseTargetResponseTask = _backendCommunication.GetResponseAsync(onPremiseConnectorRequest.RequestId);

			_logger?.Trace("{0}: Sending on premise connector request.", link.Id);
			await _backendCommunication.SendOnPremiseConnectorRequest(link.Id, onPremiseConnectorRequest).ConfigureAwait(false);

			_logger?.Trace("{0}: Waiting for response. Request Id", onPremiseConnectorRequest.RequestId);
			var onPremiseTargetResponse = await onPremiseTargetResponseTask.ConfigureAwait(false);

			if (onPremiseTargetResponse != null)
			{
				_logger?.Trace("{0}: Response received. From: {1}", link.Id, onPremiseTargetResponse.RequestId);
			}
			else
			{
				_logger?.Trace("{0}: On-Premise timeout.", link.Id);
			}

			response = _interceptorManager.HandleResponse(onPremiseConnectorRequest, onPremiseTargetResponse)
				?? _httpResponseMessageBuilder.BuildFrom(onPremiseTargetResponse, link);

			FinishRequest(onPremiseConnectorRequest, onPremiseTargetResponse, response, link.Id, path);
			return response;
		}

		private bool CanRequestBeHandled(string path, PathInformation pathInformation, Link link)
		{
			if (path == null)
			{
				_logger?.Info("Path is not set.");
				return false;
			}

			if (link == null)
			{
				_logger?.Info("Link not found. Path: {0}", path);
				return false;
			}

			if (link.IsDisabled)
			{
				_logger?.Info("{0}: Link {1} is disabled.", link.Id, link.SymbolicName);
				return false;
			}

			if (String.IsNullOrWhiteSpace(pathInformation.PathWithoutUserName))
			{
				_logger?.Info("{0}: Path without user name is not found. Path: {1}", link.Id, path);
				return false;
			}

			if (link.AllowLocalClientRequestsOnly && !Request.IsLocal())
			{
				_logger?.Info("{0}: Link {1} only allows local requests.", link.Id, link.SymbolicName);
				return false;
			}

			return true;
		}

		private void FinishRequest(IOnPremiseConnectorRequest onPremiseConnectorRequest, IOnPremiseTargetResponse onPremiseTargetResponse, HttpResponseMessage response, Guid linkId, string path)
		{
			onPremiseConnectorRequest.RequestFinished = DateTime.UtcNow;

			// TODO this may be debounced for e.g. 5 minutes to skip querying on each request
			var currentTraceConfigurationId = _traceManager.GetCurrentTraceConfigurationId(linkId);
			if (currentTraceConfigurationId != null)
			{
				_traceManager.Trace(onPremiseConnectorRequest, onPremiseTargetResponse, currentTraceConfigurationId.Value);
			}

			_requestLogger.LogRequest(onPremiseConnectorRequest, onPremiseTargetResponse, response.StatusCode, linkId, _backendCommunication.OriginId, path);
		}

		private new HttpResponseMessage NotFound()
		{
			return new HttpResponseMessage(HttpStatusCode.NotFound);
		}
	}
}
