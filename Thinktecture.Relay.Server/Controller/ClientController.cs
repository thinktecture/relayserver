using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Serilog;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Filters;
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.Http;
using Thinktecture.Relay.Server.Http.Filters;
using Thinktecture.Relay.Server.Interceptor;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Controller
{
	[RelayModuleBindingFilter]
	[ConfigurableClientAuthorization(Roles = "OnPremise")]
	public class ClientController : ApiController
	{
		private readonly IBackendCommunication _backendCommunication;
		private readonly ILogger _logger;
		private readonly ILinkRepository _linkRepository;
		private readonly IRequestLogger _requestLogger;
		private readonly IOnPremiseRequestBuilder _onPremiseRequestBuilder;
		private readonly IPathSplitter _pathSplitter;
		private readonly ITraceManager _traceManager;
		private readonly IInterceptorManager _interceptorManager;
		private readonly IPostDataTemporaryStore _postDataTemporaryStore;
		private readonly bool _requireLinkAvailability;

		public ClientController(IBackendCommunication backendCommunication, ILogger logger, ILinkRepository linkRepository, IRequestLogger requestLogger,
			IOnPremiseRequestBuilder onPremiseRequestBuilder, IPathSplitter pathSplitter, IConfiguration configuration,
			ITraceManager traceManager, IInterceptorManager interceptorManager, IPostDataTemporaryStore postDataTemporaryStore)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

			_backendCommunication = backendCommunication ?? throw new ArgumentNullException(nameof(backendCommunication));
			_logger = logger;
			_linkRepository = linkRepository ?? throw new ArgumentNullException(nameof(linkRepository));
			_requestLogger = requestLogger ?? throw new ArgumentNullException(nameof(requestLogger));
			_onPremiseRequestBuilder = onPremiseRequestBuilder ?? throw new ArgumentNullException(nameof(onPremiseRequestBuilder));
			_pathSplitter = pathSplitter ?? throw new ArgumentNullException(nameof(pathSplitter));
			_traceManager = traceManager ?? throw new ArgumentNullException(nameof(traceManager));
			_interceptorManager = interceptorManager ?? throw new ArgumentNullException(nameof(interceptorManager));
			_postDataTemporaryStore = postDataTemporaryStore ?? throw new ArgumentNullException(nameof(postDataTemporaryStore));

			_requireLinkAvailability = configuration.RequireLinkAvailability;
		}

		[HttpOptions]
		[HttpPost]
		[HttpGet]
		[HttpPut]
		[HttpPatch]
		[HttpDelete]
		[HttpHead]
		public async Task<HttpResponseMessage> Relay(string fullPathToOnPremiseEndpoint)
		{
			_logger?.Debug("Relaying request. method={RequestMethod}, path={RequestPath}", ControllerContext.Request.Method, fullPathToOnPremiseEndpoint);

			if (fullPathToOnPremiseEndpoint == null)
			{
				_logger?.Information("Path to on premise endpoint is not set");
				return NotFound();
			}

			var pathInformation = _pathSplitter.Split(fullPathToOnPremiseEndpoint);
			var link = _linkRepository.GetLink(pathInformation.UserName);

			if (!CanRequestBeHandled(fullPathToOnPremiseEndpoint, pathInformation, link))
			{
				_logger?.Information("Request cannot be handled");
				return NotFound();
			}

			if (_requireLinkAvailability && !await _linkRepository.HasActiveConnectionAsync(link.Id))
			{
				_logger?.Debug("Request cannot be handled without an active connection");
				return ServiceUnavailable();
			}

			var request = await _onPremiseRequestBuilder.BuildFromHttpRequest(Request, _backendCommunication.OriginId, pathInformation.PathWithoutUserName).ConfigureAwait(false);
			PrepareRequestBodyForRelaying((OnPremiseConnectorRequest)request);

			var statusCode = HttpStatusCode.GatewayTimeout;
			IOnPremiseConnectorResponse response = null;
			try
			{
				request = _interceptorManager.HandleRequest(request, Request, User, out var message);
				if (message != null)
				{
					_logger?.Verbose("Interceptor caused direct answering of request. request-id={RequestId}, status-code={ResponseStatusCode}", request.RequestId, message.StatusCode);

					statusCode = message.StatusCode;

					if (request.AlwaysSendToOnPremiseConnector)
					{
						_logger?.Verbose("Interceptor caused always sending of request. request-id={RequestId}", request.RequestId);
						SendOnPremiseConnectorRequest(link.Id, request);
					}

					return message;
				}

				var task = _backendCommunication.GetResponseAsync(request.RequestId);
				SendOnPremiseConnectorRequest(link.Id, request);

				_logger?.Verbose("Waiting for response. request-id={RequestId}, link-id={LinkId}", request.RequestId, link.Id);
				response = await task.ConfigureAwait(false);

				if (response != null)
				{
					_logger?.Verbose("Response received. request-id={RequestId}, link-id={LinkId}", request.RequestId, link.Id);
					FetchResponseBodyForIntercepting((OnPremiseConnectorResponse)response);
					statusCode = response.StatusCode;
				}
				else
				{
					_logger?.Verbose("No response received because of on-premise timeout. request-id={RequestId}, link-id={LinkId}", request.RequestId, link.Id);
				}

				return _interceptorManager.HandleResponse(request, Request, User, response, link.ForwardOnPremiseTargetErrorResponse);
			}
			finally
			{
				FinishRequest(request as OnPremiseConnectorRequest, response, link.Id, fullPathToOnPremiseEndpoint, statusCode);
			}
		}

		private void SendOnPremiseConnectorRequest(Guid linkId, IOnPremiseConnectorRequest request)
		{
			_logger?.Verbose("Sending on premise connector request. request-id={RequestId}, link-id={LinkId}", request.RequestId, linkId);
			_backendCommunication.SendOnPremiseConnectorRequest(linkId, request);
		}

		private void PrepareRequestBodyForRelaying(OnPremiseConnectorRequest request)
		{
			if (request.Stream == null)
			{
				request.Body = null;
				return;
			}

			if (request.ContentLength == 0 || request.ContentLength >= 0x10000)
			{
				// We might have no Content-Length header, but still content, so we'll assume a large body first
				using (var storeStream = _postDataTemporaryStore.CreateRequestStream(request.RequestId))
				{
					request.Stream.CopyTo(storeStream);
					if (storeStream.Length < 0x10000)
					{
						if (storeStream.Length == 0)
						{
							// no body available (e.g. GET request)
						}
						else
						{
							// the body is small enough to be used directly
							request.Body = new byte[storeStream.Length];
							storeStream.Position = 0;
							storeStream.Read(request.Body, 0, (int)storeStream.Length);
						}
					}
					else
					{
						// a length of 0 indicates that there is a larger body available on the server
						request.Body = Array.Empty<byte>();
					}

					request.Stream = null;
					request.ContentLength = storeStream.Length;
				}
			}
			else
			{
				// we have a body, and it is small enough to be transmitted directly
				request.Body = new byte[request.ContentLength];
				request.Stream.Read(request.Body, 0, (int)request.ContentLength);
				request.Stream = null;
			}
		}

		private void FetchResponseBodyForIntercepting(OnPremiseConnectorResponse response)
		{
			if (response.ContentLength == 0)
			{
				_logger?.Verbose("Received empty body. request-id={RequestId}", response.RequestId);
			}
			else if (response.Body != null)
			{
				_logger?.Verbose("Received small legacy body with data. request-id={RequestId}, body-length={ResponseContentLength}", response.RequestId, response.Body.Length);
			}
			else
			{
				_logger?.Verbose("Received body. request-id={RequestId}, content-length={ResponseContentLength}", response.RequestId, response.ContentLength);
				response.Stream = _postDataTemporaryStore.GetResponseStream(response.RequestId);
			}
		}

		private bool CanRequestBeHandled(string path, PathInformation pathInformation, Link link)
		{
			if (link == null)
			{
				_logger?.Information("Link for path {RequestPath} not found", path);
				return false;
			}

			if (link.IsDisabled)
			{
				_logger?.Information("Link {LinkName} is disabled", link.SymbolicName);
				return false;
			}

			if (String.IsNullOrWhiteSpace(pathInformation.PathWithoutUserName))
			{
				_logger?.Information("Path {RequestPath} for link {LinkName} without user name is not found", path, link.SymbolicName);
				return false;
			}

			if (link.AllowLocalClientRequestsOnly && !Request.IsLocal())
			{
				_logger?.Information("Link {LinkName} only allows local requests", link.SymbolicName);
				return false;
			}

			return true;
		}

		private void FinishRequest(OnPremiseConnectorRequest request, IOnPremiseConnectorResponse response, Guid linkId, string path, HttpStatusCode statusCode)
		{
			if (request == null)
			{
				return;
			}

			request.RequestFinished = DateTime.UtcNow;

			_logger?.Verbose("Finishing request. request-id={RequestId}, link-id={LinkId}, on-premise-duration={RemoteDuration}, global-duration={GlobalDuration}", request.RequestId, linkId, response?.RequestFinished - response?.RequestStarted, request.RequestFinished - request.RequestStarted);

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
			return Request.CreateResponse(HttpStatusCode.NotFound);
		}

		private HttpResponseMessage ServiceUnavailable()
		{
			return Request.CreateResponse(HttpStatusCode.ServiceUnavailable);
		}
	}
}
