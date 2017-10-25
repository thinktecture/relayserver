using System;
using Autofac;
using Microsoft.Owin.Hosting;
using NLog;

namespace Thinktecture.Relay.Server
{
	internal class RelayService
	{
		private readonly string _hostName;
		private readonly int _port;
		private readonly bool _allowHttp;
		private readonly ILifetimeScope _scope;
		private readonly ILogger _logger;

		private IDisposable _host;

		public RelayService(string hostName, int port, bool allowHttp, ILifetimeScope scope)
		{
			_hostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
			_port = port;
			_allowHttp = allowHttp;
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			_logger = LogManager.GetCurrentClassLogger();
		}

		public void Start()
		{
			try
			{
				var address = $"{(_allowHttp ? "http" : "https")}://{_hostName}:{_port}";

#if DEBUG
				_logger?.Info("Listening on: {0}", address);
#endif

				_host = WebApp.Start(address, app =>  _scope.Resolve<Startup>().Configuration(app));
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during start of the relay server listener.");
				throw;
			}
		}

		public void Stop()
		{
			_host?.Dispose();
			_host = null;
		}
	}
}
