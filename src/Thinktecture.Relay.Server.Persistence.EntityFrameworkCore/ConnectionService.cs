using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

/// <inheritdoc />
public class ConnectionService : IConnectionService
{
	private readonly RelayDbContext _dbContext;

	/// <summary>
	/// Initializes a new instance of the <see cref="ConnectionService"/> class.
	/// </summary>
	/// <param name="dbContext">The Entity Framework Core database context.</param>
	public ConnectionService(RelayDbContext dbContext)
		=> _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

	/// <inheritdoc />
	public Task<bool> IsConnectionAvailableAsync(Guid tenantId)
		=> _dbContext.Connections.AnyAsync(c => c.TenantId == tenantId);
}
