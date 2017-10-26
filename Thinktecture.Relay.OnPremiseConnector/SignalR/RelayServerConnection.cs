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
using NLog;
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

		public void RegisterOnPremiseTarget(string key, Uri baseUri, bool ignoreSslErrors)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (baseUri == null)
				throw new ArgumentNullException(nameof(baseUri));

			key = RemoveTrailingSlashes(key);

			_logger?.Debug("Registering on-premise web target");
			_logger?.Trace("Registering on-premise web target. key={0}, baseUri={1}, ignoreSslErrors={2}", key, baseUri, ignoreSslErrors);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(baseUri, _requestTimeout, ignoreSslErrors);
		}

		public void RegisterOnPremiseTarget(string key, Type handlerType)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			key = RemoveTrailingSlashes(key);

			_logger?.Debug("Registering on-premise in-proc target");
			_logger?.Trace("Registering on-premise in-proc target. key={0}, type={1}", key, handlerType);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(handlerType, _requestTimeout);
		}

		public void RegisterOnPremiseTarget(string key, Func<IOnPremiseInProcHandler> handlerFactory)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (handlerFactory == null)
				throw new ArgumentNullException(nameof(handlerFactory));

			key = RemoveTrailingSlashes(key);

			_logger?.Debug("Registering on-premise in-proc target");
			_logger?.Trace("Registering on-premise in-proc target using a handler factory. key={0}", key);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(handlerFactory, _requestTimeout);
		}

		public void RegisterOnPremiseTarget<T>(string key) where T : IOnPremiseInProcHandler, new()
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			key = RemoveTrailingSlashes(key);

			_logger?.Debug("Registering on-premise in-proc target");
			_logger?.Trace("Registering on-premise in-proc target. key={0}, type={1}", key, typeof(T));

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
					_logger?.Debug("Requesting authorization token");
					_logger?.Trace("Requesting authorization token. relay-server={0}, id={1}", _relayServer, _id);

					var response = await client.RequestResourceOwnerPasswordAsync(_userName, _password).ConfigureAwait(false);

					_logger?.Trace("Received token. relay-server={0}, id={1}", _relayServer, _id);
					return response;
				}
				catch (Exception ex)
				{
					var randomWaitTime = GetRandomWaitTime();
					_logger?.Info("Could not authenticate with relay server - re-trying in {0} seconds", randomWaitTime.TotalSeconds);
					_logger?.Trace(ex, "Could not authenticate with relay server - re-trying in {0} seconds", randomWaitTime.TotalSeconds);
					await Task.Delay(randomWaitTime).ConfigureAwait(false);
				}
			}

			return null;
		}

		public async Task ConnectAsync()
		{
			_logger?.Info("Connecting to relay server #{0}", _id);

			if (!await TryRequestAuthorizationTokenAsync().ConfigureAwait(false))
			{
				return;
			}

			try
			{
				await Start().ConfigureAwait(false);
				_logger?.Info("Connected to relay server #{0}", _id);
			}
			catch (Exception ex)
			{
				_logger?.Info("Error while connecting to relay server #{0}", _id);
				_logger?.Trace(ex, "Error while connecting to relay server #{0}", _id);
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

			_logger?.Trace("Setting bearer token. access-token={0}", _accessToken);
		}

		private void CheckResponseTokenForErrors(TokenResponse token)
		{
			if (token.IsHttpError)
			{
				_logger?.Warn("Could not authenticate with relay server.");
				_logger?.Trace("Could not authenticate with relay server: status-code={0}, reason={1}", token.HttpErrorStatusCode, token.HttpErrorReason);
				throw new Exception("Could not authenticate with relay server: " + token.HttpErrorReason);
			}

			if (token.IsError)
			{
				_logger?.Warn("Could not authenticate with relay server");
				_logger?.Trace("Could not authenticate with relay server. reason={0}", token.Error);
				throw new Exception("Could not authenticate with relay server: " + token.Error);
			}
		}

		private void OnReconnected()
		{
			_logger?.Debug("Connection restored");
			_logger?.Trace("Connection restored. relay-server={0}", _relayServer);
		}

		private void OnReconnecting()
		{
			_logger?.Debug("Connection lost - trying to reconnect");
			_logger?.Trace("Connection lost - trying to reconnect. relay-server={0}", _relayServer);
		}

		protected override void OnMessageReceived(JToken message)
		{
			base.OnMessageReceived(message);

			OnReceivedAsync(message).ConfigureAwait(false);
		}

		private async Task OnReceivedAsync(JToken message)
		{
			var ctx = new RequestContext();
			OnPremiseTargetRequest request = null;

			try
			{
				_logger?.Debug("Received message from server");
				_logger?.Trace("Received message from server. message={0}", message);

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
						_logger?.Trace("Found on-premise target. key={0}", key);

						await RequestLocalTargetAsync(ctx, key, connector, request, CancellationToken.None).ConfigureAwait(false);
						return;
					}
				}

				_logger?.Trace("No connector found for local server. request-id={0}, url={1}", request.RequestId, request.Url);
				_logger?.Debug("No connector found for local server {0} of request {1}", request.Url, request.RequestId);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during handling received message.");
			}
			finally
			{
				if (!ctx.IsRelayServerNotified && request != null)
				{
					_logger?.Debug("Unhandled request {0}", message);

					var response = new OnPremiseTargetResponse
					{
						RequestStarted = ctx.StartDate,
						RequestFinished = DateTime.UtcNow,
						StatusCode = HttpStatusCode.NotFound,
						OriginId = request.OriginId,
						RequestId = request.RequestId,
					};

					await PostToRelayAsync(ctx, response, CancellationToken.None).ConfigureAwait(false);
				}
			}
		}

		private async Task HandlePingRequestAsync(RequestContext ctx, IOnPremiseTargetRequest request)
		{
			_logger?.Info("Received ping from relay server #{0}", _id);

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
			_logger?.Debug("Received heartbeat from relay server #{0}", _id);

			if (_lastHeartbeat == DateTime.MinValue)
			{
				request.HttpHeaders.TryGetValue("X-TTRELAY-HEARTBEATINTERVAL", out var heartbeatHeaderValue);
				if (Int32.TryParse(heartbeatHeaderValue, out var heartbeatInterval))
				{
					_logger?.Info("Heartbeat interval set to {0}s, starting checking of heartbeat.", heartbeatInterval);
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
						_logger?.Trace("Checking last heartbeat time. Interval: {0}s, last heartbeat: {1}", heartbeatInterval, _lastHeartbeat);

						if (_lastHeartbeat <= DateTime.UtcNow.Add(-intervalWithTolerance))
						{
							_logger?.Warn("Last heartbeat was at {0} and out of interval of {1}s. Forcing reconnect.", _lastHeartbeat, heartbeatInterval);

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
			Disconnect();

			Task.Delay(1500).ContinueWith(_ =>
			{
				_stopRequested = false;
				return ConnectAsync().ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

		public void Disconnect()
		{
			_logger?.Info("Disconnecting from relay server #{0}", _id);

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
			_logger?.Debug("Requesting local server {0} for request id {1}", request.Url, request.RequestId);

			var url = (request.Url.Length > key.Length) ? request.Url.Substring(key.Length + 1) : String.Empty;

			if (request.Body != null)
			{
				if (request.Body.Length == 0)
				{
					_logger?.Trace("Requesting body from relay server. relay-server={0}, request-id={1}", _relayServer, request.RequestId);
					// request the body from the relay server (because SignalR cannot handle large messages)
					var webResponse = await GetToRelay("/request/" + request.RequestId, cancellationToken).ConfigureAwait(false);
					request.Stream = await webResponse.Content.ReadAsStreamAsync().ConfigureAwait(false); // this stream should not be disposed (owned by the Framework)
				}
				else
				{
					request.Stream = new MemoryStream(request.Body);
				}
			}
			else
			{
				request.Stream = Stream.Null;
			}

			var response = await connector.GetResponseFromLocalTargetAsync(url, request, RelayedRequestHeader).ConfigureAwait(false);
			if (response.Stream == null)
			{
				response.Stream = Stream.Null;
			}

			_logger?.Debug("Sending response from {0} to relay server", request.Url);

			try
			{
				// transfer the result to the relay server (need POST here, because SignalR does not handle large messages)
				await PostToRelayAsync(ctx, response, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.Debug(ex, "Error while posting to relay server");
				_logger?.Error("Error communicating with relay server. Aborting response...");
			}
		}

		private async Task PostToRelayAsync(RequestContext ctx, IOnPremiseTargetResponse response, CancellationToken cancellationToken)
		{
			await PostToRelay("/forward", headers => headers.Add("X-TTRELAY-METADATA", JsonConvert.SerializeObject(response)), new StreamContent(response.Stream), cancellationToken).ConfigureAwait(false);
			ctx.IsRelayServerNotified = true;
		}

		protected override void OnClosed()
		{
			_logger?.Info("Connection closed #{0}", _id);

			base.OnClosed();

			if (!_stopRequested)
			{
				var randomWaitTime = GetRandomWaitTime();
				_logger?.Debug("Reconnecting in {0} seconds", randomWaitTime.TotalSeconds);
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
