using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.DbContexts;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore
{
	/// <inheritdoc />
	public class TenantRepository : ITenantRepository
	{
		private readonly RelayServerConfigurationDbContext _dbContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="TenantRepository"/>.
		/// </summary>
		/// <param name="dbContext">The entity framework core database context.</param>
		public TenantRepository(RelayServerConfigurationDbContext dbContext)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		/// <inheritdoc />
		public Task<Tenant> LoadTenantByNameAsync(string name)
		{
			return _dbContext.Tenants
				.Include(t => t.ClientSecrets)
				.AsNoTracking()
				.SingleOrDefaultAsync(t => t.Name == name);
		}
	}
}
