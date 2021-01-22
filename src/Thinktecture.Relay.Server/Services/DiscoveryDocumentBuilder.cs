using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Thinktecture.Relay.Server.Services
{
	/// <summary>
	/// An implementation that creates a discovery document.
	/// </summary>
	public class DiscoveryDocumentBuilder
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly RelayServerOptions _relayServerOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscoveryDocumentBuilder"/> class.
		/// </summary>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		/// <param name="relayServerOptions">An <see cref="IOptions{TOptions}"/>.</param>
		public DiscoveryDocumentBuilder(IServiceProvider serviceProvider, IOptions<RelayServerOptions> relayServerOptions)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_relayServerOptions = relayServerOptions?.Value ?? throw new ArgumentNullException(nameof(relayServerOptions));
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
				ServerVersion = GetType().GetAssemblyVersion(),
				AuthorizationServer = GetAuthority(),
				EndpointTimeout = _relayServerOptions.EndpointTimeout,
				ConnectorEndpoint = new Uri(baseUri, "connector").ToString(),
				AcknowledgeEndpoint = new Uri(baseUri, "acknowledge").ToString(),
				RequestEndpoint = new Uri(baseUri, "body/request").ToString(),
				ResponseEndpoint = new Uri(baseUri, "body/response").ToString(),
				ReconnectMinimumDelay = _relayServerOptions.ReconnectMinimumDelay,
				ReconnectMaximumDelay = _relayServerOptions.ReconnectMaximumDelay,
				HandshakeTimeout = _relayServerOptions.HandshakeTimeout,
				KeepAliveInterval = _relayServerOptions.KeepAliveInterval,
			};
		}

		private Uri BuildBaseUri(HttpRequest request) => new Uri($"{request.Scheme}://{request.Host}{request.PathBase}", UriKind.Absolute);

		private string GetAuthority()
			=> _serviceProvider.GetService<IOptionsSnapshot<JwtBearerOptions>>()?.Get(Constants.DefaultAuthenticationScheme)?.Authority;
	}
}
