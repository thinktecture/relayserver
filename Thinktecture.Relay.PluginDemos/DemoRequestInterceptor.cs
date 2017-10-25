using System;
using System.Net;
using System.Net.Http;
using NLog;
using Thinktecture.Relay.Server.Plugins;

namespace Thinktecture.Relay.PluginDemos
{
	/// <inheritdoc />
	/// <summary>
	/// Example implementation of a plugin that modifies the request headers
	/// </summary>
	public class DemoRequestInterceptor : IOnPremiseRequestInterceptor
	{
		private readonly ILogger _logger;

		/// <summary>
		/// Creates a new instance of the <see cref="DemoRequestInterceptor"/>
		/// </summary>
		/// <param name="logger">An instance of an <see cref="ILogger"/>, that will be injected by Autofac when this plugin is created</param>
		public DemoRequestInterceptor(ILogger logger)
		{
			// You can also have the DI inject different custom dependencies, as long as
			// they are all registered in your plugins Autofac module
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// This method can modify the request and prevent further processing by returning an <see cref="HttpResponseMessage"/>.
		/// </summary>
		/// <param name="request">The request that can be modified.</param>
		/// <returns>If the returned <see cref="HttpResponseMessage"/> is not null then it will immidiately be send out to the client without any further processing.</returns>
		public HttpResponseMessage OnRequestReceived(IInterceptedRequest request)
		{
			_logger.Debug($"{nameof(DemoRequestInterceptor)}.{nameof(OnRequestReceived)} is called.");

			// If a PUT is received, we immidiately reject and tell the user to use PATCH instead
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

			// Add an HTTP header
			request.HttpHeaders.Add("X-ThinkectureRelay-Example", $"Added by {nameof(DemoRequestInterceptor)}");

			return null;
		}
	}
}
