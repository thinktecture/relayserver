using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
		private readonly string _userName;
		private readonly string _password;
		private readonly Uri _relayServer;
		private readonly int _requestTimeout;
		private readonly IOnPremiseTargetConnectorFactory _onPremiseTargetConnectorFactory;
		private readonly ILogger _logger;
		private readonly ConcurrentDictionary<string, IOnPremiseTargetConnector> _connectors;
		private readonly HttpClient _httpClient;
		private readonly int _id;
		private CancellationTokenSource _cts;
		private DateTime _lastHeartbeat;
		private string _accessToken;
		private bool _stopRequested;

		private static int _nextId;
		private static readonly Random _random = new Random();

		private const int _MIN_WAIT_TIME_IN_SECONDS = 2;
		private const int _MAX_WAIT_TIME_IN_SECONDS = 30;

		public RelayServerConnection(string userName, string password, Uri relayServer, int requestTimeout, IOnPremiseTargetConnectorFactory onPremiseTargetConnectorFactory, ILogger logger)
			: base(new Uri(relayServer, "/signalr").AbsoluteUri, "version=2")
		{
			_id = Interlocked.Increment(ref _nextId);
			_userName = userName;
			_password = password;
			_relayServer = relayServer;
			_requestTimeout = requestTimeout;
			_onPremiseTargetConnectorFactory = onPremiseTargetConnectorFactory;
			_logger = logger;
			_connectors = new ConcurrentDictionary<string, IOnPremiseTargetConnector>(StringComparer.OrdinalIgnoreCase);
			_httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(requestTimeout) };
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

			_logger?.Verbose("Registering on-premise web target. key={handler-key}, base-uri={base-url}", key, baseUri);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(baseUri, _requestTimeout);
		}

		public void RegisterOnPremiseTarget(string key, Type handlerType)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			key = RemoveTrailingSlashes(key);

			_logger?.Verbose("Registering on-premise in-proc target. key={handler-key}, type={handler-type}", key, handlerType);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(handlerType, _requestTimeout);
		}

		public void RegisterOnPremiseTarget(string key, Func<IOnPremiseInProcHandler> handlerFactory)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (handlerFactory == null)
				throw new ArgumentNullException(nameof(handlerFactory));

			key = RemoveTrailingSlashes(key);

			_logger?.Verbose("Registering on-premise in-proc target using a handler factory. key={handler-key}", key);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(handlerFactory, _requestTimeout);
		}

		public void RegisterOnPremiseTarget<T>(string key) where T : IOnPremiseInProcHandler, new()
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			key = RemoveTrailingSlashes(key);

			_logger?.Verbose("Registering on-premise in-proc target. key={handler-key}, type={handler-type}", key, typeof(T));

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
			var client = new OAuth2Client(new Uri(_relayServer, "/token"));

			while (!_stopRequested)
			{
				try
				{
					_logger?.Verbose("Requesting authorization token. relay-server={relay-server}, id={relay-server-id}", _relayServer, _id);

					var response = await client.RequestResourceOwnerPasswordAsync(_userName, _password).ConfigureAwait(false);

					_logger?.Verbose("Received token. relay-server={relay-server}, id={relay-server-id}", _relayServer, _id);
					return response;
				}
				catch
				{
					var randomWaitTime = GetRandomWaitTime();
					_logger?.Information("Could not authenticate with relay server - re-trying in {retry-seconds} seconds", randomWaitTime.TotalSeconds);
					await Task.Delay(randomWaitTime, _cts.Token).ConfigureAwait(false);
				}
			}

			return null;
		}

		public async Task ConnectAsync()
		{
			_logger?.Information("Connecting to relay server {relay-server-id}", _id);

			if (!await TryRequestAuthorizationTokenAsync().ConfigureAwait(false))
			{
				return;
			}

			try
			{
				await Start().ConfigureAwait(false);
				_logger?.Information("Connected to relay server {relay-server-id}", _id);
			}
			catch
			{
				_logger?.Information("Error while connecting to relay server {relay-server-id}", _id);
			}
		}

		private async Task<bool> TryRequestAuthorizationTokenAsync()
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
			_httpClient.SetBearerToken(_accessToken);

			Headers["Authorization"] = $"{tokenResponse.TokenType} {_accessToken}";

			_logger?.Verbose("Setting access token. access-token={access-token}", _accessToken);
		}

		private void CheckResponseTokenForErrors(TokenResponse token)
		{
			if (token.IsHttpError)
			{
				_logger?.Warning("Could not authenticate with relay server {relay-server-id}: status-code={connection-http-status-code}, reason={connection-reason}", _id, token.HttpErrorStatusCode, token.HttpErrorReason);
				throw new Exception("Could not authenticate with relay server: " + token.HttpErrorReason);
			}

			if (token.IsError)
			{
				_logger?.Warning("Could not authenticate with relay server {relay-server-id}. reason={connection-reason}", _id, token.Error);
				throw new Exception("Could not authenticate with relay server: " + token.Error);
			}
		}

		private void OnReconnected()
		{
			_logger?.Verbose("Connection restored. relay-server={relay-server}", _relayServer);
		}

		private void OnReconnecting()
		{
			_logger?.Verbose("Connection lost - trying to reconnect. relay-server={relay-server}", _relayServer);
		}

		protected override async void OnMessageReceived(JToken message)
		{
			var ctx = new RequestContext();
			OnPremiseTargetRequest request = null;

			try
			{
				_logger?.Verbose("Received message from server. message={message}", message);

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
					await Send(request.AcknowledgeId).ConfigureAwait(false);
				}

				var key = request.Url.Split('/').FirstOrDefault();
				if (key != null)
				{
					if (_connectors.TryGetValue(key, out var connector))
					{
						_logger?.Verbose("Found on-premise target. key={key}", key);

						await RequestLocalTargetAsync(ctx, key, connector, request, CancellationToken.None).ConfigureAwait(false);
						return;
					}
				}

				_logger?.Verbose("No connector found for local server. request-id={request-id}, url={request-url}", request.RequestId, request.Url);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during handling received message. Message = {message}", message);
			}
			finally
			{
				if (!ctx.IsRelayServerNotified && request != null)
				{
					_logger?.Verbose("Unhandled request. message={message}", message);

					var response = new OnPremiseTargetResponse
					{
						RequestStarted = ctx.StartDate,
						RequestFinished = DateTime.UtcNow,
						StatusCode = HttpStatusCode.NotFound,
						OriginId = request.OriginId,
						RequestId = request.RequestId,
					};

					try
					{
						await PostToRelayAsync(ctx, response, CancellationToken.None).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						_logger?.Error(ex, "Error during posting to relay");
					}
				}
			}
		}

		private async Task HandlePingRequestAsync(RequestContext ctx, IOnPremiseTargetRequest request)
		{
			_logger?.Debug("Received ping from relay server {relay-server-id}", _id);

			var resp = new OnPremiseTargetResponse
			{
				RequestStarted = DateTime.UtcNow,
				RequestFinished = DateTime.UtcNow,
				StatusCode = HttpStatusCode.OK,
				OriginId = request.OriginId,
				RequestId = request.RequestId,
			};

			await PostToRelayAsync(ctx, resp, CancellationToken.None).ConfigureAwait(false);
		}

		private void HandleHeartbeatRequest(RequestContext ctx, IOnPremiseTargetRequest request)
		{
			_logger?.Debug("Received heartbeat from relay server {relay-server-id}", _id);

			if (_lastHeartbeat == DateTime.MinValue)
			{
				request.HttpHeaders.TryGetValue("X-TTRELAY-HEARTBEATINTERVAL", out var heartbeatHeaderValue);
				if (Int32.TryParse(heartbeatHeaderValue, out var heartbeatInterval))
				{
					_logger?.Information("Heartbeat interval set to {heartbeat-interval} seconds", heartbeatInterval);
					StartHeartbeatCheckLoop(TimeSpan.FromSeconds(heartbeatInterval), _cts.Token);
				}
			}

			ctx.IsRelayServerNotified = true;
		}

		private void StartHeartbeatCheckLoop(TimeSpan heartbeatInterval, CancellationToken token)
		{
			Task.Run(async () =>
			{
				var intervalWithTolerance = heartbeatInterval.Add(TimeSpan.FromSeconds(2));

				while (!token.IsCancellationRequested)
				{
					if (_lastHeartbeat != DateTime.MinValue && _lastHeartbeat != DateTime.MaxValue)
					{
						_logger?.Verbose("Checking last heartbeat time. last-heartbeat={last-heartbeat}", _lastHeartbeat);

						if (_lastHeartbeat <= DateTime.UtcNow.Add(-intervalWithTolerance))
						{
							_logger?.Warning("Last heartbeat was at {last-heartbeat} and out of an interval of {heartbeat-interval} secconds", _lastHeartbeat, heartbeatInterval.TotalSeconds);

							_lastHeartbeat = DateTime.MaxValue;

							if (!_stopRequested)
							{
								ForceReconnect();
							}
						}
					}

					await Task.Delay(heartbeatInterval, token).ConfigureAwait(false);
				}
			}, token).ConfigureAwait(false);
		}

		private void ForceReconnect()
		{
			_logger?.Debug("Forcing reconnect");

			Disconnect();

			Task.Delay(1500).ContinueWith(_ =>
			{
				_stopRequested = false;
				return ConnectAsync().ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

		public void Disconnect()
		{
			_logger?.Information("Disconnecting from relay server {relay-server-id}", _id);

			_stopRequested = true;
			Stop();
		}

		protected override void Dispose(bool disposing)
		{
			if (_cts != null)
			{
				_cts.Cancel();
				_cts.Dispose();
				_cts = null;
			}

			base.Dispose(disposing);
		}

		public List<string> GetOnPremiseTargetKeys()
		{
			return _connectors.Keys.ToList();
		}

		private async Task RequestLocalTargetAsync(RequestContext ctx, string key, IOnPremiseTargetConnector connector, OnPremiseTargetRequest request, CancellationToken cancellationToken)
		{
			_logger?.Debug("Requesting local server {request-url} for request {request-id}", request.Url, request.RequestId);

			var url = (request.Url.Length > key.Length) ? request.Url.Substring(key.Length + 1) : String.Empty;

			if (request.Body != null)
			{
				if (request.Body.Length == 0)
				{
					// a length of 0 indicates that there is a larger body available on the server
					_logger?.Verbose("Requesting body from relay server. relay-server={relay-server}, request-id={request-id}", _relayServer, request.RequestId);
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

				_logger?.Debug("Sending response from {request-url} to relay server", request.Url);

				try
				{
					// transfer the result to the relay server (need POST here, because SignalR does not handle large messages)
					await PostToRelayAsync(ctx, response, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger?.Error(ex, "Error during posting to relay");
				}
			}
		}

		private async Task PostToRelayAsync(RequestContext ctx, IOnPremiseTargetResponse response, CancellationToken cancellationToken)
		{
			await PostToRelay("/forward", headers => headers.Add("X-TTRELAY-METADATA", JsonConvert.SerializeObject(response)), new StreamContent(response.Stream), cancellationToken).ConfigureAwait(false);
			ctx.IsRelayServerNotified = true;
		}

		protected override void OnClosed()
		{
			_logger?.Information("Connection closed {relay-server-id}", _id);

			base.OnClosed();

			if (!_stopRequested)
			{
				var randomWaitTime = GetRandomWaitTime();
				_logger?.Debug("Reconnecting in {reconnect-wait-time} seconds", randomWaitTime.TotalSeconds);
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

		private Task<HttpResponseMessage> SendToRelayAsync(string relativeUrl, HttpMethod httpMethod, Action<HttpRequestHeaders> setHeaders, HttpContent content, CancellationToken cancellationToken)
		{
			if (String.IsNullOrWhiteSpace(relativeUrl))
				throw new ArgumentException("Relative url cannot be null or empty.");

			if (!relativeUrl.StartsWith("/"))
				relativeUrl = "/" + relativeUrl;

			var url = new Uri(_relayServer, relativeUrl);

			var request = new HttpRequestMessage(httpMethod, url);

			setHeaders?.Invoke(request.Headers);
			request.Content = content;

			return _httpClient.SendAsync(request, cancellationToken);
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
	}
}
