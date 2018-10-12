using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Serilog;

namespace Thinktecture.Relay.Server.Filters
{
	public class LogActionFilter : IActionFilter
	{
		private readonly ILogger _logger;

		public bool AllowMultiple => true;

		public LogActionFilter(ILogger logger)
		{
			_logger = logger;
		}

		public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
		{
			if (actionContext == null)
				throw new ArgumentNullException(nameof(actionContext));
			if (continuation == null)
				throw new ArgumentNullException(nameof(continuation));

			var response = await continuation().ConfigureAwait(false);
			_logger?.Verbose("[Response] {RequestMethod} - {ResponseStatusCode} ({ResponseStatusCodeInt}): {RequestUrl}", actionContext.Request?.Method, response.StatusCode, (int)response.StatusCode, actionContext.Request?.RequestUri);

			return response;
		}
	}
}
