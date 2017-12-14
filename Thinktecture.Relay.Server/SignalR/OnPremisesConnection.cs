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

		protected override async Task OnConnected(IRequest request, string connectionId)
		{
			var onPremiseClaims = GetOnPremiseClaims(request);
			_logger?.Debug("On-premise connected. connection-id={ConnectionId}, link-id={LinkId}, user-name={UserName}, role={Role}", connectionId, onPremiseClaims.OnPremiseId, onPremiseClaims.UserName, onPremiseClaims.Role);

			await RegisterOnPremiseAsync(request, connectionId, onPremiseClaims).ConfigureAwait(false);

			await base.OnConnected(request, connectionId).ConfigureAwait(false);
		}

		protected override async Task OnReconnected(IRequest request, string connectionId)
		{
			var onPremiseClaims = GetOnPremiseClaims(request);
			_logger?.Debug("On-premise reconnected. connection-id={ConnectionId}, link-id={LinkId}, user-name={UserName}, role={Role}", connectionId, onPremiseClaims.OnPremiseId, onPremiseClaims.UserName, onPremiseClaims.Role);

			await RegisterOnPremiseAsync(request, connectionId, onPremiseClaims).ConfigureAwait(false);

			await base.OnReconnected(request, connectionId).ConfigureAwait(false);
		}

		protected override async Task OnDisconnected(IRequest request, string connectionId, bool stopCalled)
		{
			var onPremiseClaims = GetOnPremiseClaims(request);
			_logger?.Debug("On-premise disconnected. connection-id={ConnectionId}, link-id={LinkId}, user-name={UserName}, role={Role}", connectionId, onPremiseClaims.OnPremiseId, onPremiseClaims.UserName, onPremiseClaims.Role);

			await _backendCommunication.UnregisterOnPremiseAsync(connectionId).ConfigureAwait(false);

			await base.OnDisconnected(request, connectionId, stopCalled).ConfigureAwait(false);
		}

		private async Task ForwardClientRequestAsync(string connectionId, IOnPremiseConnectorRequest request)
		{
			_logger?.Verbose("Forwarding client request to connection. connection-id={ConnectionId}, request-id={RequestId}, http-method={RequestMethod}, url={RequestUrl}, origin-id={OriginId}, body-length={RequestContentLength}",
				connectionId, request.RequestId, request.HttpMethod, request.Url, request.OriginId, request.ContentLength);

			await Connection.Send(connectionId, request).ConfigureAwait(false);
		}

		private static OnPremiseClaims GetOnPremiseClaims(IRequest request)
		{
			var claimsPrincipal = (ClaimsPrincipal)request.User;
			var onPremiseId = claimsPrincipal.FindFirst("OnPremiseId")?.Value;

			if (!Guid.TryParse(onPremiseId, out var linkId))
				throw new ArgumentException($"The claim \"OnPremiseId\" is not valid: {onPremiseId}.");

			return new OnPremiseClaims(claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value, linkId, claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value);
		}

		private async Task RegisterOnPremiseAsync(IRequest request, string connectionId, OnPremiseClaims claims)
		{
			await _backendCommunication.RegisterOnPremiseAsync(new RegistrationInformation()
			{
				ConnectionId = connectionId,
				LinkId = claims.OnPremiseId,
				UserName = claims.UserName,
				Role = claims.Role,
				RequestAction = (cr, cancellationToken) => ForwardClientRequestAsync(connectionId, cr),
				IpAddress = GetIpAddressFromOwinEnvironment(request.Environment),
				ConnectorVersion = GetConnectorVersionFromRequest(request),
				ConnectorAssemblyVersion = GetConnectorAssemblyVersionFromRequest(request),
			}).ConfigureAwait(false);
		}

		// Adopted from http://stackoverflow.com/questions/11044361/signalr-get-caller-ip-address
		private string GetIpAddressFromOwinEnvironment(IDictionary<string, object> environment)
		{
			return Get<string>(environment, "server.RemoteIpAddress");
		}

		private int GetConnectorVersionFromRequest(IRequest request)
		{
			Int32.TryParse(request.QueryString["cv"], out var version);
			return version;
		}

		private string GetConnectorAssemblyVersionFromRequest(IRequest request)
		{
			return request.QueryString["av"] ?? "Unknown";
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
