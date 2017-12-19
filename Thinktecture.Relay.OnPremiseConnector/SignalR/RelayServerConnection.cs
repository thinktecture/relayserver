using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Thinktecture.IdentityModel.Client;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal class RelayServerConnection : Connection, IRelayServerConnection
	{
		private const int _CONNECTOR_VERSION = 2;
		private const int _MIN_WAIT_TIME_IN_SECONDS = 2;
		private const int _MAX_WAIT_TIME_IN_SECONDS = 30;

		private static int _nextId;
		private static readonly Random _random = new Random();

		private readonly string _userName;
		private readonly string _password;
		private readonly int _requestTimeoutInSeconds;
		private readonly IOnPremiseTargetConnectorFactory _onPremiseTargetConnectorFactory;
		private readonly ILogger _logger;
		private readonly ConcurrentDictionary<string, IOnPremiseTargetConnector> _connectors;
		private readonly HttpClient _httpClient;

		private CancellationTokenSource _cts;
		private bool _stopRequested;
		private string _accessToken;

		public event EventHandler Disposing;

		public Uri Uri { get; }
		public TimeSpan TokenRefreshWindow { get; }
		public DateTime TokenExpiry { get; private set; } = DateTime.MaxValue;
		public int RelayServerConnectionId { get; }
		public DateTime LastHeartbeat { get; private set; } = DateTime.MinValue;
		public TimeSpan HeartbeatInterval { get; private set; }

		public RelayServerConnection(string userName, string password, Uri relayServerUri, int requestTimeoutInSeconds,
			int tokenRefreshWindowInSeconds, IOnPremiseTargetConnectorFactory onPremiseTargetConnectorFactory, ILogger logger)
			: base(new Uri(relayServerUri, "/signalr").AbsoluteUri, $"cv={_CONNECTOR_VERSION}&av={Assembly.GetEntryAssembly().GetName().Version}")
		{
			RelayServerConnectionId = Interlocked.Increment(ref _nextId);
			_userName = userName;
			_password = password;
			Uri = relayServerUri;
			_requestTimeoutInSeconds = requestTimeoutInSeconds;
			TokenRefreshWindow = TimeSpan.FromSeconds(tokenRefreshWindowInSeconds);

			_onPremiseTargetConnectorFactory = onPremiseTargetConnectorFactory;
			_logger = logger;
			_connectors = new ConcurrentDictionary<string, IOnPremiseTargetConnector>(StringComparer.OrdinalIgnoreCase);
			_httpClient = new HttpClient()
			{
				Timeout = TimeSpan.FromSeconds(requestTimeoutInSeconds),
			};
			_cts = new CancellationTokenSource();

			Reconnecting += OnReconnecting;
			Reconnected += OnReconnected;
		}

		public string RelayedRequestHeader { get; set; }

		public void RegisterOnPremiseTarget(string key, Uri baseUri)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (baseUri == null)
				throw new ArgumentNullException(nameof(baseUri));

			key = RemoveTrailingSlashes(key);

			_logger?.Verbose("Registering on-premise web target. handler-key={HandlerKey}, base-uri={BaseUri}", key, baseUri);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(baseUri, _requestTimeoutInSeconds);
		}

		public void RegisterOnPremiseTarget(string key, Type handlerType)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			key = RemoveTrailingSlashes(key);

			_logger?.Verbose("Registering on-premise in-proc target. handler-key={HandlerKey}, handler-type={HandlerType}", key, handlerType);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(handlerType, _requestTimeoutInSeconds);
		}

		public void RegisterOnPremiseTarget(string key, Func<IOnPremiseInProcHandler> handlerFactory)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (handlerFactory == null)
				throw new ArgumentNullException(nameof(handlerFactory));

			key = RemoveTrailingSlashes(key);

			_logger?.Verbose("Registering on-premise in-proc target using a handler factory. handler-key={HandlerKey}", key);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(handlerFactory, _requestTimeoutInSeconds);
		}

		public void RegisterOnPremiseTarget<T>(string key) where T : IOnPremiseInProcHandler, new()
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			key = RemoveTrailingSlashes(key);

			_logger?.Verbose("Registering on-premise in-proc target. handler-key={HandlerKey}, handler-type={HandlerType}", key, typeof(T).Name);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create<T>(_requestTimeoutInSeconds);
		}

		private static string RemoveTrailingSlashes(string key)
		{
			while (key.EndsWith("/"))
			{
				key = key.Substring(0, key.Length - 1);
			}

			return key;
		}

		public void RemoveOnPremiseTarget(string key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			key = RemoveTrailingSlashes(key);
			_connectors.TryRemove(key, out var old);
		}

		private async Task<TokenResponse> GetAuthorizationTokenAsync()
		{
			var client = new OAuth2Client(new Uri(Uri, "/token"));

			while (!_stopRequested)
			{
				try
				{
					_logger?.Verbose("Requesting authorization token. relay-server={RelayServerUri}, relay-server-id={RelayServerConnectionId}", Uri, RelayServerConnectionId);

					var response = await client.RequestResourceOwnerPasswordAsync(_userName, _password).ConfigureAwait(false);

					_logger?.Verbose("Received token. relay-server={RelayServerUri}, relay-server-id={RelayServerId}", Uri, RelayServerConnectionId);
					return response;
				}
				catch
				{
					var randomWaitTime = GetRandomWaitTime();
					_logger?.Information("Could not authenticate with relay server - re-trying in {RetryWaitTime} seconds", randomWaitTime.TotalSeconds);
					await Task.Delay(randomWaitTime, _cts.Token).ConfigureAwait(false);
				}
			}

			return null;
		}

		public async Task ConnectAsync()
		{
			_logger?.Information("Connecting to relay server {RelayServerUri} with connection id {RelayServerConnectionId}", Uri, RelayServerConnectionId);

			if (!await TryRequestAuthorizationTokenAsync().ConfigureAwait(false))
			{
				return;
			}

			try
			{
				await Start().ConfigureAwait(false);
				_logger?.Information("Connected to relay server {RelayServerUri} with connection id {RelayServerConnectionId}", Uri, RelayServerConnectionId);
			}
			catch
			{
				_logger?.Information("Error while connecting to relay server {RelayServerUri} with connection id {RelayServerConnectionId}", Uri, RelayServerConnectionId);
			}
		}

		public async Task<bool> TryRequestAuthorizationTokenAsync()
		{
			var tokenResponse = await GetAuthorizationTokenAsync().ConfigureAwait(false);

			if (_stopRequested)
			{
				return false;
			}

			CheckResponseTokenForErrors(tokenResponse);
			SetBearerToken(tokenResponse);

			return true;
		}

		private void SetBearerToken(TokenResponse tokenResponse)
		{
			_accessToken = tokenResponse.AccessToken;
			TokenExpiry = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResponse.ExpiresIn);

			_httpClient.SetBearerToken(_accessToken);

			Headers["Authorization"] = $"{tokenResponse.TokenType} {_accessToken}";

			_logger?.Verbose("Setting access token. token-expiry={TokenExpiry}", TokenExpiry);
		}

		private void CheckResponseTokenForErrors(TokenResponse token)
		{
			if (token.IsHttpError)
			{
				_logger?.Warning("Could not authenticate with relay server. relay-server-id={RelayServerId}, status-code={ConnectionHttpStatusCode}, reason={ConnectionErrorReason}", RelayServerConnectionId, token.HttpErrorStatusCode, token.HttpErrorReason);
				throw new AuthenticationException("Could not authenticate with relay server: " + token.HttpErrorReason);
			}

			if (token.IsError)
			{
				_logger?.Warning("Could not authenticate with relay server. relay-server-id={RelayServerId}, reason={ConnectionErrorReason}", RelayServerConnectionId, token.Error);
				throw new AuthenticationException("Could not authenticate with relay server: " + token.Error);
			}
		}

		private void OnReconnected()
		{
			_logger?.Verbose("Connection restored. relay-server={RelayServerUri}, relay-server-id={RelayServerConnectionId}", Uri, RelayServerConnectionId);
		}

		private void OnReconnecting()
		{
			_logger?.Verbose("Connection lost. relay-server={RelayServerUri}, relay-server-id={RelayServerConnectionId}", Uri, RelayServerConnectionId);
		}

		protected override async void OnMessageReceived(JToken message)
		{
			var ctx = new RequestContext();
			IOnPremiseTargetRequestInternal request = null;

			try
			{
				_logger?.Verbose("Received message from server. message={Message}", message);

				request = message.ToObject<OnPremiseTargetRequest>();

				try
				{
					if (request.HttpMethod == "PING")
					{
						await HandlePingRequestAsync(ctx, request).ConfigureAwait(false);
						return;
					}

					if (request.HttpMethod == "HEARTBEAT")
					{
						HandleHeartbeatRequest(ctx, request);
						return;
					}
				}
				finally
				{
					await AcknowlegdeRequest(request).ConfigureAwait(false);
				}

				var key = request.Url.Split('/').FirstOrDefault();
				if (key != null)
				{
					if (_connectors.TryGetValue(key, out var connector))
					{
						_logger?.Verbose("Found on-premise target and sending request. on-premise-key={OnPremiseTargetKey}, request-id={RequestId}", key, request.RequestId);

						await RequestLocalTargetAsync(ctx, key, connector, request, CancellationToken.None).ConfigureAwait(false);
						return;
					}
				}

				_logger?.Verbose("No connector found for local server. request-id={RequestId}, request-url={RequestUrl}", request.RequestId, request.Url);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during handling received message. message={Message}", message);
			}
			finally
			{
				if (!ctx.IsRelayServerNotified && request != null)
				{
					_logger?.Verbose("Unhandled request. message={message}", message);

					var response = new OnPremiseTargetResponse()
					{
						RequestStarted = ctx.StartDate,
						RequestFinished = DateTime.UtcNow,
						StatusCode = HttpStatusCode.NotFound,
						OriginId = request.OriginId,
						RequestId = request.RequestId,
					};

					try
					{
						await PostResponseAsync(ctx, response, CancellationToken.None).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						_logger?.Error(ex, "Error during posting to relay");
					}
				}
			}
		}

		private async Task AcknowlegdeRequest(IOnPremiseTargetRequest request)
		{
			if (request.AcknowledgmentMode == AcknowledgmentMode.Auto)
			{
				_logger?.Debug("Automatically acknowlegded by relay server. request-id={RequestId}, connection-id={ConnectionId}", request.RequestId, ConnectionId);
				return;
			}

			if (String.IsNullOrEmpty(request.AcknowledgeId))
			{
				_logger?.Debug("No acknowledgment possible. request-id={RequestId}, connection-id={ConnectionId}, acknowledment-mode={AcknowledgmentMode}", request.RequestId, ConnectionId, request.AcknowledgmentMode);
				return;
			}

			switch (request.AcknowledgmentMode)
			{
				case AcknowledgmentMode.Default:
					_logger?.Debug("Sending acknowlegde to relay server. request-id={RequestId}, connection-id={ConnectionId}, acknowledge-id={AcknowledgeId}", request.RequestId, ConnectionId, request.AcknowledgeId);
					await GetToRelay($"/request/acknowledge?id={ConnectionId}&tag={request.AcknowledgeId}", CancellationToken.None).ConfigureAwait(false);
					break;

				case AcknowledgmentMode.Manual:
					_logger?.Debug("Manual acknowledgment needed. request-id={RequestId}, connection-id={ConnectionId}, acknowledge-id={AcknowledgeId}", request.RequestId, ConnectionId, request.AcknowledgeId);
					break;

				default:
					_logger?.Warning("Unknown acknowledgment mode found. request-id={RequestId}, connection-id={ConnectionId}, acknowledment-mode={AcknowledgmentMode}, acknowledge-id={AcknowledgeId}", request.RequestId, ConnectionId, request.AcknowledgmentMode, request.AcknowledgeId);
					break;
			}
		}

		private async Task HandlePingRequestAsync(RequestContext ctx, IOnPremiseTargetRequest request)
		{
			_logger?.Debug("Received ping from relay server. relay-server={RelayServerUri}, relay-server-id={RelayServerConnectionId}", Uri, RelayServerConnectionId);

			var response = new OnPremiseTargetResponse()
			{
				RequestStarted = DateTime.UtcNow,
				RequestFinished = DateTime.UtcNow,
				StatusCode = HttpStatusCode.OK,
				OriginId = request.OriginId,
				RequestId = request.RequestId,
			};

			await PostResponseAsync(ctx, response, CancellationToken.None).ConfigureAwait(false);
		}

		private void HandleHeartbeatRequest(RequestContext ctx, IOnPremiseTargetRequest request)
		{
			_logger?.Debug("Received heartbeat from relay server. relay-server={RelayServerUri}, relay-server-id={RelayServerConnectionId}", Uri, RelayServerConnectionId);

			if (LastHeartbeat == DateTime.MinValue)
			{
				request.HttpHeaders.TryGetValue("X-TTRELAY-HEARTBEATINTERVAL", out var heartbeatHeaderValue);
				if (Int32.TryParse(heartbeatHeaderValue, out var heartbeatInterval))
				{
					_logger?.Information("Heartbeat interval set to {HeartbeatInterval} seconds", heartbeatInterval);
					HeartbeatInterval = TimeSpan.FromSeconds(heartbeatInterval);
				}
			}

			LastHeartbeat = DateTime.UtcNow;
			ctx.IsRelayServerNotified = true;
		}

		public void Reconnect()
		{
			_logger?.Debug("Forcing reconnect. relay-server={RelayServerUri}, relay-server-id={RelayServerConnectionId}", Uri, RelayServerConnectionId);

			Disconnect();

			Task.Delay(TimeSpan.FromMilliseconds(500)).ContinueWith(_ =>
			{
				_stopRequested = false;
				return ConnectAsync().ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

		public void Disconnect()
		{
			_logger?.Information("Disconnecting from relay server {RelayServerUri} with connection id {RelayServerConnectionId}", Uri, RelayServerConnectionId);

			LastHeartbeat = DateTime.MinValue;
			HeartbeatInterval = TimeSpan.Zero;

			_stopRequested = true;
			Stop();
		}

		public List<string> GetOnPremiseTargetKeys()
		{
			return _connectors.Keys.ToList();
		}

		private async Task RequestLocalTargetAsync(RequestContext ctx, string key, IOnPremiseTargetConnector connector, IOnPremiseTargetRequestInternal request, CancellationToken cancellationToken)
		{
			_logger?.Debug("Relaying request to local target. request-url={RequestUrl}, request-id={RequestId}", request.Url, request.RequestId);

			var url = (request.Url.Length > key.Length) ? request.Url.Substring(key.Length + 1) : String.Empty;

			if (request.Body != null)
			{
				if (request.Body.Length == 0)
				{
					// a length of 0 indicates that there is a larger body available on the server
					_logger?.Verbose("Requesting body. relay-server={RelayServerUri}, relay-server-id={RelayServerConnectionId}, request-id={RequestId}", Uri, RelayServerConnectionId, request.RequestId);
					// request the body from the relay server (because SignalR cannot handle large messages)
					var webResponse = await GetToRelay("/request/" + request.RequestId, cancellationToken).ConfigureAwait(false);
					request.Stream = await webResponse.Content.ReadAsStreamAsync().ConfigureAwait(false); // this stream should not be disposed (owned by the Framework)
				}
				else
				{
					// the body is small enough to be used directly
					request.Stream = new MemoryStream(request.Body);
				}
			}
			else
			{
				// no body available (e.g. GET request)
				request.Stream = Stream.Null;
			}

			using (var response = await connector.GetResponseFromLocalTargetAsync(url, request, RelayedRequestHeader).ConfigureAwait(false))
			{
				if (response.Stream == null)
				{
					response.Stream = Stream.Null;
				}

				_logger?.Debug("Sending response. request-url={RequestUrl}, relay-server={RelayServerUri}, relay-server-id={RelayServerConnectionId}", request.Url, Uri, RelayServerConnectionId);

				try
				{
					// transfer the result to the relay server (need POST here, because SignalR does not handle large messages)
					await PostResponseAsync(ctx, response, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger?.Error(ex, "Error during posting to relay");
				}
			}
		}

		private async Task PostResponseAsync(RequestContext ctx, IOnPremiseTargetResponse response, CancellationToken cancellationToken)
		{
			await PostToRelay("/forward", headers => headers.Add("X-TTRELAY-METADATA", JsonConvert.SerializeObject(response)), new StreamContent(response.Stream ?? Stream.Null), cancellationToken).ConfigureAwait(false);
			ctx.IsRelayServerNotified = true;
		}

		protected override void OnClosed()
		{
			_logger?.Information("Connection closed to relay server {RelayServerUri} with connection id {RelayServerConnectionId}", Uri, RelayServerConnectionId);

			base.OnClosed();

			if (!_stopRequested)
			{
				var randomWaitTime = GetRandomWaitTime();
				_logger?.Debug("Connection closed. reconnecte-wait-time={ReconnectWaitTime}", randomWaitTime.TotalSeconds);
				Task.Delay(randomWaitTime).ContinueWith(_ => ConnectAsync());
			}
		}

		public Task<HttpResponseMessage> GetToRelay(string relativeUrl, CancellationToken cancellationToken)
		{
			return GetToRelay(relativeUrl, null, cancellationToken);
		}

		public Task<HttpResponseMessage> GetToRelay(string relativeUrl, Action<HttpRequestHeaders> setHeaders, CancellationToken cancellationToken)
		{
			return SendToRelayAsync(relativeUrl, HttpMethod.Get, setHeaders, null, cancellationToken);
		}

		public Task<HttpResponseMessage> PostToRelay(string relativeUrl, HttpContent content, CancellationToken cancellationToken)
		{
			return PostToRelay(relativeUrl, null, content, cancellationToken);
		}

		public Task<HttpResponseMessage> PostToRelay(string relativeUrl, Action<HttpRequestHeaders> setHeaders, HttpContent content, CancellationToken cancellationToken)
		{
			return SendToRelayAsync(relativeUrl, HttpMethod.Post, setHeaders, content, cancellationToken);
		}

		private async Task<HttpResponseMessage> SendToRelayAsync(string relativeUrl, HttpMethod httpMethod, Action<HttpRequestHeaders> setHeaders, HttpContent content, CancellationToken cancellationToken)
		{
			if (String.IsNullOrWhiteSpace(relativeUrl))
				throw new ArgumentException("Relative url cannot be null or empty.");

			if (!relativeUrl.StartsWith("/"))
				relativeUrl = "/" + relativeUrl;

			var url = new Uri(Uri, relativeUrl);

			var request = new HttpRequestMessage(httpMethod, url);

			setHeaders?.Invoke(request.Headers);
			request.Content = content;

			return await _httpClient.SendAsync(request, cancellationToken);
		}

		private TimeSpan GetRandomWaitTime()
		{
			return TimeSpan.FromSeconds(_random.Next(_MIN_WAIT_TIME_IN_SECONDS, _MAX_WAIT_TIME_IN_SECONDS));
		}

		private class RequestContext
		{
			public DateTime StartDate { get; }
			public bool IsRelayServerNotified { get; set; }

			public RequestContext()
			{
				StartDate = DateTime.UtcNow;
			}
		}

		protected virtual void OnDisposing()
		{
			Disposing?.Invoke(this, EventArgs.Empty);
		}

		protected override void Dispose(bool disposing)
		{
			OnDisposing();

			if (disposing)
			{
				if (_cts != null)
				{
					_cts.Cancel();
					_cts.Dispose();
					_cts = null;
				}
			}

			base.Dispose(disposing);
		}
	}
}
