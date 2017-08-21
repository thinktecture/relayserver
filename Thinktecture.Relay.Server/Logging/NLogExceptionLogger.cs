using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using NLog;

namespace Thinktecture.Relay.Server.Logging
{
	public class NLogExceptionLogger : IExceptionLogger
	{
		private readonly ILogger _logger;

		public NLogExceptionLogger(ILogger logger)
		{
			_logger = logger;
		}

		public Task LogAsync(ExceptionLoggerContext ctx, CancellationToken cancellationToken)
		{
			if (ctx == null)
				throw new ArgumentNullException(nameof(ctx));

			_logger.Error(ctx.Exception, "Url: {0}, Http method: {1}.", ctx.Request?.RequestUri, ctx.Request?.Method);

			return Task.FromResult(0);
		}
	}
}
