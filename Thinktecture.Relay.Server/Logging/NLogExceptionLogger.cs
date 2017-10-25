using System;
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

			_logger?.Error(ctx.Exception, "Action: {0}.{1}, Request: {2}: {3}.",
				ctx.ExceptionContext.ControllerContext?.Controller.GetType().Name ?? "none",
				ctx.ExceptionContext.ActionContext?.ActionDescriptor.ActionName ?? "none",
				ctx.Request?.Method,
				ctx.Request?.RequestUri
			);

			return Task.FromResult(0);
		}
	}
}
