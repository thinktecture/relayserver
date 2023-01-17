using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thinktecture.Relay.Server.Persistence.DataTransferObjects;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

/// <inheritdoc/>
public class TenantService : ITenantService
{
	private readonly RelayDbContext _dbContext;

	/// <summary>
	/// Initializes a new instance of the <see cref="TenantService"/> class.
	/// </summary>
	/// <param name="dbContext">The Entity Framework Core database context.</param>
	public TenantService(RelayDbContext dbContext)
		=> _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

	/// <inheritdoc/>
	public async Task<Tenant?> LoadTenantByNameAsync(string name, CancellationToken cancellationToken)
	{
		var normalizedName = NormalizeName(name);

		return await _dbContext.Tenants
			.Include(tenant => tenant.ClientSecrets)
			.AsNoTracking()
			.SingleOrDefaultAsync(tenant => tenant.NormalizedName == normalizedName, cancellationToken: cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<Page<Tenant>> LoadAllTenantsPagedAsync(int skip, int take, CancellationToken cancellationToken)
		=> await _dbContext.Tenants
			.Include(t => t.Config)
			.Include(t => t.ClientSecrets)
			.AsNoTracking()
			.OrderBy(t => t.NormalizedName)
			.ToPagedResultAsync(skip, take, cancellationToken);

	/// <inheritdoc/>
	public async Task<Tenant?> LoadTenantByIdAsync(Guid id, CancellationToken cancellationToken)
		=> await _dbContext.Tenants
			.Include(t => t.Config)
			.Include(t => t.ClientSecrets)
			.AsNoTracking()
			.SingleOrDefaultAsync(t => t.Id == id, cancellationToken: cancellationToken);

	/// <inheritdoc/>
	public async Task<Guid> CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken)
	{
		if (await _dbContext.Tenants.AnyAsync(t => t.Id == tenant.Id, cancellationToken))
		{
			throw new InvalidOperationException(
				$"Tenant with id {tenant.Id} does already exist and cannot be created.");
		}

		if (tenant.Id == Guid.Empty)
		{
			tenant.Id = Guid.NewGuid();
		}

		var newTenant = new Tenant()
		{
			Id = tenant.Id,
		};

		tenant.NormalizedName = NormalizeName(tenant.Name);
		newTenant.UpdateFrom(tenant);

		// ReSharper disable once MethodHasAsyncOverload
		_dbContext.Tenants.Add(newTenant);
		await _dbContext.SaveChangesAsync(cancellationToken);

		return newTenant.Id;
	}

	/// <inheritdoc/>
	public async Task<bool> UpdateTenantAsync(Guid tenantId, Tenant tenant, CancellationToken cancellationToken)
	{
		var existingTenant = await _dbContext.Tenants
			.Include(t => t.Config)
			.Include(t => t.ClientSecrets)
			.SingleOrDefaultAsync(t => t.Id == tenantId, cancellationToken: cancellationToken);

		if (existingTenant == null)
		{
			return false;
		}

		if (tenant.Id == Guid.Empty)
		{
			tenant.Id = tenantId;
		}

		tenant.NormalizedName = tenant.Name.ToUpperInvariant();
		existingTenant.UpdateFrom(tenant);
		await _dbContext.SaveChangesAsync(cancellationToken);

		return true;
	}

	/// <inheritdoc/>
	public async Task<bool> DeleteTenantByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		var tenant = new Tenant() { Id = id, };

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

	/// <inheritdoc/>
	public async Task<Config?> LoadTenantConfigAsync(Guid id, CancellationToken cancellationToken)
		=> await _dbContext.Configs
			.AsNoTracking()
			.SingleOrDefaultAsync(c => c.TenantId == id, cancellationToken: cancellationToken);

	/// <inheritdoc/>
	public async Task CreateClientSecretAsync(ClientSecret clientSecret, CancellationToken cancellationToken)
	{
		if (String.IsNullOrWhiteSpace(clientSecret.Value))
		{
			throw new InvalidOperationException($"Client secret needs a value.");
		}

		if (_dbContext.ClientSecrets.Any(cs => cs.Id == clientSecret.Id))
		{
			throw new InvalidOperationException(
				$"Client secret with id {clientSecret.Id} does already exist and cannot be created.");
		}

		if (!_dbContext.Tenants.Any(t => t.Id == clientSecret.TenantId))
		{
			throw new InvalidOperationException(
				$"Client secret cannot be created because tenant with id {clientSecret.TenantId} does not exist.");
		}

		if (clientSecret.Id == Guid.Empty)
		{
			clientSecret.Id = Guid.NewGuid();
		}

		var newSecret = new ClientSecret() { Id = clientSecret.Id, TenantId = clientSecret.TenantId, };
		newSecret.UpdateFrom(clientSecret);

		_dbContext.ClientSecrets.Add(newSecret);
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	/// <inheritdoc/>
	public string NormalizeName(string name)
		=> name.ToUpperInvariant();
}
