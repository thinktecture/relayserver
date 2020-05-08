using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using Serilog;
using Thinktecture.Relay.Server;
using Thinktecture.Relay.Server.Interceptor;

namespace Thinktecture.Relay.CustomCodeDemo
{
	/// <inheritdoc />
	/// <summary>
	/// Example implementation of an interceptor that modifies the request headers
	/// </summary>
	public class DemoRequestInterceptor : IOnPremiseRequestInterceptor
	{
		private readonly ILogger _logger;
		private readonly HttpRequestContext _context;

		/// <summary>
		/// Creates a new instance of the <see cref="DemoRequestInterceptor"/>
		/// </summary>
		/// <param name="logger">An instance of an <see cref="ILogger"/>, that will be injected by Autofac when this interceptor is created</param>
		/// <param name="context">An instance of an <see cref="HttpRequestContext"/> for access to request specifics.</param>
		public DemoRequestInterceptor(ILogger logger, HttpRequestContext context)
		{
			// You can also have the DI inject different custom dependencies, as long as
			// they are all registered in your interceptors Autofac module
			_logger = logger;
			_context = context;
		}

		/// <summary>
		/// This method can modify the request and prevent further processing by returning an <see cref="HttpResponseMessage"/>.
		/// </summary>
		/// <param name="request">The request that can be modified.</param>
		/// <returns>If the returned <see cref="HttpResponseMessage"/> is not null then it will immediately be send out to the client without any further processing.</returns>
		public HttpResponseMessage OnRequestReceived(IInterceptedRequest request)
		{
			_logger?.Debug($"{nameof(DemoRequestInterceptor)}.{nameof(OnRequestReceived)} is called.");

			if (_context.IsLocal)
			{
				_logger?.Debug("This request comes from localhost.");
			}

			// Example: Move query parameters into own JSON property
			if (request.Url.Contains("?"))
			{
				var parts = request.Url.Split('?');
				request.Url = parts[0];
				request.Properties = new Dictionary<string, object>() {
					{ "Parameter", parts[1] }
				};
			}

			// Example: Modify content
			if (request.HttpHeaders.TryGetValue("Content-Type", out var contentType) && contentType == "application/json")
			{
				using (var reader = new StreamReader(request.Content))
				{
					// get original content
					var content = reader.ReadToEnd();
					_logger?.Information("Received content {Content}", content);

					// modify content
					content = $"{{ \"modified\": true, \"originalContent\": {content} }}";
					request.Content = new MemoryStream(Encoding.UTF8.GetBytes(content));
				}
			}

			// Example: Set Request expiration
			if (request.HttpHeaders.TryGetValue("Request-Expiration", out var expirationValue) && TimeSpan.TryParse(expirationValue, out var expiration))
			{
				_logger?.Information("Interceptor is setting RabbitMQ TTL to {RequestExpiration} for Request {RequestId}", expiration, request.RequestId);
				request.Expiration = expiration;
				return null;
			}

			// Example: Set AcknowledgmentMode
			if (request.HttpHeaders.TryGetValue("Acknowledgment-Mode", out var ackModeValue) && Enum.TryParse(ackModeValue, out AcknowledgmentMode ackMode))
			{
				// This request will be auto-acknowledged when taken out of RabbitMQ.
				// When there is a problem with SignalR or the OnPremiseConnector, this request may not reach the remote API.
				request.AcknowledgmentMode = ackMode;
				return null;
			}

			// If a PUT is received, we immediately reject and tell the user to use PATCH instead
			if (String.Equals(request.HttpMethod, HttpMethod.Put.Method, StringComparison.InvariantCultureIgnoreCase))
			{
				return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed)
				{
					ReasonPhrase = "Use PATCH instead of PUT"
				};
			}

			// If a PATCH is received, we internally change that to PUT
			if (String.Equals(request.HttpMethod, "PATCH", StringComparison.InvariantCultureIgnoreCase))
				request.HttpMethod = HttpMethod.Put.Method;

			// Append something to the url
			request.Url += (request.Url.Contains("?") ? "&" : "?") + "parameterAddedFromDemoInterceptor=valueAddedFromDemoInterceptor";

			// Add an HTTP header
			var headers = request.CloneHttpHeaders();
			headers.Add("X-ThinkectureRelay-Example", $"Added by {nameof(DemoRequestInterceptor)}");
			request.HttpHeaders = headers;

			_logger?.Information("Demo interceptor modified request");
			return null;
		}
	}
}
