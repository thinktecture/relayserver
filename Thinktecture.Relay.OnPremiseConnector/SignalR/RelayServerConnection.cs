using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using NLog.Interface;
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
        private int _id;
        private string _accessToken;
        private bool _stopRequested;
        private string _relayedRequestHeader = null;

        private static int _nextId = 0;
        private static Random _random = new Random();

        private const int MIN_WAIT_TIME_IN_SECONDS = 2 * 1000;
        private const int MAX_WAIT_TIME_IN_SECONDS = 30 * 1000;
        
        public RelayServerConnection(string userName, string password, Uri relayServer, int requestTimeout, int maxRetries, IOnPremiseTargetConnectorFactory onPremiseTargetConnectorFactory, ILogger logger)
            : base(new Uri(relayServer, "/signalr").AbsoluteUri)
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
        }

        public String RelayedRequestHeader
        {
            set { _relayedRequestHeader = value; }
        }

        public void RegisterOnPremiseTarget(string key, Uri baseUri)
        {
            while (key.EndsWith("/"))
            {
                key = key.Substring(0, key.Length - 1);
            }

            _logger.Trace("Registering On-Premise Target: key={0}, baseUri={1}", key, baseUri);
            _logger.Debug("Registering On-Premise Target.");

            if (baseUri == null)
            {
                IOnPremiseTargetConnector old;
                _connectors.TryRemove(key, out old);
            }
            else
            {
                _connectors[key] = _onPremiseTargetConnectorFactory.Create(baseUri, _requestTimeout);
            }
        }

        private async Task<TokenResponse> GetAuthorizationTokenAsync()
        {
            var client = new OAuth2Client(new Uri(_relayServer, "/token"));

            while (!_stopRequested)
            {
                try
                {
                    _logger.Trace("Requesting authorization token: relayServer={0}. #" + _id, _relayServer);
                    _logger.Debug("Requesting authorization token");

                    var resp = await client.RequestResourceOwnerPasswordAsync(_userName, _password);
         
                    _logger.Trace("Received token for #" + _id);
                    return resp;
                }
                catch (Exception ex)
                {
                    var randomWaitTime = GetRandomWaitTime();
                    _logger.Trace(String.Format("Could not connect and authenticate to relay server - re-trying in {0} second. #{1}", randomWaitTime, _id), ex);
                    Thread.Sleep(randomWaitTime);
                }
            }

            return null;
        }

        public async Task Connect()
        {
            _logger.Info("Connecting...");
            if (!await TryRequestAuthorizationTokenAsync()) return;

            if (!_eventsHooked)
            {
                _eventsHooked = true;

                Reconnecting += OnReconnecting;
                Received += OnReceived;
                Reconnected += OnReconnected;
            }

            try
            {
                await Start();
                _logger.Info("Connected to relay server. #" + _id);
            }
            catch (Exception)
            {
                _logger.Info("***** ERROR WHILE CONNECTING: #" + _id);
                await Delay(5000).ContinueWith(_ => Start());
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

            if (Headers.ContainsKey("Authorization"))
            {
                Headers.Remove("Authorization");
            }

            Headers.Add("Authorization", tokenResponse.TokenType + " " + _accessToken);
            
            _logger.Trace("Setting Bearer token: {0}", _accessToken);
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
                _logger.Trace("Could not authenticate with relay server: reason={0}", token.Error);
                _logger.Warn("Could not authenticate with relay server.");
                throw new Exception("Could not authenticate with relay server: " + token.Error);
            }
        }

        private void OnReconnected()
        {
            _logger.Debug("Connection restored. #" + _id);
        }

        private void OnReconnecting()
        {
            _logger.Debug("Connection lost. Trying to reconnect... #" + _id);
        }

        private async void OnReceived(string data)
        {
            _logger.Trace("Received message from server: data={0}", data);
            _logger.Debug("Received message from server.");

            // receive a client request from relay server
            var onPremiseTargetRequest = JsonConvert.DeserializeObject<OnPremiseTargetRequest>(data);

            if (onPremiseTargetRequest.HttpMethod == "PING")
            {
                await HandlePingRequestAsync(onPremiseTargetRequest);
                return;
            }

            foreach (var connector in _connectors)
            {
                // find the corresponding On-Premise Target
                if (onPremiseTargetRequest.Url.StartsWith(connector.Key + "/", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Trace("Found On-Premise Target. connector={0}", connector.Key);

                    if (_relayedRequestHeader != null)
                    {
                        onPremiseTargetRequest.HttpHeaders[_relayedRequestHeader] = "true";
                    }

                    await RequestOnPremiseTargetAsync(connector, onPremiseTargetRequest);
                    return;
                }
            }

            _logger.Debug("No connector for request {0} found. url={1}", onPremiseTargetRequest.RequestId, onPremiseTargetRequest.Url);
        }

        private async Task HandlePingRequestAsync(IOnPremiseTargetRequest request)
        {
            _logger.Info("Received ping from server");

            await PostToRelayAsync(() => new StringContent(JsonConvert.SerializeObject(new OnPremiseTargetReponse
            {
                RequestStarted = DateTime.UtcNow,
                RequestFinished = DateTime.UtcNow,
                StatusCode = HttpStatusCode.OK,
                OriginId = request.OriginId,
                RequestId = request.RequestId
            }), Encoding.UTF8, "application/json"));
        }

        public void Disconnect()
        {
            _logger.Info("Disconnecting...");
            _stopRequested = true;
            Stop();
        }

        public List<string> GetOnPremiseTargetKeys()
        {
            return _connectors.Keys.ToList();
        }

        private async Task RequestOnPremiseTargetAsync(KeyValuePair<string, IOnPremiseTargetConnector> connector, OnPremiseTargetRequest onPremiseTargetRequest)
        {
            _logger.Debug("Requesting local server {0} for request {1}", connector.Value.BaseUri, onPremiseTargetRequest.RequestId);

            string url = onPremiseTargetRequest.Url.Substring(connector.Key.Length + 1);

            if (onPremiseTargetRequest.Body != null && onPremiseTargetRequest.Body.Length == 0)
            {
                _logger.Trace("Requesting body from relay server. relay-server={0}, request-id={1}", _relayServer, onPremiseTargetRequest.RequestId);
                // request the body from the relay server (because SignalR cannot handle large messages)
                var response =
                    await ReplayHttpRequestIfNeededAsync(() => _httpClient.GetAsync(new Uri(_relayServer, "/request/" + onPremiseTargetRequest.RequestId)));
                onPremiseTargetRequest.Body = await response.Content.ReadAsByteArrayAsync();
            }

            var onPremiseTargetReponse = await connector.Value.GetResponseAsync(url, onPremiseTargetRequest);

            _logger.Debug("Sending reponse from {0} to relay", connector.Value.BaseUri);

            // transfer the result to the relay server (need POST here, because SignalR does not handle large messages)
            Func<HttpContent> content = () => new StringContent(JsonConvert.SerializeObject(onPremiseTargetReponse), Encoding.UTF8, "application/json");

            var currentRetryCount = 0;

            while (!_stopRequested && currentRetryCount < _maxRetries)
            {
                try
                {
                    currentRetryCount++;
                    await PostToRelayAsync(content);
                    return;
                }
                catch (Exception e)
                {
                    _logger.Debug("Error while posting to relay server. Retry {0}/{1}", currentRetryCount, _maxRetries);
                    _logger.Error(e.ToString());
                    Thread.Sleep(1000);
                }
            }

            _logger.Error("Error communitcating with relay server. Aborting response...");
        }

        private async Task PostToRelayAsync(Func<HttpContent> content)
        {
            await ReplayHttpRequestIfNeededAsync(() => _httpClient.PostAsync(new Uri(_relayServer, "/forward"), content()));
        }

        protected override void OnClosed()
        {
            _logger.Info("Closed ... #" + _id);

            base.OnClosed();

            _logger.Debug("Reconnecting in 5 seconds");

            if (!_stopRequested)
            {
                Delay(5000).ContinueWith(_ => Connect());
            }
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

        public static Task Delay(double milliseconds)
        {
            var tcs = new TaskCompletionSource<bool>();
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += (obj, args) => tcs.TrySetResult(true);
            timer.Interval = milliseconds;
            timer.AutoReset = false;
            timer.Start();
            return tcs.Task;
        }

        private int GetRandomWaitTime()
        {
            return _random.Next(MIN_WAIT_TIME_IN_SECONDS, MAX_WAIT_TIME_IN_SECONDS);
        }
    }
}
