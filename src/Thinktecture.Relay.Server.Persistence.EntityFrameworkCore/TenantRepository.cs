using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.DbContexts;
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
		public TenantRepository(RelayDbContext dbContext)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		/// <inheritdoc />
		public Task<Tenant> LoadTenantByNameAsync(string name)
		{
			var normalizedName = name.ToUpperInvariant();

			return _dbContext.Tenants
				.Include(tenant => tenant.ClientSecrets)
				.AsNoTracking()
				.SingleOrDefaultAsync(tenant => tenant.NormalizedName == normalizedName);
		}

		// TODO: Fix these methods, these are preliminary

		/// <inheritdoc />
		public Task<Tenant> LoadTenantByIdAsync(Guid id)
		{
			return _dbContext.Tenants
				.Include(t => t.ClientSecrets)
				.AsNoTracking()
				.SingleOrDefaultAsync(t => t.Id == id);
		}

		/// <inheritdoc />
		public IAsyncEnumerable<Tenant> LoadAllTenantsPagedAsync(int skip, int take)
		{
			return _dbContext.Tenants
				.AsNoTracking()
				.OrderBy(t => t.NormalizedName)
				.Skip(skip)
				.Take(take)
				.AsAsyncEnumerable();
		}

		/// <inheritdoc />
		public async Task<Guid> CreateTenantAsync(Tenant tenantToCreate)
		{
			if (tenantToCreate.Id == Guid.Empty)
			{
				tenantToCreate.Id = Guid.NewGuid();
			}

			tenantToCreate.NormalizedName = tenantToCreate.Name.ToUpperInvariant();

			await _dbContext.Tenants.AddAsync(tenantToCreate);
			await _dbContext.SaveChangesAsync();

			return tenantToCreate.Id;
		}

		/// <inheritdoc />
		public async Task CreateClientSecretAsync(ClientSecret clientSecret)
		{
			await _dbContext.ClientSecrets.AddAsync(clientSecret);
			await _dbContext.SaveChangesAsync();
		}

		/// <inheritdoc />
		public async Task<bool> DeleteTenantByIdAsync(Guid id)
		{
			// create stub
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
	}
}
