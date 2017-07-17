using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using NLog;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.Communication;

namespace Thinktecture.Relay.Server.SignalR
{
    internal class OnPremisesConnection : PersistentConnection
    {
        private readonly IBackendCommunication _backendCommunication;
        private readonly IPostDataTemporaryStore _temporaryStore;
        private readonly ILogger _logger;

        public OnPremisesConnection(IBackendCommunication backendCommunication, IPostDataTemporaryStore temporaryStore, ILogger logger)
        {
			_backendCommunication = backendCommunication ?? throw new ArgumentNullException(nameof(backendCommunication));
            _temporaryStore = temporaryStore ?? throw new ArgumentNullException(nameof(temporaryStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override bool AuthorizeRequest(IRequest request)
        {
            return request.User?.Identity.IsAuthenticated ?? false;
        }

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            var onPremiseClaims = GetOnPremiseClaims(request);
            _logger.Debug("On-premise connected. Connection Id: {0}, OnPremise Id: {1}, User name: {2}, Role: {3}", connectionId, onPremiseClaims.OnPremiseId, onPremiseClaims.UserName, onPremiseClaims.Role);

            RegisterOnPremise(request, connectionId, onPremiseClaims);

            return base.OnConnected(request, connectionId);
        }

        protected override Task OnReconnected(IRequest request, string connectionId)
        {
            var onPremiseClaims = GetOnPremiseClaims(request);
            _logger.Debug("On-premise reconnected. Connection Id: {0}, OnPremise Id: {1}, User name: {2}, Role: {3}", connectionId, onPremiseClaims.OnPremiseId, onPremiseClaims.UserName, onPremiseClaims.Role);

            RegisterOnPremise(request, connectionId, onPremiseClaims);

            return base.OnReconnected(request, connectionId);
        }

        protected override Task OnDisconnected(IRequest request, string connectionId, bool stopCalled)
        {
            var onPremiseClaims = GetOnPremiseClaims(request);
            _logger.Debug("On-premise disconnected. Connection Id: {0}, OnPremise Id: {1}, User name: {2}, Role: {3}", connectionId, onPremiseClaims.OnPremiseId, onPremiseClaims.UserName, onPremiseClaims.Role);

            _backendCommunication.UnregisterOnPremise(connectionId);

            return base.OnDisconnected(request, connectionId, stopCalled);
        }

        private async Task ForwardClientRequest(string connectionId, OnPremiseTargetRequest request)
        {
            _logger.Debug("Forwarding client request to connection {0}", connectionId);
            _logger.Trace("Forwarding client request to connection. connection-id={0}, request-id={1}, http-method={2}, url={3}, origin-id={4}, body-length={5}",
                connectionId, request.RequestId, request.HttpMethod, request.Url, request.OriginId, request.Body?.Length ?? 0);

            if (request.Body != null)
            {
                // always store request in temporary store
                _temporaryStore.Save(request.RequestId, request.Body);
                request.Body = new byte[0];
            }

            await Connection.Send(connectionId, request);
        }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            _logger.Debug("Acknowledge from connection {0} for {1}", connectionId, data);

            _backendCommunication.AcknowledgeOnPremiseConnectorRequest(connectionId, data);

            return base.OnReceived(request, connectionId, data);
        }

        private static OnPremiseClaims GetOnPremiseClaims(IRequest request)
        {
            var claims = ((ClaimsPrincipal) request.User).Claims;

            return new OnPremiseClaims(
                claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                claims.Single(c => c.Type == "OnPremiseId").Value,
                claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
            );
        }

        private void RegisterOnPremise(IRequest request, string connectionId, OnPremiseClaims claims)
        {
            _backendCommunication.RegisterOnPremise(new RegistrationInformation()
            {
                ConnectionId = connectionId,
                OnPremiseId = claims.OnPremiseId,
                UserName = claims.UserName,
                Role = claims.Role,
                RequestAction = cr => ForwardClientRequest(connectionId, (OnPremiseTargetRequest)cr),
                IpAddress = GetIpAddressFromOwinEnvironment(request.Environment),
                ConnectorVersion = GetConnectorVersionFromRequest(request),
            });
        }

        // Adopted from http://stackoverflow.com/questions/11044361/signalr-get-caller-ip-address
        private string GetIpAddressFromOwinEnvironment(IDictionary<string, object> environment)
        {
            return Get<string>(environment, "server.RemoteIpAddress");
        }

        private int GetConnectorVersionFromRequest(IRequest request)
        {
            string queryArgument = request.QueryString["version"];

            if (!String.IsNullOrWhiteSpace(queryArgument))
            {
                return int.Parse(queryArgument);
            }

            return 0;
        }

        private static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) ? (T) value : default(T);
        }

        private class OnPremiseClaims
        {
            public string UserName { get; }
            public string OnPremiseId { get; }
            public string Role { get; }

            public OnPremiseClaims(string userName, string onPremiseId, string role)
            {
                UserName = userName;
                OnPremiseId = onPremiseId;
                Role = role;
            }
        }
    }
}