using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.IdentityServer.Stores
{
	/// <summary>
	/// Loads IdentityServer4 <see cref="Client"/> objects from the tenant store.
	/// </summary>
	// ReSharper disable once ClassNeverInstantiated.Global
	public class RelayServerTenantStore : IClientStore
	{
		private readonly ITenantRepository _tenantRepository;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayServerTenantStore"/> class.
		/// </summary>
		public RelayServerTenantStore(ITenantRepository tenantRepository)
			=> _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));

		/// <inheritdoc />
		async Task<Client?> IClientStore.FindClientByIdAsync(string clientId)
		{
			var tenant = await _tenantRepository.LoadTenantByNameAsync(clientId);
			return tenant == null ? null : ConvertToClient(tenant);
		}

		private Client ConvertToClient(Tenant tenant)
			=> new Client()
			{
				ClientId = tenant.Id.ToString(),
				ClientName = tenant.Name,
				Description = tenant.Description,
				ClientSecrets = GetClientSecrets(tenant),
				AllowedGrantTypes = new[] { GrantType.ClientCredentials },
				AllowedScopes = new[] { "relaying" }, // TODO define correct scopes
				Claims = new[]
				{
					new ClientClaim("name", tenant.Name),
					new ClientClaim("display_name", tenant.DisplayName ?? tenant.Name)
				}
				// TODO fill access token lifetime etc. from config
			};

		private ICollection<Secret> GetClientSecrets(Tenant tenant)
			=> tenant.ClientSecrets!
				.Select(secret => new Secret(secret.Value, secret.Expiration))
				.ToArray();
	}
}
