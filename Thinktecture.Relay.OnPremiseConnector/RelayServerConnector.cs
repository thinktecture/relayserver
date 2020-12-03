using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Thinktecture.Relay.OnPremiseConnector.ServerMigration;
using Thinktecture.Relay.OnPremiseConnector.SignalR;

namespace Thinktecture.Relay.OnPremiseConnector
{
	/// <inheritdoc cref="IRelayServerConnector" />
	public class RelayServerConnector : IDisposable, IRelayServerConnector
	{
		private static int _nextInstanceId;
		private static string _relayedRequestHeader;

		internal static int GetNextInstanceId() => Interlocked.Increment(ref _nextInstanceId);
		internal static string GetRelayedRequestHeader() => _relayedRequestHeader;

		/// <inheritdoc />
		public bool LogSensitiveData { get; set; }

		/// <inheritdoc />
		public event EventHandler Connected;
		/// <inheritdoc />
		public event EventHandler Disconnected;

		/// <inheritdoc />
		public string RelayedRequestHeader
		{
			get => _relayedRequestHeader;
			set => _relayedRequestHeader = value;
		}

		private IRelayServerConnection _connectionv2;
		private IRelayServerConnection _connectionv3;
		private CancellationTokenSource _reconnecting;

		private readonly Uri _relayServerBaseUri;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IServiceProvider _serviceProvider;

		private IRelayServerConnection ActiveConnection => _connectionv3 ?? _connectionv2;

		private bool _disposed;

		/// <summary>
		/// Creates a new instance of <see cref="RelayServerConnector"/>.
		/// </summary>
		/// <param name="versionAssembly">An <see cref="Assembly"/> to be used as version.</param>
		/// <param name="userName">A <see cref="String"/> containing the user name.</param>
		/// <param name="password">A <see cref="String"/> containing the password.</param>
		/// <param name="relayServer">An <see cref="Uri"/> containing the RelayServer's base url.</param>
		/// <param name="requestTimeoutInSeconds">An <see cref="Int32"/> defining the timeout in seconds.</param>
		/// <param name="tokenRefreshWindowInSeconds">An <see cref="Int32"/> defining the access token refresh window in seconds.</param>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/> used for injecting services as required.</param>
		/// <param name="logSensitiveData">Determines whether sensitive data will be logged.</param>
		[Obsolete("Use the ctor without tokenRefreshWindowInSeconds instead.")]
		public RelayServerConnector(Assembly versionAssembly, string userName, string password, Uri relayServer, int requestTimeoutInSeconds = 30,
			int tokenRefreshWindowInSeconds = 5, IServiceProvider serviceProvider = null, bool logSensitiveData = true)
		{
			LogSensitiveData = logSensitiveData;
			_relayServerBaseUri = relayServer ?? throw new ArgumentNullException(nameof(relayServer));

			_serviceProvider = serviceProvider ?? CreateServiceProvider(_relayServerBaseUri, userName, password);
			_httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();

			var factory = _serviceProvider.GetRequiredService<IRelayServerConnectionFactory>();
			_connectionv2 = factory.Create(versionAssembly, userName, password, _relayServerBaseUri, TimeSpan.FromSeconds(requestTimeoutInSeconds), TimeSpan.FromSeconds(tokenRefreshWindowInSeconds), LogSensitiveData);
			_connectionv2.Connected += HandleConnected;
			_connectionv2.Reconnecting += HandleReconnecting;
			_connectionv2.Reconnected += HandleReconnected;
		}

