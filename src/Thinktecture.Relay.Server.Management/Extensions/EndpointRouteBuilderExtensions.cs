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
		string basePath = ManagementApiConstants.DefaultBasePath)
	{
		var endpointPath = $"{basePath}{ManagementApiConstants.DefaultTenantsPath}";

		app.MapGetTenantsPaged(endpointPath, ManagementApiConstants.DefaultReadPolicyName);
		app.MapGetTenantById(endpointPath, ManagementApiConstants.DefaultReadPolicyName);
		app.MapGetTenantByName(endpointPath, ManagementApiConstants.DefaultReadPolicyName);
		app.MapPostTenant(endpointPath, ManagementApiConstants.DefaultWritePolicyName);
		app.MapPutTenant(endpointPath, ManagementApiConstants.DefaultWritePolicyName);
		app.MapDeleteTenant(endpointPath, ManagementApiConstants.DefaultWritePolicyName);
	}
}
