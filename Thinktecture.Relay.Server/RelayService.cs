using System;
using Microsoft.Owin.Hosting;
using NLog;

namespace Thinktecture.Relay.Server
{
	internal class RelayService
	{
		private readonly string _hostName;
		private readonly int _port;
		private readonly bool _allowHttp;

		private readonly ILogger _logger;
		private IDisposable _host;

		public RelayService(string hostName, int port, bool allowHttp)
		{
			_hostName = hostName;
			_port = port;
			_allowHttp = allowHttp;
			_logger = LogManager.GetCurrentClassLogger();
		}

		public void Start()
		{
			try
			{
				var address = $"{(_allowHttp ? "http" : "https")}://{_hostName}:{_port}";

#if DEBUG
				_logger.Info("Listing on: {0}", address);
#endif

				_host = WebApp.Start<Startup>(address);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error during start of the relay server listener.");
				throw;
			}
		}

		public void Stop()
		{
			if (_host != null)
			{
				_host.Dispose();
				_host = null;
			}
		}
	}
}
