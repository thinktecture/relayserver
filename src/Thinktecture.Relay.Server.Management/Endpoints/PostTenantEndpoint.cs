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
	/// Maps the endpoint to add a new tenant.
	/// </summary>
	/// <param name="app">The web application to add the endpoint to.</param>
	/// <param name="pattern">The url pattern for this endpoint.</param>
	/// <param name="policy">Optional; The authorization policy to apply to this endpoint.</param>
	/// <returns>The <see cref="RouteHandlerBuilder"/> with the configured endpoint.</returns>
	public static RouteHandlerBuilder MapPostTenant(this IEndpointRouteBuilder app, string pattern, string? policy)
	{
		var builder = app
				.MapPost($"{pattern}", PostTenantEndpoint.HandleRequestAsync)
				.WithName("PostTenant")
				.WithDisplayName("Post tenant")
				.Produces(StatusCodes.Status201Created, typeof(IdResult))
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
public static class PostTenantEndpoint
{
	/// <summary>
	/// Stores a tenant to the persistence layer.
	/// </summary>
	/// <param name="service">An instance of an <see cref="ITenantService"/> to access the persistence.</param>
	/// <param name="tenant">The tenant to save.</param>
	/// <param name="request">An instance of an <see cref="HttpRequest"/>.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <returns>The tenant, if found; otherwise, 404.</returns>
	public static async Task<IResult> HandleRequestAsync(
		[FromBody] Tenant tenant,
		[FromServices] ITenantService service,
		HttpRequest request,
		CancellationToken cancellationToken = default
	)
		=> await service.CreateTenantAsync(tenant.ToEntity(), cancellationToken)
			is { } id
			? Results.Created($"{request.Scheme}://{request.Host}{request.Path}/{id}", new IdResult(id))
			: Results.BadRequest();
}
