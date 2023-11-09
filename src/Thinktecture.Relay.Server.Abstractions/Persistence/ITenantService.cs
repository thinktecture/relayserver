using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.Persistence.DataTransferObjects;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence;

/// <summary>
/// Represents a way to access tenant data in the persistence layer.
/// </summary>
public interface ITenantService
{
	/// <summary>
	/// Loads a <see cref="Tenant"/> with all depending entities (connections, secrets, config).
	/// </summary>
	/// <param name="tenantName">The name of the <see cref="Tenant"/> to load.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <returns>
	/// A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="Tenant"/> or null if
	/// not found.
	/// </returns>
	Task<Tenant?> LoadTenantCompleteAsync(string tenantName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Loads a <see cref="Tenant"/> with some depending entities (secrets, config).
	/// </summary>
	/// <param name="tenantName">The name of the <see cref="Tenant"/> to load.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <returns>
	/// A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="Tenant"/> or null if
	/// not found.
	/// </returns>
	Task<Tenant?> LoadTenantAsync(string tenantName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Loads a <see cref="Tenant"/> together with its connections.
	/// </summary>
	/// <param name="tenantName">The name of the <see cref="Tenant"/> to load.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <returns>
	/// A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="Tenant"/> or null if
	/// not found.
	/// </returns>
	Task<Tenant?> LoadTenantWithConnectionsAsync(string tenantName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Loads a <see cref="Tenant"/> together with its config.
	/// </summary>
	/// <param name="tenantName">The name of the <see cref="Tenant"/> to load.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <returns>
	/// A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="Tenant"/> or null if
	/// not found.
	/// </returns>
	Task<Tenant?> LoadTenantWithConfigAsync(string tenantName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Loads all <see cref="Tenant"/> with paging.
	/// </summary>
	/// <param name="skip">The amount of entries to skip.</param>
	/// <param name="take">The amount of entries to take.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	///</param>
	/// <returns>A <see cref="Page{Tenant}"/> representing the asynchronous loading of a page of <see cref="Tenant"/>s.</returns>
	Task<Page<Tenant>> LoadAllTenantsPagedAsync(int skip, int take, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new <see cref="Tenant"/>.
	/// </summary>
	/// <param name="tenant">The <see cref="Tenant"/> to create.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing <see cref="Tenant"/>.
	/// </summary>
	/// <param name="tenantName">The name of the <see cref="Tenant"/> to update.</param>
	/// <param name="tenant">The <see cref="Tenant"/> with the data to update.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task<bool> UpdateTenantAsync(string tenantName, Tenant tenant, CancellationToken cancellationToken = default);

	// TODO fix these methods, these are preliminary

	/// <summary>
	/// Creates a client secret for an existing <see cref="Tenant"/>.
	/// </summary>
	/// <param name="clientSecret">The secret to create.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task CreateClientSecretAsync(ClientSecret clientSecret, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a <see cref="Tenant"/>.
	/// </summary>
	/// <param name="tenantName">The name of the <see cref="Tenant"/> to delete.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the success of the deletion.</returns>
	Task<bool> DeleteTenantAsync(string tenantName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Loads an optional <see cref="Config"/> for a tenant
	/// </summary>
	/// <param name="tenantName">The name of the <see cref="Tenant"/> to load the config for.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <returns>
	/// A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="Config"/> or null if
	/// not found.
	/// </returns>
	Task<Config?> LoadTenantConfigAsync(string tenantName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Normalizes the name of the tenant.
	/// </summary>
	/// <param name="name">The name of the tenant.</param>
	/// <returns>A <see cref="String"/> representing the normalized name of the tenant.</returns>
	string NormalizeName(string name);
}
