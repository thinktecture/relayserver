using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.OnPremiseConnector.IdentityModel;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal class RelayServerConnection : Connection, IRelayServerConnection
	{
		private const int _CONNECTOR_VERSION = 2;
		
		private static int _nextInstanceId;
		private static readonly Random _random = new Random();

		private readonly string _userName;
		private readonly string _password;
		private readonly TimeSpan _requestTimeout;
		private readonly IOnPremiseTargetConnectorFactory _onPremiseTargetConnectorFactory;
		private readonly ILogger _logger;
		private readonly ConcurrentDictionary<string, IOnPremiseTargetConnector> _connectors;
		private readonly HttpClient _httpClient;

		private CancellationTokenSource _cts;
		private bool _stopRequested;
		private string _accessToken;


		private readonly int _minConnectWaitTimeInSeconds;
		private readonly int _maxConnectWaitTimeInSeconds;

		public event EventHandler Disposing;

		public Uri Uri { get; }
		public TimeSpan TokenRefreshWindow { get; }
		public DateTime TokenExpiry { get; private set; } = DateTime.MaxValue;
		public int RelayServerConnectionInstanceId { get; }
		public DateTime LastHeartbeat { get; private set; } = DateTime.MinValue;
		public TimeSpan HeartbeatInterval { get; private set; }

		public RelayServerConnection(RelayServerConnectionConfig config, IOnPremiseTargetConnectorFactory onPremiseTargetConnectorFactory, ILogger logger)
			: base(new Uri(config.RelayServerUri, "/signalr").AbsoluteUri, $"cv={_CONNECTOR_VERSION}&av={config.VersionAssembly.GetName().Version}")
		{
			RelayServerConnectionInstanceId = Interlocked.Increment(ref _nextInstanceId);
			_userName = config.UserName;
			_password = config.Password;
			_requestTimeout = config.RequestTimeout;
			_minConnectWaitTimeInSeconds = config.MinConnectWaitTimeInSeconds;
			_maxConnectWaitTimeInSeconds = config.MaxConnectWaitTimeInSeconds;

			Uri = config.RelayServerUri;
			TokenRefreshWindow = config.TokenRefreshWindow;

			_onPremiseTargetConnectorFactory = onPremiseTargetConnectorFactory;
			_logger = logger;

			_connectors = new ConcurrentDictionary<string, IOnPremiseTargetConnector>(StringComparer.OrdinalIgnoreCase);
			_httpClient = new HttpClient()
			{
				Timeout = config.RequestTimeout,
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

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(baseUri, _requestTimeout);
		}

		public void RegisterOnPremiseTarget(string key, Type handlerType)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			key = RemoveTrailingSlashes(key);

			_logger?.Verbose("Registering on-premise in-proc target. handler-key={HandlerKey}, handler-type={HandlerType}", key, handlerType);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(handlerType, _requestTimeout);
		}

		public void RegisterOnPremiseTarget(string key, Func<IOnPremiseInProcHandler> handlerFactory)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (handlerFactory == null)
				throw new ArgumentNullException(nameof(handlerFactory));

			key = RemoveTrailingSlashes(key);

			_logger?.Verbose("Registering on-premise in-proc target using a handler factory. handler-key={HandlerKey}", key);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(handlerFactory, _requestTimeout);
		}

		public void RegisterOnPremiseTarget<T>(string key) where T : IOnPremiseInProcHandler, new()
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			key = RemoveTrailingSlashes(key);

			_logger?.Verbose("Registering on-premise in-proc target. handler-key={HandlerKey}, handler-type={HandlerType}", key, typeof(T).Name);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create<T>(_requestTimeout);
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
					_logger?.Verbose("Requesting authorization token. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);

					var response = await client.RequestResourceOwnerPasswordAsync(_userName, _password).ConfigureAwait(false);
					if (response.IsError)
						throw new AuthenticationException(response.HttpErrorReason ?? response.Error);

					_logger?.Verbose("Received token. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);
					return response;
				}
				catch (Exception ex)
				{
					var randomWaitTime = GetRandomWaitTime();
					_logger?.Error(ex, "Could not authenticate with RelayServer - re-trying in {RetryWaitTime} seconds. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}", randomWaitTime.TotalSeconds, Uri, RelayServerConnectionInstanceId);
					await Task.Delay(randomWaitTime, _cts.Token).ConfigureAwait(false);
				}
			}

			return null;
		}

		public async Task ConnectAsync()
		{
			_logger?.Information("Connecting to RelayServer {RelayServerUri} with connection id {RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);

			_stopRequested = false;

			if (!await TryRequestAuthorizationTokenAsync().ConfigureAwait(false))
			{
				return;
			}

			try
			{
				await Start().ConfigureAwait(false);
				_logger?.Information("Connected to RelayServer {RelayServerUri} with connection id {ConnectionId}", Uri, ConnectionId);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error while connecting to RelayServer {RelayServerUri} with connection id {RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);
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

			_logger?.Verbose("Setting access token. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}, token-expiry={TokenExpiry}", Uri, RelayServerConnectionInstanceId, TokenExpiry);
		}

		private void CheckResponseTokenForErrors(TokenResponse token)
		{
			if (token.IsHttpError)
			{
				_logger?.Warning("Could not authenticate with RelayServer. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}, status-code={ConnectionHttpStatusCode}, reason={ConnectionErrorReason}", Uri, RelayServerConnectionInstanceId, token.HttpErrorStatusCode, token.HttpErrorReason);
				throw new AuthenticationException("Could not authenticate with RelayServer: " + token.HttpErrorReason);
			}

			if (token.IsError)
			{
				_logger?.Warning("Could not authenticate with RelayServer. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}, reason={ConnectionErrorReason}", Uri, RelayServerConnectionInstanceId, token.Error);
				throw new AuthenticationException("Could not authenticate with RelayServer: " + token.Error);
			}
		}

		private void OnReconnected()
		{
			_logger?.Verbose("Connection restored. connection-id={ConnectionId}", ConnectionId);
		}

		private void OnReconnecting()
		{
			_logger?.Verbose("Connection lost. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);
		}

		protected override async void OnMessageReceived(JToken message)
		{
			IOnPremiseTargetRequestInternal request = null;
			var startDate = DateTime.UtcNow;

			var ctx = new RequestContext();
			try
			{
				_logger?.Verbose("Received message from server. connection-id={ConnectionId}, message={Message}", ConnectionId, message);

				request = message.ToObject<OnPremiseTargetRequest>();

				try
				{
					if (request.IsPingRequest)
					{
						await HandlePingRequestAsync(ctx, request).ConfigureAwait(false);
						return;
					}

					if (request.IsHeartbeatRequest)
					{
						await HandleHeartbeatRequestAsync(ctx, request).ConfigureAwait(false);
						return;
					}
				}
				finally
				{
					await AcknowledgeRequest(request).ConfigureAwait(false);
				}

				var key = request.Url.Split('/').FirstOrDefault();
				if (key != null)
				{
					if (_connectors.TryGetValue(key, out var connector))
					{
						_logger?.Verbose("Found on-premise target and sending request. request-id={RequestId}, on-premise-key={OnPremiseTargetKey}", request.RequestId, key);

						await RequestLocalTargetAsync(ctx, key, connector, request, CancellationToken.None).ConfigureAwait(false); // TODO no cancellation token here?
						return;
					}
				}

				_logger?.Verbose("No connector found for local server. request-id={RequestId}, request-url={RequestUrl}", request.RequestId, request.Url);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during handling received message. connection-id={ConnectionId}, message={Message}", ConnectionId, message);
			}
			finally
			{
				if (!ctx.IsRelayServerNotified && request != null)
				{
					_logger?.Warning("Unhandled request. connection-id={ConnectionId}, message={message}", ConnectionId, message);

					var response = new OnPremiseTargetResponse()
					{
						RequestStarted = startDate,
						RequestFinished = DateTime.UtcNow,
						StatusCode = HttpStatusCode.NotFound,
						OriginId = request.OriginId,
						RequestId = request.RequestId,
						HttpHeaders = new Dictionary<string, string>(),
					};

					try
					{
						// No cancellation token here, to not cancel sending of an already fetched response
						await PostResponseAsync(ctx, response, CancellationToken.None).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						_logger?.Error(ex, "Error during posting to relay. connection-id={ConnectionId}", ConnectionId);
					}
				}
			}
		}

		private async Task AcknowledgeRequest(IOnPremiseTargetRequest request)
		{
			if (request.AcknowledgmentMode == AcknowledgmentMode.Auto)
			{
				_logger?.Debug("Automatically acknowledged by RelayServer. request-id={RequestId}", request.RequestId);
				return;
			}

			if (String.IsNullOrEmpty(request.AcknowledgeId))
			{
				_logger?.Debug("No acknowledgment possible. request-id={RequestId}, acknowledment-mode={AcknowledgmentMode}", request.RequestId, request.AcknowledgmentMode);
				return;
			}

			switch (request.AcknowledgmentMode)
			{
				case AcknowledgmentMode.Default:
					_logger?.Debug("Sending acknowledge to RelayServer. request-id={RequestId}, origin-id={OriginId}, acknowledge-id={AcknowledgeId}", request.RequestId, request.AcknowledgeOriginId, request.AcknowledgeId);
					await GetToRelay($"/request/acknowledge?oid={request.AcknowledgeOriginId}&aid={request.AcknowledgeId}&cid={ConnectionId}", CancellationToken.None).ConfigureAwait(false); // TODO no cancellation token here?
					break;

				case AcknowledgmentMode.Manual:
					_logger?.Debug("Manual acknowledgment needed. request-id={RequestId}, origin-id={OriginId}, acknowledge-id={AcknowledgeId}", request.RequestId, request.AcknowledgeOriginId, request.AcknowledgeId);
					break;

				default:
					_logger?.Warning("Unknown acknowledgment mode found. request-id={RequestId}, acknowledment-mode={AcknowledgmentMode}, acknowledge-id={AcknowledgeId}", request.RequestId, request.AcknowledgmentMode, request.AcknowledgeId);
					break;
			}
		}

		private async Task HandlePingRequestAsync(RequestContext ctx, IOnPremiseTargetRequest request)
		{
			_logger?.Debug("Received ping from RelayServer. request-id={RequestId}", request.RequestId);

			var response = new OnPremiseTargetResponse()
			{
				RequestStarted = DateTime.UtcNow,
				RequestFinished = DateTime.UtcNow,
				StatusCode = HttpStatusCode.OK,
				OriginId = request.OriginId,
				RequestId = request.RequestId,
			};

			// No cancellation token here, to not cancel sending of an already fetched response
			await PostResponseAsync(ctx, response, CancellationToken.None).ConfigureAwait(false);
		}

		private async Task HandleHeartbeatRequestAsync(RequestContext ctx, IOnPremiseTargetRequest request)
		{
			_logger?.Debug("Received heartbeat from RelayServer. request-id={RequestId}", request.RequestId);

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

			var response = new OnPremiseTargetResponse()
			{
				RequestStarted = DateTime.UtcNow,
				RequestFinished = DateTime.UtcNow,
				StatusCode = HttpStatusCode.OK,
				OriginId = request.OriginId,
				RequestId = request.RequestId,
			};
			// No cancellation token here, to not cancel sending of an already fetched response
			await PostResponseAsync(ctx, response, CancellationToken.None).ConfigureAwait(false);
		}

		public void Reconnect()
		{
			_logger?.Debug("Forcing reconnect. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);

			Disconnect();

			Task.Delay(GetRandomWaitTime()).ContinueWith(_ => ConnectAsync()).ConfigureAwait(false);
		}

		public void Disconnect()
		{
			_logger?.Information("Disconnecting from RelayServer {RelayServerUri} with connection instance id {RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);

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
			_logger?.Debug("Relaying request to local target. request-id={RequestId}, request-url={RequestUrl}", request.RequestId, request.Url);

			var url = (request.Url.Length > key.Length) ? request.Url.Substring(key.Length + 1) : String.Empty;

			if (request.Body != null)
			{
				if (request.Body.Length == 0)
				{
					// a length of 0 indicates that there is a larger body available on the server
					_logger?.Verbose("Requesting body. request-id={RequestId}", request.RequestId);
					// request the body from the RelayServer (because SignalR cannot handle large messages)
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

				_logger?.Debug("Sending response. request-id={RequestId}", request.RequestId);

				try
				{
					// transfer the result to the RelayServer (need POST here, because SignalR does not handle large messages)
					await PostResponseAsync(ctx, response, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger?.Error(ex, "Error during posting to relay. request-id={RequestId}", request.RequestId);
				}
			}
		}

		private async Task PostResponseAsync(RequestContext ctx, IOnPremiseTargetResponse response, CancellationToken cancellationToken)
		{
			await PostToRelay("/forward", headers => headers.Add("X-TTRELAY-METADATA", JsonConvert.SerializeObject(response)), new StreamContent(response.Stream ?? Stream.Null, 0x10000), cancellationToken).ConfigureAwait(false);
			ctx.IsRelayServerNotified = true;
		}

		protected override void OnClosed()
		{
			_logger?.Information("Connection closed to RelayServer {RelayServerUri} with connection instance id {RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);

			base.OnClosed();

			if (!_stopRequested)
			{
				var randomWaitTime = GetRandomWaitTime();
				_logger?.Debug("Connection closed. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}, reconnect-wait-time={ReconnectWaitTime}", Uri, RelayServerConnectionInstanceId, randomWaitTime.TotalSeconds);
				Task.Delay(randomWaitTime).ContinueWith(_ => ConnectAsync()).ConfigureAwait(false);
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

			return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
		}

		private TimeSpan GetRandomWaitTime()
		{
			var waitingSeconds = _random.Next(_minConnectWaitTimeInSeconds, _maxConnectWaitTimeInSeconds);
			_logger.Debug($"waiting {waitingSeconds}s from now...");
			return TimeSpan.FromSeconds(waitingSeconds);
		}

		private class RequestContext
		{
			public bool IsRelayServerNotified { get; set; }
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
