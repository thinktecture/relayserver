using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using NLog.Interface;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.Http;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Controller
{
    [AllowAnonymous]
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

        public ClientController(IBackendCommunication backendCommunication, ILogger logger, ILinkRepository linkRepository, IRequestLogger requestLogger,
            IHttpResponseMessageBuilder httpResponseMessageBuilder, IOnPremiseRequestBuilder onPremiseRequestBuilder, IPathSplitter pathSplitter,
            ITraceManager traceManager)
        {
            _backendCommunication = backendCommunication;
            _logger = logger;
            _linkRepository = linkRepository;
            _requestLogger = requestLogger;
            _httpResponseMessageBuilder = httpResponseMessageBuilder;
            _onPremiseRequestBuilder = onPremiseRequestBuilder;
            _pathSplitter = pathSplitter;
            _traceManager = traceManager;
        }

        [HttpDelete]
        [HttpGet]
        [HttpHead]
        [HttpPost]
        [HttpPut]
        [HttpOptions]
        public async Task<HttpResponseMessage> Relay(string path)
        {
            _logger.Trace("Relaying http {0} {1}", path, ControllerContext.Request.Method);

            if (path == null)
            {
                _logger.Info("Path is not set.");
                return NotFound();
            }

            var pathInformation = _pathSplitter.Split(path);
            var link = _linkRepository.GetLink(pathInformation.UserName);

            if (link == null)
            {
                _logger.Info("Link for requested path {0} not found", path);
                return NotFound();
            }

            if (link.IsDisabled)
            {
                _logger.Info("{0}: Link {1} is disabled", link.Id, link.SymbolicName);
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(pathInformation.PathWithoutUserName))
            {
                _logger.Info("{0}: Path without username is not found. Wrong path information: {1}", link.Id, path);
                return NotFound();
            }

            if (link.AllowLocalClientRequestsOnly && !Request.IsLocal())
            {
                _logger.Info("{0}: Link {1} only allows local requests.", link.Id, link.SymbolicName);
                return NotFound();
            }

            _logger.Trace("{0}: Building on premise connector request. Origin Id: {1}, Path: {2}", link.Id, _backendCommunication.OriginId, path);
            var onPremiseConnectorRequest =
                (OnPremiseConnectorRequest)
                    await _onPremiseRequestBuilder.BuildFrom(Request, _backendCommunication.OriginId, pathInformation.PathWithoutUserName);

            _logger.Trace("{0}: Sending on premise connector request.", link.Id);
            await _backendCommunication.SendOnPremiseConnectorRequest(link.Id.ToString(), onPremiseConnectorRequest);

            _logger.Trace("{0}: Waiting for response. Request Id", onPremiseConnectorRequest.RequestId);
            var onPremiseTargetReponse = await _backendCommunication.GetResponseAsync(onPremiseConnectorRequest.RequestId);

            if (onPremiseTargetReponse != null)
            {
                _logger.Trace("{0}: Response received from {1}", link.Id, onPremiseTargetReponse.RequestId);
            }
            else
            {
                _logger.Trace("{0}: On Premise Timeout", link.Id);     
            }

            var response = _httpResponseMessageBuilder.BuildFrom(onPremiseTargetReponse, link);

            onPremiseConnectorRequest.RequestFinished = DateTime.UtcNow;

            var currentTraceConfigurationId = _traceManager.GetCurrentTraceConfigurationId(link.Id);
            if (currentTraceConfigurationId != null)
            {
                _traceManager.Trace(onPremiseConnectorRequest, onPremiseTargetReponse, currentTraceConfigurationId.Value);
            }

            _requestLogger.LogRequest(onPremiseConnectorRequest, onPremiseTargetReponse, response.StatusCode, link.Id, _backendCommunication.OriginId, path);

            return response;
        }

        private new HttpResponseMessage NotFound()
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}