using System;
using Microsoft.Owin.Hosting;
using NLog;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server
{
	internal class RelayService
	{
		private readonly ILogger _logger;
		private readonly IStartup _startup;
		private readonly IConfiguration _configuration;

		private IDisposable _host;

		public RelayService(ILogger logger, IConfiguration configuration, IStartup startup)
		{
			_logger = logger;
			_startup = startup ?? throw new ArgumentNullException(nameof(startup));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public void Start()
		{
			try
			{
				var address = $"{(_configuration.UseInsecureHttp ? "http" : "https")}://{_configuration.HostName}:{_configuration.Port}";
				_logger?.Info("Listening on {0}", address);

				_host = WebApp.Start(address, app => _startup.Configuration(app));
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during start of the relay server listener");
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
