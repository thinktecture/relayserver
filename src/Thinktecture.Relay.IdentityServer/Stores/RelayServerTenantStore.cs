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
	/// Loads IdentityServer4 <see cref="Client"/> objects from the RelayServer Tenant store.
	/// </summary>
	public class RelayServerTenantStore : IClientStore
	{
		private readonly ITenantRepository _tenantRepository;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayServerTenantStore"/>.
		/// </summary>
		public RelayServerTenantStore(ITenantRepository tenantRepository)
		{
			_tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
		}

		/// <inheritdoc />
		async Task<Client> IClientStore.FindClientByIdAsync(string clientId)
		{
			var tenant = await _tenantRepository.LoadTenantByNameAsync(clientId);
			if (tenant == null)
			{
				return null;
			}

			return ConvertToClient(tenant);
		}

		private Client ConvertToClient(Tenant tenant)
		{
			var client = new Client()
			{
				ClientId = tenant.Name,
				Description = tenant.Description,
				ClientName = tenant.DisplayName,
				ClientSecrets = ConvertToClientSecrets(tenant.ClientSecrets).ToArray(),
				AllowedGrantTypes = new []
				{
					GrantType.ClientCredentials
				},
				AllowedScopes = new []
				{
					// TODO: Define correct scopes
					"relaying",
				},
				// TODO: Fill access token lifetime etc. from config
			};

			return client;
		}

		private IEnumerable<Secret> ConvertToClientSecrets(IEnumerable<ClientSecret> tenantClientSecrets)
		{
			foreach (var tenantClientSecret in tenantClientSecrets)
			{
				yield return new Secret()
				{
					Value = tenantClientSecret.Value,
					Expiration = tenantClientSecret.Expiration,
				};
			}
		}
	}
}
