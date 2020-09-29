using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;
using Serilog;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.SignalR
{
	internal class OnPremisesConnection : PersistentConnection
	{
		private readonly ILogger _logger;
		private readonly IBackendCommunication _backendCommunication;
		private readonly IConfiguration _configuration;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IOnPremisesConnectionOnReceivedHandler _onPremisesConnectionOnReceivedHandler;

		public OnPremisesConnection(ILogger logger, IBackendCommunication backendCommunication, IConfiguration configuration, ILifetimeScope lifetimeScope,
			IOnPremisesConnectionOnReceivedHandler onPremisesConnectionOnReceivedHandler)
		{
			_logger = logger;
			_backendCommunication = backendCommunication ?? throw new ArgumentNullException(nameof(backendCommunication));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_onPremisesConnectionOnReceivedHandler = onPremisesConnectionOnReceivedHandler ?? throw new ArgumentNullException(nameof(onPremisesConnectionOnReceivedHandler));
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

		protected override async Task OnReceived(IRequest request, string connectionId, string data)
		{
			_logger?.Debug("On-premise sent. connection-id={ConnectionId}, data={Data}", connectionId, data);

			await _onPremisesConnectionOnReceivedHandler.HandleAsync(connectionId, data);
			await base.OnReceived(request, connectionId, data);
		}

		protected override async Task OnDisconnected(IRequest request, string connectionId, bool stopCalled)
		{
			var onPremiseClaims = GetOnPremiseClaims(request);
			_logger?.Debug("On-premise disconnected. connection-id={ConnectionId}, link-id={LinkId}, user-name={UserName}, role={Role}", connectionId, onPremiseClaims.OnPremiseId, onPremiseClaims.UserName, onPremiseClaims.Role);

			await _backendCommunication.UnregisterOnPremiseConnectionAsync(connectionId).ConfigureAwait(false);

			await base.OnDisconnected(request, connectionId, stopCalled).ConfigureAwait(false);
		}

		private async Task ForwardClientRequestAsync(string connectionId, IOnPremiseConnectorRequest request)
		{
			try
			{
				_logger?.Verbose("Forwarding client request to connection. connection-id={ConnectionId}, request-id={RequestId}, http-method={RequestMethod}, url={RequestUrl}, origin-id={OriginId}, body-length={RequestContentLength}",
					connectionId, request.RequestId, request.HttpMethod, _configuration.LogSensitiveData ? request.Url : new UrlParameterFilter(request.Url).ToString(), request.OriginId, request.ContentLength);

				var json = JObject.FromObject(request);
				if (request.Properties != null)
				{
					json.Remove(nameof(IOnPremiseConnectorRequest.Properties));

					foreach (var kvp in request.Properties)
					{
						json[kvp.Key] = JToken.FromObject(kvp.Value);
					}
				}

				await Connection.Send(connectionId, json).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "An error occured forwarding request to connection. connection-id={ConnectionId}, request={@Request}", connectionId, request);
			}
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
			var context = _lifetimeScope.Resolve<IOnPremiseConnectionContext>();

			context.ConnectionId = connectionId;
			context.LinkId = claims.OnPremiseId;
			context.UserName = claims.UserName;
			context.Role = claims.Role;
			context.RequestAction = (cr, cancellationToken) => ForwardClientRequestAsync(connectionId, cr);
			context.IpAddress = GetIpAddressFromOwinEnvironment(request.Environment);
			context.ConnectorVersion = GetConnectorVersionFromRequest(request);
			context.ConnectorAssemblyVersion = GetConnectorAssemblyVersionFromRequest(request);

			await _backendCommunication.RegisterOnPremiseAsync(context).ConfigureAwait(false);
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
