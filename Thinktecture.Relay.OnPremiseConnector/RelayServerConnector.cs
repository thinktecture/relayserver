using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutofacSerilogIntegration;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.OnPremiseConnector.SignalR;

namespace Thinktecture.Relay.OnPremiseConnector
{
	public class RelayServerConnector : IDisposable, IRelayServerConnector
	{
		private static readonly IContainer _container;

		static RelayServerConnector()
		{
			var builder = new ContainerBuilder();

			builder.RegisterLogger();
			builder.RegisterType<OnPremiseWebTargetRequestMessageBuilder>().As<IOnPremiseWebTargetRequestMessageBuilder>();
			builder.RegisterType<RelayServerConnectionFactory>().As<IRelayServerConnectionFactory>();
			builder.RegisterType<HttpClientFactory>().As<IHttpClientFactory>().SingleInstance();
			builder.RegisterType<OnPremiseTargetConnectorFactory>().As<IOnPremiseTargetConnectorFactory>();
			builder.RegisterType<HeartbeatChecker>().As<IHeartbeatChecker>();
			builder.RegisterType<TokenExpiryChecker>().As<ITokenExpiryChecker>();
			builder.RegisterType<MaintenanceLoop>().As<IMaintenanceLoop>().SingleInstance().OnActivated(e => e.Instance.StartLoop());

			_container = builder.Build();
		}

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
		public RelayServerConnector(Assembly versionAssembly, string userName, string password, Uri relayServer, int requestTimeoutInSeconds = 30, int tokenRefreshWindowInSeconds = 5)
		{
			var factory = _container.Resolve<IRelayServerConnectionFactory>();
			_connection = factory.Create(versionAssembly, userName, password, relayServer, TimeSpan.FromSeconds(requestTimeoutInSeconds), TimeSpan.FromSeconds(tokenRefreshWindowInSeconds));
		}

		/// <summary>
		/// Registers a on-premise web target.
		/// </summary>
		/// <param name="key">A <see cref="String"/> defining the key for the target.</param>
		/// <param name="uri">An <see cref="Uri"/> containing the on-premise target's base url.</param>
		public void RegisterOnPremiseTarget(string key, Uri uri)
		{
			CheckDisposed();
			_connection.RegisterOnPremiseTarget(key, uri);
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

			return _connection.GetToRelay("relay/" + linkName + relativeUrl, setHeaders, cancellationToken);
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

			return _connection.PostToRelay("relay/" + linkName + relativeUrl, setHeaders, content, cancellationToken);
		}

		private void CheckDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("RelayServerConnector");
			}
		}

		#region IDisposable

		~RelayServerConnector()
		{
			Dispose(false);
		}

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

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
