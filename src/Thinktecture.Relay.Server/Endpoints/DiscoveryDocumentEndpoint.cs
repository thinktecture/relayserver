using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Services;

namespace Thinktecture.Relay.Server.Endpoints;

internal static partial class EndpointRouteBuilderExtensions
{
	public static IEndpointRouteBuilder MapDiscoveryDocument(this IEndpointRouteBuilder app, string pattern)
	{
		app.MapGet(pattern, DiscoveryDocumentEndpoint.HandleRequestAsync)
			.WithName("GetDiscoveryDocument")
			.WithDisplayName("Get discovery document")
			.Produces(StatusCodes.Status200OK);

		return app;
	}
}

internal partial class DiscoveryDocumentEndpoint
{
	public static Task<IResult> HandleRequestAsync([FromServices] ILogger<DiscoveryDocumentEndpoint> logger,
		[FromServices] DiscoveryDocumentBuilder documentBuilder, [FromServices] IHttpContextAccessor httpContextAccessor)
	{
		Log.ReturnDiscoveryDocument(logger);

		var document = documentBuilder.Build(httpContextAccessor.HttpContext!.Request);

		return Task.FromResult(Results.Ok(document));
	}
}