		/// <summary>
		/// Creates a new instance of <see cref="RelayServerConnector"/>.
		/// </summary>
		/// <param name="versionAssembly">An <see cref="Assembly"/> to be used as version.</param>
		/// <param name="userName">A <see cref="String"/> containing the user name.</param>
		/// <param name="password">A <see cref="String"/> containing the password.</param>
		/// <param name="relayServer">An <see cref="Uri"/> containing the RelayServer's base url.</param>
		/// <param name="requestTimeoutInSeconds">An <see cref="Int32"/> defining the timeout in seconds.</param>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/> used for injecting services as required.</param>
		public RelayServerConnector(Assembly versionAssembly, string userName, string password, Uri relayServer, int requestTimeoutInSeconds = 30, IServiceProvider serviceProvider = null)
#pragma warning disable CS0618 // Type or member is obsolete; Justification: Backward-compatibility with older servers that do not yet provide server-side config
			: this(versionAssembly, userName, password, relayServer, requestTimeoutInSeconds, 5, serviceProvider)
#pragma warning restore CS0618 // Type or member is obsolete;
		{
		}

		/// <summary>
		/// Registers a on-premise web target.
		/// </summary>
		/// <param name="key">A <see cref="String"/> defining the key for the target.</param>
		/// <param name="uri">An <see cref="Uri"/> containing the on-premise target's base url.</param>
		/// <param name="followRedirects">A <see cref="bool"/> defining whether redirects will automatically be followed.</param>
		public void RegisterOnPremiseTarget(string key, Uri uri, bool followRedirects = true)
		{
			CheckDisposed();
			_connectionv2.RegisterOnPremiseTarget(key, uri, followRedirects);
			_connectionv3?.RegisterOnPremiseTarget(key, uri, followRedirects);
			RelayServerConnectionv3.RegisterStaticOnPremiseTarget(key, uri, followRedirects);
		}

		/// <summary>
		/// Registers a on-premise in-proc target.
		/// </summary>
		/// <param name="key">A <see cref="String"/> defining the key for the target.</param>
		/// <param name="handlerType">A <see cref="Type"/> implementing <see cref="IOnPremiseInProcHandler"/>.</param>
		public void RegisterOnPremiseTarget(string key, Type handlerType)
		{
			CheckDisposed();
			_connectionv2.RegisterOnPremiseTarget(key, handlerType);
			_connectionv3?.RegisterOnPremiseTarget(key, handlerType); // will throw if active
		}

		/// <summary>
		/// Registers a on-premise in-proc target.
		/// </summary>
		/// <param name="key">A <see cref="String"/> defining the key for the target.</param>
		/// <param name="handlerFactory">Creates handler.</param>
		public void RegisterOnPremiseTarget(string key, Func<IOnPremiseInProcHandler> handlerFactory)
		{
			CheckDisposed();
			_connectionv2.RegisterOnPremiseTarget(key, handlerFactory);
			_connectionv3?.RegisterOnPremiseTarget(key, handlerFactory); // will throw if active
		}

		/// <summary>
		/// Registers a on-premise in-proc target.
		/// </summary>
		/// <param name="key">A <see cref="String"/> defining the key for the target.</param>
		/// <typeparam name="T">The type of the handler.</typeparam>
		public void RegisterOnPremiseTarget<T>(string key)
			where T : IOnPremiseInProcHandler, new()
		{
			CheckDisposed();
			_connectionv2.RegisterOnPremiseTarget<T>(key);
			_connectionv3?.RegisterOnPremiseTarget<T>(key); // will throw if active
		}

		/// <summary>
		/// Removes a on-premise target.
		/// </summary>
		/// <param name="key">A <see cref="String"/> defining the key for the target.</param>
		public void RemoveOnPremiseTarget(string key)
		{
			CheckDisposed();
			_connectionv2.RemoveOnPremiseTarget(key);
			_connectionv3?.RemoveOnPremiseTarget(key);
			RelayServerConnectionv3.RemoveStaticOnPremiseTarget(key);
		}

		/// <summary>
		/// Returns the list of configured target keys
		/// </summary>
		public List<string> GetOnPremiseTargetKeys()
		{
			CheckDisposed();
			return _connectionv2.GetOnPremiseTargetKeys();
		}

