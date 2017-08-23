using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
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
		private readonly int _maxRetries;
		private readonly IOnPremiseTargetConnectorFactory _onPremiseTargetConnectorFactory;
		private readonly ILogger _logger;
		private readonly ConcurrentDictionary<string, IOnPremiseTargetConnector> _connectors;
		private readonly HttpClient _httpClient;
		private bool _eventsHooked;
		private readonly int _id;
		private string _accessToken;
		private bool _stopRequested;

		private static int _nextId;
		private static readonly Random _random = new Random();

		private readonly object _lastHeartbeatLock;
		private DateTime _lastHeartbeat = DateTime.MinValue;
		private CancellationTokenSource _heartbeatCancellationTokenSource;

		private const int _MIN_WAIT_TIME_IN_SECONDS = 2;
		private const int _MAX_WAIT_TIME_IN_SECONDS = 30;

		public RelayServerConnection(string userName, string password, Uri relayServer, int requestTimeout, int maxRetries, IOnPremiseTargetConnectorFactory onPremiseTargetConnectorFactory, ILogger logger)
			: base(new Uri(relayServer, "/signalr").AbsoluteUri, "version=2")
		{
			_id = Interlocked.Increment(ref _nextId);
			_userName = userName;
			_password = password;
			_relayServer = relayServer;
			_requestTimeout = requestTimeout;
			_maxRetries = maxRetries;
			_onPremiseTargetConnectorFactory = onPremiseTargetConnectorFactory;
			_logger = logger;
			_connectors = new ConcurrentDictionary<string, IOnPremiseTargetConnector>(StringComparer.OrdinalIgnoreCase);
			_httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(requestTimeout) };
			_lastHeartbeatLock = new object();
		}

		public string RelayedRequestHeader { get; set; }

		public void RegisterOnPremiseTarget(string key, Uri baseUri)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (baseUri == null)
				throw new ArgumentNullException(nameof(baseUri));

			key = RemoveTrailingSlashes(key);

			_logger.Trace("Registering on-premise web target. key={0}, baseUri={1}", key, baseUri);
			_logger.Debug("Registering on-premise web target");

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(baseUri, _requestTimeout);
		}

		public void RegisterOnPremiseTarget(string key, Type handlerType)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			key = RemoveTrailingSlashes(key);

			_logger.Trace("Registering on-premise in-proc target. key={0}, type={1}", key, handlerType);
			_logger.Debug("Registering on-premise in-proc target");

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(handlerType, _requestTimeout);
		}

		public void RegisterOnPremiseTarget(string key, Func<IOnPremiseInProcHandler> handlerFactory)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (handlerFactory == null)
				throw new ArgumentNullException(nameof(handlerFactory));

			key = RemoveTrailingSlashes(key);

			_logger.Trace("Registering on-premise in-proc target using a handler factory. key={0}", key);
			_logger.Debug("Registering on-premise in-proc target");

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(handlerFactory, _requestTimeout);
		}

		public void RegisterOnPremiseTarget<T>(string key)
			where T : IOnPremiseInProcHandler, new()
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			key = RemoveTrailingSlashes(key);

			_logger.Trace("Registering on-premise in-proc target. key={0}, type={1}", key, typeof(T));
			_logger.Debug("Registering on-premise in-proc target");

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

			while (key.EndsWith("/"))
			{
				key = key.Substring(0, key.Length - 1);
			}

			_connectors.TryRemove(key, out var old);
		}

		private async Task<TokenResponse> GetAuthorizationTokenAsync()
		{
			var client = new OAuth2Client(new Uri(_relayServer, "/token"));

			while (!_stopRequested)
			{
				try
				{
					_logger.Trace("Requesting authorization token. relay-server={0}, id={1}", _relayServer, _id);
					_logger.Debug("Requesting authorization token");

					var response = await client.RequestResourceOwnerPasswordAsync(_userName, _password);

					_logger.Trace("Received token. relay-server={0}, id={1}", _relayServer, _id);
					return response;
				}
				catch (Exception ex)
				{
					var randomWaitTime = GetRandomWaitTime();
                    _logger.Info("Could not authenticate with relay server - re-trying in {0} seconds", randomWaitTime.TotalSeconds);
                    _logger.Trace(ex, "Could not authenticate with relay server - re-trying in {0} seconds", randomWaitTime.TotalSeconds);
					Thread.Sleep(randomWaitTime);
				}
			}

			return null;
		}

		public async Task Connect()
		{
			_logger.Info("Connecting to relay server #{0}", _id);

			if (!await TryRequestAuthorizationTokenAsync())
				return;

			if (!_eventsHooked)
			{
				_eventsHooked = true;

				Reconnecting += OnReconnecting;
				Received += OnReceivedAsync;
				Reconnected += OnReconnected;
			}

			try
			{
				await Start();
				_logger.Info("Connected to relay server #{0}", _id);
			}
			catch (Exception ex)
			{
                _logger.Info("Error while connecting to relay server #{0}", _id);
                _logger.Trace(ex, "Error while connecting to relay server #{0}", _id);
			}
		}

		private async Task<bool> TryRequestAuthorizationTokenAsync()
		{
			var tokenResponse = await GetAuthorizationTokenAsync();

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

			_logger.Trace("Setting bearer token. access-token={0}", _accessToken);
		}

		private void CheckResponseTokenForErrors(TokenResponse token)
		{
			if (token.IsHttpError)
			{
				_logger.Trace("Could not authenticate with relay server: status-code={0}, reason={1}", token.HttpErrorStatusCode, token.HttpErrorReason);
				_logger.Warn("Could not authenticate with relay server.");
				throw new Exception("Could not authenticate with relay server: " + token.HttpErrorReason);
			}

			if (token.IsError)
			{
				_logger.Trace("Could not authenticate with relay server. reason={0}", token.Error);
				_logger.Warn("Could not authenticate with relay server");
				throw new Exception("Could not authenticate with relay server: " + token.Error);
			}
		}

		private void OnReconnected()
		{
			_logger.Trace("Connection restored. relay-server={0}", _relayServer);
			_logger.Debug("Connection restored");
		}

		private void OnReconnecting()
		{
			_logger.Trace("Connection lost - trying to reconnect. relay-server={0}", _relayServer);
			_logger.Debug("Connection lost - trying to reconnect");
		}

		private async void OnReceivedAsync(string data)
		{
			var ctx = new RequestContext();
			OnPremiseTargetRequest request = null;

			try
			{
				_logger.Trace("Received message from server. data={0}", data);
				_logger.Debug("Received message from server");

				// receive a client request from relay server
				request = JsonConvert.DeserializeObject<OnPremiseTargetRequest>(data);

				if (request.HttpMethod == "PING")
				{
					await HandlePingRequestAsync(ctx, request);
					return;
				}

				if (request.HttpMethod == "INIT_HEARTBEAT")
				{
					await HandleInitHeartbeatRequestAsync(request);
					return;
				}

				if (request.HttpMethod == "HEARTBEAT")
				{
					await HandleHeartbeatRequestAsync(request);
					return;
				}

				await AcknowledgeAsync(request);

				var key = request.Url.Split('/').FirstOrDefault();
				if (key != null)
				{
					if (_connectors.TryGetValue(key, out var connector))
					{
						_logger.Trace("Found on-premise target. key={0}", key);

						if (RelayedRequestHeader != null)
						{
							request.HttpHeaders[RelayedRequestHeader] = "true";
						}

						await RequestOnPremiseTargetAsync(ctx, key, connector, request, CancellationToken.None);
						return;
					}
				}

				_logger.Trace("No connector found for local server. request-id={0}, url={1}", request.RequestId, request.Url);
				_logger.Debug("No connector found for local server {0} of request {1}", request.Url, request.RequestId);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error during handling received message.");
			}
			finally
			{
				if (!ctx.IsRelayServerNotified && request != null)
				{
					_logger.Debug("Unhandled request #{0}", data);

					await PostToRelayAsync(ctx, () => new StringContent(JsonConvert.SerializeObject(new OnPremiseTargetResponse
					{
						RequestStarted = ctx.StartDate,
						RequestFinished = DateTime.UtcNow,
						StatusCode = HttpStatusCode.NotFound,
						OriginId = request.OriginId,
						RequestId = request.RequestId,
					}), Encoding.UTF8, "application/json"), CancellationToken.None);
				}
			}
		}

		private async Task AcknowledgeAsync(IOnPremiseTargetRequest request)
		{
			// older servers do not send acknowledge id, so we need to check for it not being null
			if (!String.IsNullOrEmpty(request.AcknowledgeId))
				await Send(request.AcknowledgeId);
		}

		private async Task HandlePingRequestAsync(RequestContext ctx, IOnPremiseTargetRequest request)
		{
			_logger.Info("Received ping from relay server #{0}", _id);

			await PostToRelayAsync(ctx, () => new StringContent(JsonConvert.SerializeObject(new OnPremiseTargetResponse
			{
				RequestStarted = DateTime.UtcNow,
				RequestFinished = DateTime.UtcNow,
				StatusCode = HttpStatusCode.OK,
				OriginId = request.OriginId,
				RequestId = request.RequestId,
			}), Encoding.UTF8, "application/json"), CancellationToken.None);

			await AcknowledgeAsync(request);
		}

		private async Task HandleInitHeartbeatRequestAsync(IOnPremiseTargetRequest request)
		{
			_logger.Debug($"{nameof(RelayServerConnection)}: Received initialize heartbeat from relay server #{{0}}", _id);

			_lastHeartbeat = DateTime.UtcNow;
			request.HttpHeaders.TryGetValue("X-TTRELAY-HEARTBEATINTERVAL", out var heartbeatHeaderValue);

			if (Int32.TryParse(heartbeatHeaderValue, out var heartbeatInterval))
			{
				_logger.Info("Heartbeat interval set to {0}s, starting checking of heartbeat.", heartbeatInterval);
				CancellationToken token;

				lock (_lastHeartbeatLock)
				{
					// cancel existing heartbeat if we already have one running
					TryDisposeCurrentCancellationTokenSource();
					_heartbeatCancellationTokenSource = new CancellationTokenSource();

					token = _heartbeatCancellationTokenSource.Token;
				}

#pragma warning disable 4014
				// Justification: Heartbeat checking is intended to be done in background and not awaited
				CheckHeartbeatPeriodicallyAsync(TimeSpan.FromSeconds(heartbeatInterval), token);
#pragma warning restore 4014

			}
			else
			{
				_logger.Warn("Heartbeat interval is not a valid integer: {0}", heartbeatHeaderValue);

				lock (_lastHeartbeatLock)
				{
					TryDisposeCurrentCancellationTokenSource();
				}
			}

			await AcknowledgeAsync(request);
		}

		private async Task HandleHeartbeatRequestAsync(IOnPremiseTargetRequest request)
		{
			_logger.Debug("Received heartbeat from relay server #{0}", _id);

			_lastHeartbeat = DateTime.UtcNow;

			await AcknowledgeAsync(request);
		}

		private async Task CheckHeartbeatPeriodicallyAsync(TimeSpan heartbeatInterval, CancellationToken token)
		{
			try
			{
				await Task.Run(async () =>
				{
					var intervalWithTolerance = heartbeatInterval.Add(TimeSpan.FromSeconds(2));

					while (!token.IsCancellationRequested)
					{
						_logger.Trace("Checking last heartbeat time. Interval: {0}s, last heartbeat: {1}", heartbeatInterval, _lastHeartbeat);

						if (_lastHeartbeat <= DateTime.UtcNow.Add(-intervalWithTolerance))
						{
							_logger.Warn("Last heartbeat was at {0}, and out of interval of {1}s. Forcing reconnect.", _lastHeartbeat, heartbeatInterval);

							if (!_stopRequested)
								ForceReconnect();

							return;
						}

						await Task.Delay(heartbeatInterval, token);
					}
				}, token);
			}
			catch (OperationCanceledException)
			{
				/* Cancellation has been requested */
			}
		}

		private void ForceReconnect()
		{
			Disconnect();

			Task.Delay(1500).ContinueWith(_ =>
			{
				_stopRequested = false;
				return Connect();
			});
		}

		public void Disconnect()
		{
			_logger.Info("Disconnecting from relay server #{0}", _id);

			_stopRequested = true;
			Stop();

			lock (_lastHeartbeatLock)
			{
				TryDisposeCurrentCancellationTokenSource();
			}
		}

		private void TryDisposeCurrentCancellationTokenSource()
		{
			if (_heartbeatCancellationTokenSource != null)
			{
				_heartbeatCancellationTokenSource.Cancel();
				_heartbeatCancellationTokenSource.Dispose();
				_heartbeatCancellationTokenSource = null;
			}
		}

		public List<string> GetOnPremiseTargetKeys()
		{
			return _connectors.Keys.ToList();
		}

		private async Task RequestOnPremiseTargetAsync(RequestContext ctx, string key, IOnPremiseTargetConnector connector, OnPremiseTargetRequest request, CancellationToken cancellationToken)
		{
			_logger.Debug("Requesting local server {0} for request id {1}", request.Url, request.RequestId);

			var url = (request.Url.Length > key.Length) ? request.Url.Substring(key.Length + 1) : String.Empty;

			if (request.Body != null && request.Body.Length == 0)
			{
				_logger.Trace("Requesting body from relay server. relay-server={0}, request-id={1}", _relayServer, request.RequestId);
				// request the body from the relay server (because SignalR cannot handle large messages)
				var webResponse = await GetToRelay("/request/" + request.RequestId, cancellationToken);
				request.Body = await webResponse.Content.ReadAsByteArrayAsync();
			}

			var response = await connector.GetResponseAsync(url, request);

			_logger.Debug("Sending response from {0} to relay server", request.Url);

			// transfer the result to the relay server (need POST here, because SignalR does not handle large messages)
			HttpContent GetContent() => new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json");

			var retryCount = 0;

			while (!_stopRequested && retryCount++ < _maxRetries)
			{
				try
				{
					await PostToRelayAsync(ctx, GetContent, cancellationToken);
					return;
				}
				catch (Exception ex)
				{
					_logger.Debug(ex, "Error while posting to relay server - retry {0} of {1}", retryCount, _maxRetries);
					Thread.Sleep(1000);
				}
			}

			_logger.Error("Error communicating with relay server. Aborting response...");
		}

		private async Task PostToRelayAsync(RequestContext ctx, Func<HttpContent> content, CancellationToken cancellationToken)
		{
			await PostToRelay("/forward", content, cancellationToken);
			ctx.IsRelayServerNotified = true;
		}

		protected override void OnClosed()
		{
			_logger.Info("Connection closed #{0}", _id);

			base.OnClosed();

            if (!_stopRequested)
            {
                var randomWaitTime = GetRandomWaitTime();
                _logger.Debug("Reconnecting in {0} seconds", randomWaitTime.TotalSeconds);
                Task.Delay(randomWaitTime).ContinueWith(_ => Connect());
            }
		}

		public Task<HttpResponseMessage> GetToRelay(string relativeUrl, CancellationToken cancellationToken)
		{
			return GetToRelay(relativeUrl, null, cancellationToken);
		}

		public async Task<HttpResponseMessage> GetToRelay(string relativeUrl, Action<HttpRequestHeaders> setHeaders, CancellationToken cancellationToken)
		{
			return await SendToRelayAsync(relativeUrl, HttpMethod.Get, setHeaders, null, cancellationToken);
		}

		public Task<HttpResponseMessage> PostToRelay(string relativeUrl, Func<HttpContent> content, CancellationToken cancellationToken)
		{
			return PostToRelay(relativeUrl, null, content, cancellationToken);
		}

		public async Task<HttpResponseMessage> PostToRelay(string relativeUrl, Action<HttpRequestHeaders> setHeaders, Func<HttpContent> content, CancellationToken cancellationToken)
		{
			return await SendToRelayAsync(relativeUrl, HttpMethod.Post, setHeaders, content, cancellationToken);
		}

		private async Task<HttpResponseMessage> SendToRelayAsync(string relativeUrl, HttpMethod httpMethod, Action<HttpRequestHeaders> setHeaders, Func<HttpContent> getContent,
			CancellationToken cancellationToken)
		{
			if (String.IsNullOrWhiteSpace(relativeUrl))
				throw new ArgumentException("Relative url cannot be null or empty.");

			if (!relativeUrl.StartsWith("/"))
				relativeUrl = "/" + relativeUrl;

			var url = new Uri(_relayServer, relativeUrl);

			return await ReplayHttpRequestIfNeededAsync(() =>
			{
				var request = new HttpRequestMessage(httpMethod, url);

				setHeaders?.Invoke(request.Headers);

				if (getContent != null)
					request.Content = getContent();

				return _httpClient.SendAsync(request, cancellationToken);
			});
		}

		private async Task<HttpResponseMessage> ReplayHttpRequestIfNeededAsync(Func<Task<HttpResponseMessage>> httpRequest)
		{
			var result = await httpRequest();

			if (result.StatusCode == HttpStatusCode.Unauthorized)
			{
				// If we don't get a new token and stop is requested, we return the first request
				if (!await TryRequestAuthorizationTokenAsync())
				{
					return result;
				}

				result = await httpRequest();
			}

			return result;
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
