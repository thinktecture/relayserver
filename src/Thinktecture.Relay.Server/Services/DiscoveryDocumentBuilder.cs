using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Services
{
	/// <summary>
	/// An implementation that creates a discovery document.
	/// </summary>
	public class DiscoveryDocumentBuilder
	{
		private readonly ILogger<DiscoveryDocumentBuilder> _logger;
		private readonly IHttpContextAccessor _contextAccessor;

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscoveryDocumentBuilder"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger"/> to log to.</param>
		/// <param name="contextAccessor">An <see cref="IHttpContextAccessor"/> to retrieve the http context from.</param>
		public DiscoveryDocumentBuilder(ILogger<DiscoveryDocumentBuilder> logger, IHttpContextAccessor contextAccessor)
		{
			_logger = logger;
			_contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
		}

		/// <summary>
		/// Builds a discovery document.
		/// </summary>
		/// <returns>A new instance of the discovery document dto.</returns>
		public DiscoveryDocument BuildDiscoveryDocument()
		{
			var baseUrl = BuildBaseUrl();

			return new DiscoveryDocument()
			{
				ServerVersion = "3.0.0-alpha.1",
				ConnectorEndpoint = baseUrl + "connector",
				RequestEndpoint = baseUrl + "request",
				ResponseEndpoint = baseUrl + "response",
				ConnectionTimeout = (int)TimeSpan.FromSeconds(30).TotalSeconds,
				ReconnectMinDelay = (int)TimeSpan.FromSeconds(30).TotalSeconds,
				ReconnectMaxDelay = (int)TimeSpan.FromMinutes(5).TotalSeconds,
			};
		}

		private string BuildBaseUrl()
		{
			var httpContext = _contextAccessor.HttpContext;

			var scheme = httpContext.Request.Scheme;
			var host = httpContext.Request.Host.ToUriComponent();
			var basePath = httpContext.Request.PathBase.Value;

			// remove trailing slash
			if (basePath != null && basePath.EndsWith("/"))
			{
				basePath = basePath.Substring(0, basePath.Length - 1);
			}

			var baseUrl = $"{scheme}://{host}{basePath}";
			// ensure trailing slash
			if (!baseUrl.EndsWith("/"))
			{
				baseUrl += "/";
			}

			_logger?.LogDebug("Base url '{BaseUrl}' was build from: {Scheme}, {Host}, {BasePath}", baseUrl, scheme, host, basePath);

			return baseUrl;
		}
	}
}
