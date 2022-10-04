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

public static partial class EndpointRouteBuilderExtensions
{
	/// <summary>
	/// Maps the endpoint to retrieve a tenant by id.
	/// </summary>
	/// <param name="app">The web application to add the endpoint to.</param>
	/// <param name="pattern">The url pattern for this endpoint.</param>
	/// <param name="policy">Optional; The authorization policy to apply to this endpoint.</param>
	/// <returns>The <see cref="RouteHandlerBuilder"/> with the configured endpoint.</returns>
	public static RouteHandlerBuilder MapGetTenantById(this IEndpointRouteBuilder app, string pattern, string? policy)
	{
		var builder = app
				.MapGet($"{pattern}/{{tenantId:Guid}}", GetTenantEndpoints.HandleRequestByIdAsync)
				.WithName("GetSingleTenantById")
				.WithDisplayName("Get single tenant by id")
				.Produces<Tenant>()
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

	/// <summary>
	/// Maps the endpoint to retrieve a tenant by name.
	/// </summary>
	/// <param name="app">The web application to add the endpoint to.</param>
	/// <param name="pattern">The url pattern for this endpoint.</param>
	/// <param name="policy">Optional; The authorization policy to apply to this endpoint.</param>
	/// <returns>The <see cref="RouteHandlerBuilder"/> with the configured endpoint.</returns>
	public static RouteHandlerBuilder MapGetTenantByName(this IEndpointRouteBuilder app, string pattern, string? policy)
	{
		var builder = app
				.MapGet($"{pattern}/{{tenantName}}", GetTenantEndpoints.HandleRequestByNameAsync)
				.WithName("GetSingleTenantByName")
				.WithDisplayName("Get single tenant by name")
				.Produces<Tenant>()
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
public static class GetTenantEndpoints
{
	/// <summary>
	/// Retrieves a single tenant identified by its id.
	/// </summary>
	/// <param name="tenantId">The id of the tenant to load.</param>
	/// <param name="service">An instance of an <see cref="ITenantService"/> to access the persistence.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <returns>The tenant, if found; otherwise, 404.</returns>
	public static async Task<IResult> HandleRequestByIdAsync(
		[FromRoute] Guid tenantId,
		[FromServices] ITenantService service,
		CancellationToken cancellationToken = default
	)
		=> await service.LoadTenantByIdAsync(tenantId, cancellationToken)
			is { } tenant
			? Results.Ok(tenant.ToModel())
			: Results.NotFound();

	/// <summary>
	/// Retrieves a single tenant identified by its name
	/// </summary>
	/// <param name="tenantName">The name of the tenant to load.</param>
	/// <param name="service">An instance of an <see cref="ITenantService"/> to access the persistence.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <returns>The tenant, if found; otherwise, 404.</returns>
	public static async Task<IResult> HandleRequestByNameAsync(
		[FromRoute] string tenantName,
		[FromServices] ITenantService service,
		CancellationToken cancellationToken = default
	)
		=> await service.LoadTenantByNameAsync(tenantName, cancellationToken)
			is { } tenant
			? Results.Ok(tenant.ToModel())
			: Results.NotFound();
}
