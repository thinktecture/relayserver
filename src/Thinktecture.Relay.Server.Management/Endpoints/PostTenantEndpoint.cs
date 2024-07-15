using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Thinktecture.Relay.Server.Management.DataTransferObjects;
using Thinktecture.Relay.Server.Management.Extensions;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Management.Endpoints;

/// <summary>
/// Provides extension methods for the <see cref="IEndpointRouteBuilder"/>.
/// </summary>
public static partial class EndpointRouteBuilderExtensions
{
	/// <summary>
	/// Maps the endpoint to add a new tenant.
	/// </summary>
	/// <param name="app">An <see cref="IEndpointRouteBuilder"/>.</param>
	/// <param name="pattern">The url pattern for this endpoint.</param>
	/// <param name="policy">An optional authorization policy.</param>
	/// <returns>The <see cref="IEndpointRouteBuilder"/>.</returns>
	public static IEndpointRouteBuilder MapPostTenant(this IEndpointRouteBuilder app, string pattern, string? policy)
	{
		var builder = app
				.MapPost($"{pattern}", PostTenantEndpoint.HandleRequestAsync)
				.WithName("PostTenant")
				.WithDisplayName("Post tenant")
				.Produces(StatusCodes.Status201Created, typeof(IdResult))
				.Produces(StatusCodes.Status404NotFound);

		if (!string.IsNullOrWhiteSpace(policy))
		{
			builder.RequireAuthorization(policy)
				.Produces(StatusCodes.Status401Unauthorized)
				.Produces(StatusCodes.Status403Forbidden);
		}

		return app;
	}
}

internal static class PostTenantEndpoint
{
	public static async Task<IResult> HandleRequestAsync([FromBody] Tenant tenant, [FromServices] ITenantService service,
		HttpRequest request, CancellationToken cancellationToken = default)
	{
		await service.CreateTenantAsync(tenant.ToEntity(), cancellationToken);
		return Results.Accepted();
	}
}
