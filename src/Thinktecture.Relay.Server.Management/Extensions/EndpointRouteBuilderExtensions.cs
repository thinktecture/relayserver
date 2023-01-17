using Microsoft.AspNetCore.Routing;
using Thinktecture.Relay.Server.Management.Endpoints;

namespace Thinktecture.Relay.Server.Management.Extensions;

/// <summary>
/// Provides extensions methods for the <see cref="IEndpointRouteBuilder"/>.
/// </summary>
public static class EndpointRouteBuilderExtensions
{
	/// <summary>
	/// Adds the RelayServer management api endpoints with an optional path.
	/// </summary>
	/// <param name="app">The web application to register the endpoints with.</param>
	/// <param name="basePath">The base path under which the endpoints should be mapped.
	/// Defaults to /api/management</param>
	public static void UseRelayServerManagementEndpoints(this IEndpointRouteBuilder app,
		string basePath = "/api/management")
	{
		var endpointPath = $"{basePath}/tenants";

		app.MapGetTenantsPaged(endpointPath, ManagementApiPolicyNames.Read);
		app.MapGetTenantById(endpointPath, ManagementApiPolicyNames.Read);
		app.MapGetTenantByName(endpointPath, ManagementApiPolicyNames.Read);
		app.MapPostTenant(endpointPath, ManagementApiPolicyNames.Write);
		app.MapPutTenant(endpointPath, ManagementApiPolicyNames.Write);
		app.MapDeleteTenant(endpointPath, ManagementApiPolicyNames.Write);
	}
}
