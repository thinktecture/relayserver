using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Serilog;
using Thinktecture.Relay.Server.Interceptor;

namespace Thinktecture.Relay.InterceptorDemos
{
	/// <inheritdoc />
	/// <summary>
	/// Example implementation of an interceptor that modifies the request headers
	/// </summary>
	public class DemoRequestInterceptor : IOnPremiseRequestInterceptor
	{
		private readonly ILogger _logger;

		/// <summary>
		/// Creates a new instance of the <see cref="DemoRequestInterceptor"/>
		/// </summary>
		/// <param name="logger">An instance of an <see cref="ILogger"/>, that will be injected by Autofac when this interceptor is created</param>
		public DemoRequestInterceptor(ILogger logger)
		{
			// You can also have the DI inject different custom dependencies, as long as
			// they are all registered in your interceptors Autofac module
			_logger = logger;
		}

		/// <summary>
		/// This method can modify the request and prevent further processing by returning an <see cref="HttpResponseMessage"/>.
		/// </summary>
		/// <param name="request">The request that can be modified.</param>
		/// <returns>If the returned <see cref="HttpResponseMessage"/> is not null then it will immediately be send out to the client without any further processing.</returns>
		public HttpResponseMessage OnRequestReceived(IInterceptedRequest request)
		{
			_logger?.Debug($"{nameof(DemoRequestInterceptor)}.{nameof(OnRequestReceived)} is called.");

			// Example: Set AUTO-ACK to true
			if (request.Url.IndexOf("autoAcknowledge=true", 0, StringComparison.InvariantCultureIgnoreCase) >= 0)
			{
				// This request will be auto-acknowledged when taken out of RabbitMQ.
				// When there is a problem with SignalR or the OnPremiseConnector, this request may not reach the remote API.

				request.AutoAcknowledge = true;
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

			_logger?.Information("Demo interceptor modified request: {@Request}", request);
			return null;
		}
	}
}
