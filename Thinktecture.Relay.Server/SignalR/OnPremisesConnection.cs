using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using NLog.Interface;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.SignalR
{
    internal class OnPremisesConnection : PersistentConnection
    {
        private readonly IBackendCommunication _backendCommunication;
        private readonly IPostDataTemporaryStore _temporaryStore;
        private readonly ILogger _logger;

        public OnPremisesConnection(IBackendCommunication backendCommunication, IPostDataTemporaryStore temporaryStore, ILogger logger)
        {
            _backendCommunication = backendCommunication;
            _temporaryStore = temporaryStore;
            _logger = logger;

        }

        protected override bool AuthorizeRequest(IRequest request)
        {
            if (request.User != null)
            {
                return request.User.Identity.IsAuthenticated;
            }
            return false;
        }

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            string onPremiseId = GetOnPremiseIdFromRequest(request);

            _logger.Debug("OnPremise {0} connected", onPremiseId);

            RegisterOnPremise(request, connectionId, onPremiseId);

            return base.OnConnected(request, connectionId);
        }

        private async Task ForwardClientRequest(string connectionId, IOnPremiseConnectorRequest onPremiseConnectorRequest)
        {
            _logger.Debug("Forwarding client request to connection '{0}'", connectionId);
            _logger.Trace("Forwarding client request to connection '{0}'. request-id={1}, http-method={2}, url={3}, origin-id={4}, body-length={5}",
                connectionId, onPremiseConnectorRequest.RequestId, onPremiseConnectorRequest.HttpMethod, onPremiseConnectorRequest.Url, onPremiseConnectorRequest.OriginId, onPremiseConnectorRequest.Body != null ? onPremiseConnectorRequest.Body.Length : 0);

            var onPremiseTargetRequest = new OnPremiseTargetRequest()
            {
                RequestId = onPremiseConnectorRequest.RequestId,
                HttpMethod = onPremiseConnectorRequest.HttpMethod,
                Url = onPremiseConnectorRequest.Url,
                HttpHeaders = onPremiseConnectorRequest.HttpHeaders,
                OriginId = onPremiseConnectorRequest.OriginId
            };

            if (onPremiseConnectorRequest.Body != null)
            {
                if (onPremiseConnectorRequest.Body.Length > 0x20000) // 128k
                {
                    _temporaryStore.Save(onPremiseConnectorRequest.RequestId, onPremiseConnectorRequest.Body);
                    onPremiseTargetRequest.Body = String.Empty;
                }
                else
                {
                    onPremiseTargetRequest.Body = Convert.ToBase64String(onPremiseConnectorRequest.Body);
                }
            }

            await Connection.Send(connectionId, onPremiseTargetRequest);
        }

        protected override Task OnReconnected(IRequest request, string connectionId)
        {
            var onPremiseId = GetOnPremiseIdFromRequest(request);

            _logger.Debug("OnPremise {0} reconnected.", onPremiseId);

            RegisterOnPremise(request, connectionId, onPremiseId);

            return base.OnReconnected(request, connectionId);
        }

        private static string GetOnPremiseIdFromRequest(IRequest request)
        {
            string onPremiseId = ((ClaimsPrincipal)request.User).Claims.Single(c => c.Type == "OnPremiseId").Value;
            return onPremiseId;
        }

        private void RegisterOnPremise(IRequest request, string connectionId, string onPremiseId)
        {
            _backendCommunication.RegisterOnPremise(new RegistrationInformation()
            {
                ConnectionId = connectionId,
                OnPremiseId = onPremiseId,
                RequestAction = async cr => await ForwardClientRequest(connectionId, cr),
                IpAddress = GetIpAddressFromOwinEnvironment(request.Environment)
            });
        }

        protected override Task OnDisconnected(IRequest request, string connectionId, bool stopCalled)
        {
            _logger.Debug("OnPremise {0} disconnected - V2.", connectionId);
            _backendCommunication.UnregisterOnPremise(connectionId);

            return base.OnDisconnected(request, connectionId, stopCalled);
        }

        // Adopted from http://stackoverflow.com/questions/11044361/signalr-get-caller-ip-address
        private string GetIpAddressFromOwinEnvironment(IDictionary<string, object> environment)
        {
            var ipAddress = Get<string>(environment, "server.RemoteIpAddress");
            return ipAddress;
        }

        private static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) ? (T)value : default(T);
        }
    }
}