		/// <summary>
		/// Connects to the RelayServer.
		/// </summary>
		public async Task ConnectAsync()
		{
			CheckDisposed();
			await DetermineCurrentlyActiveConnectionAsync();
			await ActiveConnection.ConnectAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Disconnectes from the RelayServer.
		/// </summary>
		public void Disconnect()
		{
			CheckDisposed();
			ActiveConnection.Disconnect();
			Disconnected?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Makes a GET request to RelayServer using the current authentication token.
		/// </summary>
		/// <param name="linkName">The name of the relay link.</param>
		/// <param name="relativeUrl">Url relative to the RelayServer url.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Http response</returns>
		public Task<HttpResponseMessage> GetViaRelay(string linkName, string relativeUrl, CancellationToken cancellationToken)
		{
			return GetViaRelay(linkName, relativeUrl, null, cancellationToken);
		}

		/// <summary>
		/// Makes a GET request to RelayServer using the current authentication token.
		/// </summary>
		/// <param name="linkName">The name of the relay link.</param>
		/// <param name="relativeUrl">Url relative to the RelayServer url.</param>
		/// <param name="setHeaders">Callback for setting headers.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Http response</returns>
		public Task<HttpResponseMessage> GetViaRelay(string linkName, string relativeUrl, Action<HttpRequestHeaders> setHeaders, CancellationToken cancellationToken)
		{
			if (linkName == null)
				throw new ArgumentNullException(nameof(linkName));
			if (relativeUrl == null)
				throw new ArgumentNullException(nameof(relativeUrl));

			CheckDisposed();

			if (!relativeUrl.StartsWith("/"))
				relativeUrl = "/" + relativeUrl;

			return ActiveConnection.GetToRelayAsync("relay/" + linkName + relativeUrl, setHeaders, cancellationToken);
		}

		/// <summary>
		/// Makes a POST request to RelayServer using the current authentication token.
		/// </summary>
		/// <param name="linkName">The name of the relay link.</param>
		/// <param name="relativeUrl">Url relative to the RelayServer url.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Http response</returns>
		public Task<HttpResponseMessage> PostViaRelay(string linkName, string relativeUrl, CancellationToken cancellationToken)
		{
			return PostViaRelay(linkName, relativeUrl, null, null, cancellationToken);
		}

		/// <summary>
		/// Makes a POST request to RelayServer using the current authentication token.
		/// </summary>
		/// <param name="linkName">The name of the relay link.</param>
		/// <param name="relativeUrl">Url relative to the RelayServer url.</param>
		/// <param name="setHeaders">Callback for setting headers.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Http response</returns>
		public Task<HttpResponseMessage> PostViaRelay(string linkName, string relativeUrl, Action<HttpRequestHeaders> setHeaders, CancellationToken cancellationToken)
		{
			return PostViaRelay(linkName, relativeUrl, setHeaders, null, cancellationToken);
		}

		/// <summary>
		/// Makes a POST request to RelayServer using the current authentication token.
		/// </summary>
		/// <param name="linkName">The name of the relay link.</param>
		/// <param name="relativeUrl">Url relative to the RelayServer url.</param>
		/// <param name="content">The <see cref="HttpContent"/> to post through the RelayServer.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Http response</returns>
		public Task<HttpResponseMessage> PostViaRelay(string linkName, string relativeUrl, HttpContent content, CancellationToken cancellationToken)
		{
			return PostViaRelay(linkName, relativeUrl, null, content, cancellationToken);
		}

		/// <summary>
		/// Makes a POST request to RelayServer using the current authentication token.
		/// </summary>
		/// <param name="linkName">The name of the relay link.</param>
		/// <param name="relativeUrl">Url relative to the RelayServer url.</param>
		/// <param name="setHeaders">A Callback for setting headers.</param>
		/// <param name="content">The <see cref="HttpContent"/> to post through the RelayServer.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Http response</returns>
		public Task<HttpResponseMessage> PostViaRelay(string linkName, string relativeUrl, Action<HttpRequestHeaders> setHeaders, HttpContent content, CancellationToken cancellationToken)
		{
			if (linkName == null)
				throw new ArgumentNullException(nameof(linkName));
			if (relativeUrl == null)
				throw new ArgumentNullException(nameof(relativeUrl));

			CheckDisposed();

			if (!relativeUrl.StartsWith("/"))
				relativeUrl = "/" + relativeUrl;

			return ActiveConnection.PostToRelayAsync("relay/" + linkName + relativeUrl, setHeaders, content, cancellationToken);
		}

		/// <summary>
		/// Sends an acknowledgment to the RelayServer using the current authentication token.
		/// </summary>
		/// <param name="acknowledgeOriginId">The OriginId of the RelayServer instance that needs to acknowledge the request message on its rabbit connection.</param>
		/// <param name="acknowledgeId">The id of the message in the queue that should be acknowledged.</param>
		/// <param name="connectionId">The Id of the connection which identifies the Rabbit queue to acknowledge the message on.</param>
		/// <returns>A Task that completes when the acknowledge http request is answered.</returns>
		public Task AcknowledgeRequestAsync(Guid acknowledgeOriginId, string acknowledgeId, string connectionId = null)
		{
			return ActiveConnection.SendAcknowledgmentAsync(acknowledgeOriginId, acknowledgeId, connectionId);
		}

		private IServiceProvider CreateServiceProvider(Uri relayServerBaseUri, string tenantName, string tenantSecret)
		{
			// Build dotnet core DI for connector v3
			IServiceCollection services = new ServiceCollection();
			services
				.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger))
				.AddDistributedMemoryCache()
				.AddRelayConnector(options =>
				{
					options.RelayServerBaseUri = relayServerBaseUri;
					options.TenantName = tenantName;
					options.TenantSecret = tenantSecret;
				})
				.AddSignalRConnectorTransport();

			// Create autofac container for v2 connector
			var builder = new ContainerBuilder();
			builder.RegisterOnPremiseConnectorTypes();

			// Also add v3 services to autofac
			builder.Populate(services);

			return new AutofacServiceProvider(builder.Build());
		}

