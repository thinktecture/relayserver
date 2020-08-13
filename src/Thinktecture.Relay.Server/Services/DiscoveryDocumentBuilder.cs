using System;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Thinktecture.Relay.Server.Services
{
	/// <summary>
	/// An implementation that creates a discovery document.
	/// </summary>
	public class DiscoveryDocumentBuilder
	{
		private readonly ILogger<DiscoveryDocumentBuilder> _logger;
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Initializes a new instance of <see cref="DiscoveryDocumentBuilder"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		public DiscoveryDocumentBuilder(ILogger<DiscoveryDocumentBuilder> logger, IServiceProvider serviceProvider)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		/// <summary>
		/// Builds a discovery document.
		/// </summary>
		/// <param name="request">A <see cref="HttpRequest"/>.</param>
		/// <returns>A new instance of the discovery document.</returns>
		public DiscoveryDocument BuildDiscoveryDocument(HttpRequest request)
		{
			var baseUri = BuildBaseUri(request);

			return new DiscoveryDocument()
			{
				ServerVersion = GetServerVersion(),
				AuthorizationServer = GetAuthority(),
				ConnectorEndpoint = new Uri(baseUri, "connector").ToString(),
				RequestEndpoint = new Uri(baseUri, "body/request").ToString(),
				ResponseEndpoint = new Uri(baseUri, "body/response").ToString(),
				ConnectionTimeout = (int)TimeSpan.FromSeconds(30).TotalSeconds,
				ReconnectMinDelay = (int)TimeSpan.FromSeconds(30).TotalSeconds,
				ReconnectMaxDelay = (int)TimeSpan.FromMinutes(5).TotalSeconds,
			};
		}

		private Uri BuildBaseUri(HttpRequest request) => new Uri($"{request.Scheme}://{request.Host}{request.PathBase}", UriKind.Absolute);

		private string GetServerVersion()
		{
			return GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
				?? GetType().Assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version;
		}

		private string GetAuthority()
			=> _serviceProvider.GetService<IOptionsSnapshot<JwtBearerOptions>>()?.Get(Constants.DefaultAuthenticationScheme)?.Authority;
	}
}
