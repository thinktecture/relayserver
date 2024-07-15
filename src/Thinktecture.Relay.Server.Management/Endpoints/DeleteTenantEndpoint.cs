using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Management.Endpoints;

/// <summary>
/// Provides extension methods for the <see cref="IEndpointRouteBuilder"/>.
/// </summary>
public static partial class EndpointRouteBuilderExtensions
{
	/// <summary>
	/// Maps the endpoint to delete an existing tenant.
	/// </summary>
	/// <param name="app">An <see cref="IEndpointRouteBuilder"/>.</param>
	/// <param name="pattern">The url pattern for this endpoint.</param>
	/// <param name="policy">An optional authorization policy.</param>
	/// <returns>The <see cref="IEndpointRouteBuilder"/>.</returns>
	public static IEndpointRouteBuilder MapDeleteTenant(this IEndpointRouteBuilder app, string pattern, string? policy)
	{
		var builder = app
			.MapDelete($"{pattern}/{{tenantName}}", DeleteTenantEndpoint.HandleRequestAsync)
			.WithName("DeleteTenant")
			.WithDisplayName("Delete tenant")
			.Produces(StatusCodes.Status204NoContent)
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

internal static class DeleteTenantEndpoint
{
	public static async Task<IResult> HandleRequestAsync([FromRoute] string tenantName,
		[FromServices] ITenantService service, CancellationToken cancellationToken = default
	)
		=> await service.DeleteTenantAsync(tenantName, cancellationToken) ? Results.NoContent() : Results.NotFound();
}
