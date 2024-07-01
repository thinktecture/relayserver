using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thinktecture.Relay.Server.Persistence.DataTransferObjects;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

/// <inheritdoc />
public class TenantService : ITenantService
{
	private readonly RelayDbContext _dbContext;

	/// <summary>
	/// Initializes a new instance of the <see cref="TenantService"/> class.
	/// </summary>
	/// <param name="dbContext">The Entity Framework Core database context.</param>
	public TenantService(RelayDbContext dbContext)
		=> _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

	/// <inheritdoc />
	public async Task<Tenant?> LoadTenantCompleteAsync(string tenantName, CancellationToken cancellationToken)
	{
		var normalizedName = NormalizeName(tenantName);

		return await _dbContext.Tenants
			.Include(t => t.ClientSecrets)
			.Include(t => t.Connections)
			.Include(t => t.Config)
			.AsNoTracking()
			.SingleOrDefaultAsync(t => t.NormalizedName == normalizedName, cancellationToken: cancellationToken);
	}

	/// <inheritdoc />
	public async Task<Tenant?> LoadTenantAsync(string tenantName, CancellationToken cancellationToken)
	{
		var normalizedName = NormalizeName(tenantName);

		return await _dbContext.Tenants
			.Include(t => t.ClientSecrets)
			.Include(t => t.Config)
			.AsNoTracking()
			.SingleOrDefaultAsync(t => t.NormalizedName == normalizedName, cancellationToken: cancellationToken);
	}

	/// <inheritdoc />
	public Tenant? LoadTenantWithConnections(string tenantName)
	{
		var normalizedName = NormalizeName(tenantName);

		return _dbContext.Tenants
			.Include(t => t.Connections)
			.AsNoTracking()
			.SingleOrDefault(t => t.NormalizedName == normalizedName);
	}

	/// <inheritdoc />
	public async Task<Tenant?> LoadTenantWithConfigAsync(string tenantName,
		CancellationToken cancellationToken = default)
	{
		var normalizedName = NormalizeName(tenantName);

		return await _dbContext.Tenants
			.Include(t => t.Config)
			.AsNoTracking()
			.SingleOrDefaultAsync(t => t.NormalizedName == normalizedName, cancellationToken: cancellationToken);
	}

	/// <inheritdoc />
	public async Task<Page<Tenant>> LoadAllTenantsPagedAsync(int skip, int take, CancellationToken cancellationToken)
		=> await _dbContext.Tenants
			.Include(t => t.Config)
			.Include(t => t.ClientSecrets)
			.AsNoTracking()
			.OrderBy(t => t.NormalizedName)
			.ToPagedResultAsync(skip, take, cancellationToken);

	/// <inheritdoc />
	public async Task CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken)
	{
		tenant.NormalizedName = NormalizeName(tenant.Name);

		if (await _dbContext.Tenants.AnyAsync(t => t.NormalizedName == tenant.NormalizedName, cancellationToken))
			throw new InvalidOperationException($"Tenant {tenant.Name} does already exist and cannot be created.");

		var newTenant = new Tenant()
		{
			Name = tenant.Name,
			NormalizedName = tenant.NormalizedName,
		};

		newTenant.UpdateFrom(tenant);

		// ReSharper disable once MethodHasAsyncOverload
		_dbContext.Tenants.Add(newTenant);
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	/// <inheritdoc />
	public async Task<bool> UpdateTenantAsync(string tenantName, Tenant tenant, CancellationToken cancellationToken)
	{
		tenant.NormalizedName = NormalizeName(tenantName);

		var existingTenant = await _dbContext.Tenants
			.Include(t => t.Config)
			.Include(t => t.ClientSecrets)
			.SingleOrDefaultAsync(t => t.NormalizedName == tenant.NormalizedName, cancellationToken: cancellationToken);

		if (existingTenant is null) return false;

		existingTenant.UpdateFrom(tenant);
		await _dbContext.SaveChangesAsync(cancellationToken);

		return true;
	}

	/// <inheritdoc />
	public async Task<bool> DeleteTenantAsync(string tenantName, CancellationToken cancellationToken)
	{
		var tenant = new Tenant() { NormalizedName = NormalizeName(tenantName) };

		_dbContext.Attach(tenant);
		_dbContext.Tenants.Remove(tenant);

		try
		{
			await _dbContext.SaveChangesAsync(cancellationToken);
			return true;
		}
		catch (DbUpdateConcurrencyException)
		{
			return false;
		}
	}

	/// <inheritdoc />
	public async Task<Config?> LoadTenantConfigAsync(string tenantName, CancellationToken cancellationToken)
	{
		var normalizedName = NormalizeName(tenantName);

		return await _dbContext.Configs
			.AsNoTracking()
			.SingleOrDefaultAsync(c => c.TenantName == normalizedName, cancellationToken: cancellationToken);
	}

	/// <inheritdoc />
	public async Task CreateClientSecretAsync(ClientSecret clientSecret, CancellationToken cancellationToken)
	{
		if (String.IsNullOrWhiteSpace(clientSecret.Value))
			throw new InvalidOperationException("Client secret needs a value.");

		if (_dbContext.ClientSecrets.Any(cs => cs.Id == clientSecret.Id))
			throw new InvalidOperationException(
				$"Client secret with id {clientSecret.Id} does already exist and cannot be created.");

		var normalizeName = NormalizeName(clientSecret.TenantName);

		if (!_dbContext.Tenants.Any(t => t.NormalizedName == normalizeName))
			throw new InvalidOperationException(
				$"Client secret cannot be created because tenant {clientSecret.TenantName} does not exist.");

		if (clientSecret.Id == Guid.Empty)
		{
			clientSecret.Id = Guid.NewGuid();
		}

		var newSecret = new ClientSecret() { Id = clientSecret.Id, TenantName = normalizeName, };
		newSecret.UpdateFrom(clientSecret);

		_dbContext.ClientSecrets.Add(newSecret);
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	/// <inheritdoc />
	public string NormalizeName(string name)
		=> name.ToUpperInvariant();
}
