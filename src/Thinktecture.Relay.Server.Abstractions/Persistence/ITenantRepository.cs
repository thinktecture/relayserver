using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence
{
	/// <summary>
	/// Repository that allows access to persisted tenants.
	/// </summary>
	public interface ITenantRepository
	{
		/// <summary>
		/// Loads a <see cref="Tenant"/> by its name.
		/// </summary>
		/// <param name="name">The name of the <see cref="Tenant"/> to load.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="Tenant"/> or null if not found.</returns>
		Task<Tenant?> LoadTenantByNameAsync(string name);

		// TODO: Fix these methods, these are preliminary

		/// <summary>
		/// Loads a <see cref="Tenant"/> by its id.
		/// </summary>
		/// <param name="id">The id of the <see cref="Tenant"/> to load.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="Tenant"/> or null if not found.</returns>
		Task<Tenant?> LoadTenantByIdAsync(Guid id);

		/// <summary>
		/// Loads all <see cref="Tenant"/> with paging.
		/// </summary>
		/// <param name="skip">The amount of entries to skip.</param>
		/// <param name="take">The amount of entries to take.</param>
		/// <returns>An <see cref="IAsyncEnumerable{T}"/> representing the asynchronous enumeration of <see cref="Tenant"/>.</returns>
		IAsyncEnumerable<Tenant> LoadAllTenantsPagedAsync(int skip, int take);

		/// <summary>
		/// Creates a new <see cref="Tenant"/>.
		/// </summary>
		/// <param name="tenant">The <see cref="Tenant"/> to create.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the id of the created tenant.</returns>
		Task<Guid> CreateTenantAsync(Tenant tenant);

		/// <summary>
		/// Creates a client secret for an existing <see cref="Tenant"/>.
		/// </summary>
		/// <param name="clientSecret">The secret to create.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task CreateClientSecretAsync(ClientSecret clientSecret);

		/// <summary>
		/// Deletes a <see cref="Tenant"/>.
		/// </summary>
		/// <param name="id">The unique id of the tenant.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the success of the deletion.</returns>
		Task<bool> DeleteTenantByIdAsync(Guid id);

		/// <summary>
		/// Loads an optional <see cref="Config"/> for a tenant
		/// </summary>
		/// <param name="id">The unique id of the tenant.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="Config"/> or null if not found.</returns>
		Task<Config?> LoadTenantConfigAsync(Guid id);
	}
}
