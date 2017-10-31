using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Serilog;

namespace Thinktecture.Relay.Server.Logging
{
	public class ExceptionLogger : IExceptionLogger
	{
		private readonly ILogger _logger;

		public ExceptionLogger(ILogger logger)
		{
			_logger = logger;
		}

		public Task LogAsync(ExceptionLoggerContext ctx, CancellationToken cancellationToken)
		{
			if (ctx == null)
				throw new ArgumentNullException(nameof(ctx));

			_logger?.Error(ctx.Exception, "Action: {action-controller}.{action-action}, Request: {request-method}: {request-url}.",
				ctx.ExceptionContext.ControllerContext?.Controller.GetType().Name ?? "none",
				ctx.ExceptionContext.ActionContext?.ActionDescriptor.ActionName ?? "none",
				ctx.Request?.Method,
				ctx.Request?.RequestUri
			);

			return Task.FromResult(0);
		}
	}
}
