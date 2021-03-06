using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Thinktecture.Relay.OnPremiseConnector.SignalR;

namespace Thinktecture.Relay.OnPremiseConnector
{
	/// <inheritdoc cref="IRelayServerConnector" />
	public class RelayServerConnector : IDisposable, IRelayServerConnector
	{
		private static readonly IServiceProvider _serviceProvider;

		/// <inheritdoc />
		public bool LogSensitiveData { get; set; }

		/// <inheritdoc />
		public event EventHandler Connected;
		/// <inheritdoc />
		public event EventHandler Disconnected;

		static RelayServerConnector()
		{
			var builder = new ContainerBuilder();

			builder.RegisterOnPremiseConnectorTypes();
			builder.Register(ctx => _serviceProvider);
			var container = builder.Build();

			_serviceProvider = new AutofacServiceProvider(container);
		}

		/// <inheritdoc />
		public string RelayedRequestHeader
		{
			get => _connection.RelayedRequestHeader;
			set => _connection.RelayedRequestHeader = value;
		}

		private IRelayServerConnection _connection;
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

			var factory = (serviceProvider ?? _serviceProvider).GetService(typeof(IRelayServerConnectionFactory)) as IRelayServerConnectionFactory;
			_connection = factory.Create(versionAssembly, userName, password, relayServer, TimeSpan.FromSeconds(requestTimeoutInSeconds), TimeSpan.FromSeconds(tokenRefreshWindowInSeconds), LogSensitiveData);
			_connection.Connected += (s, e) => Connected?.Invoke(s, e);
			_connection.Disconnected += (s, e) => Disconnected?.Invoke(s, e);
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
			: this (versionAssembly, userName, password, relayServer, requestTimeoutInSeconds, 5, serviceProvider)
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
			_connection.RegisterOnPremiseTarget(key, uri, followRedirects);
		}

		/// <summary>
		/// Registers a on-premise in-proc target.
		/// </summary>
		/// <param name="key">A <see cref="String"/> defining the key for the target.</param>
		/// <param name="handlerType">A <see cref="Type"/> implementing <see cref="IOnPremiseInProcHandler"/>.</param>
		public void RegisterOnPremiseTarget(string key, Type handlerType)
		{
			CheckDisposed();
			_connection.RegisterOnPremiseTarget(key, handlerType);
		}

		/// <summary>
		/// Registers a on-premise in-proc target.
		/// </summary>
		/// <param name="key">A <see cref="String"/> defining the key for the target.</param>
		/// <param name="handlerFactory">Creates handler.</param>
		public void RegisterOnPremiseTarget(string key, Func<IOnPremiseInProcHandler> handlerFactory)
		{
			CheckDisposed();
			_connection.RegisterOnPremiseTarget(key, handlerFactory);
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
			_connection.RegisterOnPremiseTarget<T>(key);
		}

		/// <summary>
		/// Removes a on-premise target.
		/// </summary>
		/// <param name="key">A <see cref="String"/> defining the key for the target.</param>
		public void RemoveOnPremiseTarget(string key)
		{
			CheckDisposed();
			_connection.RemoveOnPremiseTarget(key);
		}

		/// <summary>
		/// Returns the list of configured target keys
		/// </summary>
		public List<string> GetOnPremiseTargetKeys()
		{
			CheckDisposed();
			return _connection.GetOnPremiseTargetKeys();
		}

		/// <summary>
		/// Connects to the RelayServer.
		/// </summary>
		public async Task ConnectAsync()
		{
			CheckDisposed();
			await _connection.ConnectAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Disconnectes from the RelayServer.
		/// </summary>
		public void Disconnect()
		{
			CheckDisposed();
			_connection.Disconnect();
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

			return _connection.GetToRelayAsync("relay/" + linkName + relativeUrl, setHeaders, cancellationToken);
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

			return _connection.PostToRelayAsync("relay/" + linkName + relativeUrl, setHeaders, content, cancellationToken);
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
			return _connection.SendAcknowledgmentAsync(acknowledgeOriginId, acknowledgeId, connectionId);
		}

		private void CheckDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("RelayServerConnector");
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

				if (_connection != null)
				{
					_connection.Dispose();
					_connection = null;
				}
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
