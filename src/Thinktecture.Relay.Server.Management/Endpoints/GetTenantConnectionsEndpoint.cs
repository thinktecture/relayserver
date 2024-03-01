using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Thinktecture.Relay.Server.Management.DataTransferObjects;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Management.Endpoints;

public static partial class EndpointRouteBuilderExtensions
{
	/// <summary>
	/// Maps the endpoint to retrieve the connections of a tenant.
	/// </summary>
	/// <param name="app">The web application to add the endpoint to.</param>
	/// <param name="pattern">The url pattern for this endpoint.</param>
	/// <param name="policy">Optional; The authorization policy to apply to this endpoint.</param>
	/// <returns>The <see cref="RouteHandlerBuilder"/> with the configured endpoint.</returns>
	public static RouteHandlerBuilder MapGetTenantConnections(this IEndpointRouteBuilder app, string pattern,
		string? policy)
	{
		var builder = app
			.MapGet($"{pattern}/{{tenantName}}/connections", GetTenantConnectionsEndpoint.HandleRequest)
			.WithName("GetTenantConnections")
			.WithDisplayName("Get connections of a tenant")
			.Produces<Tenant>()
			.Produces(StatusCodes.Status404NotFound);

		if (!string.IsNullOrWhiteSpace(policy))
		{
			builder.RequireAuthorization(policy)
				.Produces(StatusCodes.Status401Unauthorized)
				.Produces(StatusCodes.Status403Forbidden);
		}

		return builder;
	}
}

/// <summary>
/// Provides an endpoint handler.
/// </summary>
public static class GetTenantConnectionsEndpoint
{
	/// <summary>
	/// Retrieves connections of a tenant.
	/// </summary>
	/// <param name="tenantName">The name of the tenant to load.</param>
	/// <param name="service">An instance of an <see cref="ITenantService"/> to access the persistence.</param>
	/// <returns>The connections, if the tenant exists; otherwise, 404.</returns>
	public static IResult HandleRequest([FromRoute] string tenantName, [FromServices] ITenantService service)
		=> service.LoadTenantWithConnections(tenantName) is { } tenant
			? Results.Ok(tenant.Connections)
			: Results.NotFound();
}
