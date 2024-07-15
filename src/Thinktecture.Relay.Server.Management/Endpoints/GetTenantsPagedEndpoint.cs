using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Thinktecture.Relay.Server.Management.DataTransferObjects;
using Thinktecture.Relay.Server.Management.Extensions;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Persistence.DataTransferObjects;

namespace Thinktecture.Relay.Server.Management.Endpoints;

/// <summary>
/// Provides extension methods for the <see cref="IEndpointRouteBuilder"/>.
/// </summary>
public static partial class EndpointRouteBuilderExtensions
{
	/// <summary>
	/// Maps the endpoint to get the existing tenants page-wise.
	/// </summary>
	/// <param name="app">An <see cref="IEndpointRouteBuilder"/>.</param>
	/// <param name="pattern">The url pattern for this endpoint.</param>
	/// <param name="policy">An optional authorization policy.</param>
	/// <returns>The <see cref="IEndpointRouteBuilder"/>.</returns>
	public static IEndpointRouteBuilder MapGetTenantsPaged(this IEndpointRouteBuilder app, string pattern,
		string? policy = null)
	{
		var builder = app
				.MapGet(pattern, GetTenantsPagedEndpoint.HandleRequestAsync)
				.WithName("GetTenantsPaged")
				.WithDisplayName("Get tenants paged")
				.Produces<Page<Tenant>>();

		if (!string.IsNullOrWhiteSpace(policy))
		{
			builder.RequireAuthorization(policy)
				.Produces(StatusCodes.Status401Unauthorized)
				.Produces(StatusCodes.Status403Forbidden);
		}

		return app;
	}
}

internal static class GetTenantsPagedEndpoint
{
	public static async Task<Page<Tenant>> HandleRequestAsync([FromServices] ITenantService service,
		[FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default
	)
		=> (await service.LoadAllTenantsPagedAsync(skip, take, cancellationToken)).ToModel();
}
