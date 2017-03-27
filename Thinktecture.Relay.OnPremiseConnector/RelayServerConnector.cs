using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using NLog.Interface;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.OnPremiseConnector.SignalR;

namespace Thinktecture.Relay.OnPremiseConnector
{
	public class RelayServerConnector : IDisposable
	{
		private static readonly IContainer _container;

		static RelayServerConnector()
		{
			var builder = new ContainerBuilder();

			builder.RegisterType<RelayServerConnectionFactory>().As<IRelayServerConnectionFactory>();
			builder.RegisterType<OnPremiseTargetConnectorFactory>().As<IOnPremiseTargetConnectorFactory>();

			builder.Register(context => new LoggerAdapter(NLog.LogManager.GetLogger("ClientLogger"))).As<ILogger>().SingleInstance();

			_container = builder.Build();
		}

	    public string RelayedRequestHeader
	    {
	        set { _connection.RelayedRequestHeader = value; }
	    }


	    private IRelayServerConnection _connection;
		private bool _disposed;

        /// <summary>
        /// Creates a new instance of <see cref="RelayServerConnector"/>.
        /// </summary>
        /// <param name="userName">A <see cref="string"/> containing the user name.</param>
        /// <param name="password">A <see cref="string"/> containing the password.</param>
        /// <param name="relayServer">An <see cref="Uri"/> containing the relay server's base url.</param>
        /// <param name="requestTimeout">An <see cref="int"/> defining the timeout in seconds.</param>
        /// <param name="maxRetries">An <see cref="int"/> defining how much retries the connector should do for posting the answer back to the relay server.</param>
        public RelayServerConnector(string userName, string password, Uri relayServer, int requestTimeout = 10, int maxRetries = 3)
		{
		    var factory = _container.Resolve<IRelayServerConnectionFactory>();
			_connection = factory.Create(userName, password, relayServer, requestTimeout, maxRetries);
		}

        /// <summary>
        /// Registers a On-Premise Target.
        /// </summary>
        /// <param name="key">A <see cref="string"/> defining the key for the target.</param>
        /// <param name="uri">An <see cref="Uri"/> containing the On-Premise Target's base url. If this value is null, the registration will be removed</param>
        public void RegisterOnPremiseTarget(string key, Uri uri)
		{
			CheckDisposed();

			_connection.RegisterOnPremiseTarget(key, uri);
		}

        /// <summary>
        /// Removes a On-Premise Target.
        /// </summary>
        /// <param name="key">A <see cref="string"/> defining the key for the target.</param>
        public void RemoveOnPremiseTarget(string key)
		{
			CheckDisposed();
			_connection.RegisterOnPremiseTarget(key, null);
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
		/// Connects to the relay server.
		/// </summary>
		public async Task Connect()
		{
			CheckDisposed();

			await _connection.Connect();
		}

		/// <summary>
		/// Disconnectes from the relay server.
		/// </summary>
		public void Disconnect()
		{
			CheckDisposed();

			_connection.Disconnect();
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
