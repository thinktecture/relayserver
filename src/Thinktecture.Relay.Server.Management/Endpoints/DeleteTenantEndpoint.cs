using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Management.Endpoints;

public static partial class EndpointRouteBuilderExtensions
{
	/// <summary>
	/// Maps the endpoint to delete an existing tenant.
	/// </summary>
	/// <param name="app">The web application to add the endpoint to.</param>
	/// <param name="pattern">The url pattern for this endpoint.</param>
	/// <param name="policy">Optional; The authorization policy to apply to this endpoint.</param>
	/// <returns>The <see cref="RouteHandlerBuilder"/> with the configured endpoint.</returns>
	public static RouteHandlerBuilder MapDeleteTenant(this IEndpointRouteBuilder app, string pattern, string? policy)
	{
		var builder = app
				.MapDelete($"{pattern}/{{tenantName}}", DeleteTenantEndpoint.HandleRequestAsync)
				.WithName("DeleteTenant")
				.WithDisplayName("Delete tenant")
				.Produces(StatusCodes.Status204NoContent)
				.Produces(StatusCodes.Status404NotFound)
			;

		if (!String.IsNullOrWhiteSpace(policy))
		{
			builder.RequireAuthorization(policy)
				.Produces(StatusCodes.Status401Unauthorized)
				.Produces(StatusCodes.Status403Forbidden)
				;
		}

		return builder;
	}
}

/// <summary>
/// Provides an endpoint handler.
/// </summary>
public static class DeleteTenantEndpoint
{
	/// <summary>
	/// Deletes a tenant from the persistence layer.
	/// </summary>
	/// <param name="tenantName">The name of the tenant to delete.</param>
	/// <param name="service">An instance of an <see cref="ITenantService"/> to access the persistence.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <returns>204, if deleted; otherwise, 404.</returns>
	public static async Task<IResult> HandleRequestAsync([FromRoute] string tenantName,
		[FromServices] ITenantService service, CancellationToken cancellationToken = default
	)
		=> await service.DeleteTenantAsync(tenantName, cancellationToken) ? Results.NoContent() : Results.NotFound();
}