		private async Task DetermineCurrentlyActiveConnectionAsync()
		{
			using (var client = _httpClientFactory.CreateClient())
			{
				client.Timeout = TimeSpan.FromSeconds(20);
				client.BaseAddress = _relayServerBaseUri;
				while (true)
				{
					try
					{
						var response = await client.GetAsync(DiscoveryDocument.WellKnownPath).ConfigureAwait(false);

						if (response.IsSuccessStatusCode)
						{
							_connectionv3 = _serviceProvider.GetRequiredService<RelayServerConnectionv3>();
							_connectionv3.Connected += HandleConnected;
							_connectionv3.Reconnecting += HandleReconnecting;
							_connectionv3.Reconnected += HandleReconnected;
						}

						break;
					}
					catch
					{
						await Task.Delay(TimeSpan.FromSeconds(10));
					}
				}
			}
		}

		private void HandleConnected(object sender, EventArgs e)
		{
			_reconnecting?.Cancel();
			_reconnecting?.Dispose();
			_reconnecting = null;
			Connected?.Invoke(sender, e);
		}

		private void HandleReconnecting(object sender, EventArgs e)
		{
			_reconnecting = new CancellationTokenSource();

			// Restart connecting after one minute (maybe the server got upgraded in the meantime)
			Task.Delay(TimeSpan.FromMinutes(1), _reconnecting.Token).ContinueWith(async _ =>
			{
				CheckDisposed();
				_connectionv2.Disconnect();

				_connectionv3?.Disconnect();
				_connectionv3?.Dispose();
				_connectionv3 = null;

				await ConnectAsync().ConfigureAwait(false);
			});
		}

		private void HandleReconnected(object sender, EventArgs e)
		{
			_reconnecting?.Cancel();
			_reconnecting?.Dispose();
			_reconnecting = null;
		}

		private void CheckDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(RelayServerConnector));
			}
		}

		#region IDisposable

		/// <inheritdoc />
		~RelayServerConnector()
		{
			Dispose(false);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">Determines whether this is called from the public <see cref="Dispose()"/> method.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_disposed = true;

				_connectionv2?.Dispose();
				_connectionv2 = null;

				_connectionv3?.Dispose();
				_connectionv3 = null;
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
