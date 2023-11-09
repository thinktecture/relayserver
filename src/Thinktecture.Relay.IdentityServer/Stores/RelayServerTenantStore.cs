using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.IdentityServer.Stores;

/// <summary>
/// Loads IdentityServer4 <see cref="Client"/> objects from the tenant store.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class RelayServerTenantStore : IClientStore
{
	private readonly ITenantService _tenantService;

	/// <summary>
	/// Initializes a new instance of the <see cref="RelayServerTenantStore"/> class.
	/// </summary>
	public RelayServerTenantStore(ITenantService tenantService)
		=> _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));

	/// <inheritdoc />
	async Task<Client?> IClientStore.FindClientByIdAsync(string clientId)
	{
		var tenant = await _tenantService.LoadTenantCompleteByNameAsync(clientId);
		return tenant == null ? null : ConvertToClient(tenant);
	}

	private Client ConvertToClient(Tenant tenant)
	{
		var claims = new HashSet<ClientClaim>();

		if (tenant.DisplayName != null)
		{
			claims.Add(new ClientClaim("name", tenant.DisplayName));
		}

		if (tenant.Description != null)
		{
			claims.Add(new ClientClaim("description", tenant.Description));
		}

		return new Client()
		{
			ClientId = tenant.Name,
			ClientName = tenant.DisplayName,
			Description = tenant.Description,
			ClientSecrets = GetClientSecrets(tenant),
			AllowedGrantTypes = new[] { GrantType.ClientCredentials },
			AllowedScopes = new[] { "connector" },
			Claims = claims,
			// TODO fill access token lifetime etc. from config
		};
	}

	private ICollection<Secret> GetClientSecrets(Tenant tenant)
		=> tenant.ClientSecrets!
			.Select(secret => new Secret(secret.Value, secret.Expiration))
			.ToArray();
}
