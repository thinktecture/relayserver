using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Serilog;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.SignalR
{
	internal class OnPremisesConnection : PersistentConnection
	{
		private readonly ILogger _logger;
		private readonly IBackendCommunication _backendCommunication;

		public OnPremisesConnection(ILogger logger, IBackendCommunication backendCommunication)
		{
			_logger = logger;
			_backendCommunication = backendCommunication ?? throw new ArgumentNullException(nameof(backendCommunication));
		}

		protected override bool AuthorizeRequest(IRequest request)
		{
			if ((request.User?.Identity.IsAuthenticated ?? false) && request.User is ClaimsPrincipal principal)
			{
				var linkId = principal.FindFirst("OnPremiseId")?.Value;
				return !String.IsNullOrWhiteSpace(linkId) && Guid.TryParse(linkId, out var _);
			}

			return false;
		}

		protected override Task OnConnected(IRequest request, string connectionId)
		{
			var onPremiseClaims = GetOnPremiseClaims(request);
			_logger?.Debug("On-premise connected with connection {connection-id}, link {link-id}, user name '{user-name}', role '{role}'", connectionId, onPremiseClaims.OnPremiseId, onPremiseClaims.UserName, onPremiseClaims.Role);

			RegisterOnPremise(request, connectionId, onPremiseClaims);

			return base.OnConnected(request, connectionId);
		}

		protected override Task OnReconnected(IRequest request, string connectionId)
		{
			var onPremiseClaims = GetOnPremiseClaims(request);
			_logger?.Debug("On-premise reconnected with connection {connection-id}, link {link-id}, user name '{user-name}', role '{role}'", connectionId, onPremiseClaims.OnPremiseId, onPremiseClaims.UserName, onPremiseClaims.Role);

			RegisterOnPremise(request, connectionId, onPremiseClaims);

			return base.OnReconnected(request, connectionId);
		}

		protected override Task OnDisconnected(IRequest request, string connectionId, bool stopCalled)
		{
			var onPremiseClaims = GetOnPremiseClaims(request);
			_logger?.Debug("On-premise disconnected with connection {connection-id}, link {link-id}, user name '{user-name}', role '{role}'", connectionId, onPremiseClaims.OnPremiseId, onPremiseClaims.UserName, onPremiseClaims.Role);

			_backendCommunication.UnregisterOnPremise(connectionId);

			return base.OnDisconnected(request, connectionId, stopCalled);
		}

		private Task ForwardClientRequest(string connectionId, IOnPremiseConnectorRequest request)
		{
			_logger?.Verbose("Forwarding client request to connection. connection-id={connection-id}, request-id={request-id}, http-method={request-method}, url={request-url}, origin-id={origin-id}, body-length={request-content-length}",
				connectionId, request.RequestId, request.HttpMethod, request.Url, request.OriginId, request.ContentLength);

			Connection.Send(connectionId, request);
			return Task.CompletedTask;
		}

		protected override Task OnReceived(IRequest request, string connectionId, string data)
		{
			_logger?.Debug("Acknowledge from connection {connection-id} for {data}", connectionId, data);

			_backendCommunication.AcknowledgeOnPremiseConnectorRequest(connectionId, data);

			return base.OnReceived(request, connectionId, data);
		}

		private static OnPremiseClaims GetOnPremiseClaims(IRequest request)
		{
			var claimsPrincipal = (ClaimsPrincipal)request.User;
			var onPremiseId = claimsPrincipal.FindFirst("OnPremiseId")?.Value;

			if (!Guid.TryParse(onPremiseId, out var linkId))
				throw new ArgumentException($"The claim \"OnPremiseId\" is not valid: {onPremiseId}.");

			return new OnPremiseClaims(claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value, linkId, claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value);
		}

		private void RegisterOnPremise(IRequest request, string connectionId, OnPremiseClaims claims)
		{
			_backendCommunication.RegisterOnPremise(new RegistrationInformation()
			{
				ConnectionId = connectionId,
				LinkId = claims.OnPremiseId,
				UserName = claims.UserName,
				Role = claims.Role,
				RequestAction = (cr, cancellationToken) => ForwardClientRequest(connectionId, cr),
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
			Int32.TryParse(request.QueryString["version"], out var version);
			return version;
		}

		private static T Get<T>(IDictionary<string, object> env, string key)
		{
			return env.TryGetValue(key, out var value) ? (T)value : default;
		}

		private class OnPremiseClaims
		{
			public string UserName { get; }
			public Guid OnPremiseId { get; }
			public string Role { get; }

			public OnPremiseClaims(string userName, Guid onPremiseId, string role)
			{
				UserName = userName;
				OnPremiseId = onPremiseId;
				Role = role;
			}
		}
	}
}
