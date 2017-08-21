using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using NLog;

namespace Thinktecture.Relay.Server.Filters
{
	public class NLogActionFilter : IActionFilter
	{
		private readonly ILogger _logger;
		private readonly JsonSerializerSettings _jsonSettings;

		/// <inheritdoc />
		public bool AllowMultiple => true;

		public NLogActionFilter(ILogger logger)
		{
			if (logger == null)
				throw new ArgumentNullException(nameof(logger));

			_logger = logger;
			_jsonSettings = new JsonSerializerSettings()
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			};
		}

		public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
		{
			if (actionContext == null)
				throw new ArgumentNullException(nameof(actionContext));
			if (continuation == null)
				throw new ArgumentNullException(nameof(continuation));

			_logger.Debug("[Request] {0}: {1}. Arguments: {2}", actionContext.Request?.Method, actionContext.Request?.RequestUri, SerializeArguments(actionContext.ActionArguments));
			var response = await continuation();
			_logger.Debug("[Response] {0} - {1} ({2}): {3}", actionContext.Request?.Method, response.StatusCode, (int)response.StatusCode, actionContext.Request?.RequestUri);

			return response;
		}

		private string SerializeArguments(Dictionary<string, object> arguments)
		{
			try
			{
				return JsonConvert.SerializeObject(arguments, _jsonSettings);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error during serializing action argements.");
				return null;
			}
		}
	}
}
