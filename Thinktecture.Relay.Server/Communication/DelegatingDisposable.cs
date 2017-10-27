using System;
using NLog;

namespace Thinktecture.Relay.Server.Communication
{
	public class DelegatingDisposable : IDisposable
	{
		private readonly ILogger _logger;
		private readonly Action _callback;

		public DelegatingDisposable(ILogger logger, Action callback)
		{
			_logger = logger;
			_callback = callback ?? throw new ArgumentNullException(nameof(callback));
		}

		public void Dispose()
		{
			try
			{
				_callback();
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during dispose");
			}
		}
	}
}
