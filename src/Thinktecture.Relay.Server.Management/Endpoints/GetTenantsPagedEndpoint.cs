using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Thinktecture.Relay.Server.Management.Extensions;
using Thinktecture.Relay.Server.Management.Models;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Management.Endpoints;

/// <summary>
/// Provides extensions methods for the <see cref="IEndpointRouteBuilder"/>.
/// </summary>
public static partial class EndpointRouteBuilderExtensions
{
	/// <summary>
	/// Maps the endpoint to retrieve the tenants.
	/// </summary>
	/// <param name="app">The web application to add the endpoint to.</param>
	/// <param name="pattern">The url pattern for this endpoint.</param>
	/// <param name="policy">Optional; The authorization policy to apply to this endpoint.</param>
	/// <returns>The <see cref="RouteHandlerBuilder"/> with the configured endpoint.</returns>
	public static RouteHandlerBuilder MapGetTenantsPaged(this IEndpointRouteBuilder app, string pattern, string? policy = null)
	{
		var builder = app
			.MapGet(pattern, GetTenantsPagedEndpoint.HandleRequestAsync)
			.WithName("GetTenantsPaged")
			.WithDisplayName("Get tenants paged")
			.Produces<Persistence.Models.Page<Tenant>>()
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
public static class GetTenantsPagedEndpoint
{
	/// <summary>
	/// Handles the endpoint to retrieve the tenants.
	/// </summary>
	/// <param name="service">An instance of an <see cref="ITenantService"/> to access the persistence layer.</param>
	/// <param name="skip">The amount of tenants to skip while retrieving.</param>
	/// <param name="take">The amount of tenants to fetch per page. Defaults to 10.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <returns>A page containing the requested tenants.</returns>
	public static async Task<Persistence.Models.Page<Tenant>> HandleRequestAsync(
		[FromServices] ITenantService service,
		[FromQuery] int skip = 0,
		[FromQuery] int take = 10,
		CancellationToken cancellationToken = default
	)
		=> (await service.LoadAllTenantsPagedAsync(skip, take, cancellationToken)).ToModel();
}
