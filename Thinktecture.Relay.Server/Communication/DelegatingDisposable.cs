using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Thinktecture.Relay.Server.Communication
{
	public class DelegatingDisposable : IDisposable
	{
		private readonly ILogger _logger;
		private readonly Action _callback;

		public DelegatingDisposable(ILogger logger, Action callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			_logger = logger;
			_callback = callback;
		}

		public void Dispose()
		{
			try
			{
				_callback();
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error during dispose.");
			}
		}
	}
}