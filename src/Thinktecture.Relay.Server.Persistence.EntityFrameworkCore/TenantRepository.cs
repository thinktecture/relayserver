using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore
{
	/// <inheritdoc />
	public class TenantRepository : ITenantRepository
	{
		private readonly RelayDbContext _dbContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="TenantRepository"/> class.
		/// </summary>
		/// <param name="dbContext">The Entity Framework Core database context.</param>
		public TenantRepository(RelayDbContext dbContext) => _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

		/// <inheritdoc />
		public async Task<Tenant?> LoadTenantByNameAsync(string name)
		{
			var normalizedName = name.ToUpperInvariant();

			return await _dbContext.Tenants
				.Include(tenant => tenant.ClientSecrets)
				.AsNoTracking()
				.SingleOrDefaultAsync(tenant => tenant.NormalizedName == normalizedName);
		}

		// TODO: Fix these methods, these are preliminary

		/// <inheritdoc />
		public async Task<Tenant?> LoadTenantByIdAsync(Guid id) => await _dbContext.Tenants
			.Include(t => t.ClientSecrets)
			.AsNoTracking()
			.SingleOrDefaultAsync(t => t.Id == id);

		/// <inheritdoc />
		public IAsyncEnumerable<Tenant> LoadAllTenantsPagedAsync(int skip, int take) => _dbContext.Tenants
			.OrderBy(t => t.NormalizedName)
			.Skip(skip)
			.Take(take)
			.AsNoTracking()
			.AsAsyncEnumerable();

		/// <inheritdoc />
		public async Task<Guid> CreateTenantAsync(Tenant tenant)
		{
			if (tenant.Id == Guid.Empty)
			{
				tenant.Id = Guid.NewGuid();
			}

			tenant.NormalizedName = tenant.Name.ToUpperInvariant();

			// ReSharper disable once MethodHasAsyncOverload
			_dbContext.Tenants.Add(tenant);
			await _dbContext.SaveChangesAsync();

			return tenant.Id;
		}

		/// <inheritdoc />
		public async Task CreateClientSecretAsync(ClientSecret clientSecret)
		{
			// ReSharper disable once MethodHasAsyncOverload
			_dbContext.ClientSecrets.Add(clientSecret);
			await _dbContext.SaveChangesAsync();
		}

		/// <inheritdoc />
		public async Task<bool> DeleteTenantByIdAsync(Guid id)
		{
			var tenant = new Tenant() { Id = id };

			_dbContext.Attach(tenant);
			_dbContext.Tenants.Remove(tenant);

			try
			{
				await _dbContext.SaveChangesAsync();
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <inheritdoc />
		public async Task<Config?> LoadTenantConfigAsync(Guid id) => await _dbContext.Configs
			.AsNoTracking()
			.SingleOrDefaultAsync(c => c.TenantId == id);
	}
}
