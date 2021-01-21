using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore
{
	/// <inheritdoc />
	public class RequestRepository : IRequestRepository
	{
		private readonly ILogger<RequestRepository> _logger;
		private readonly RelayDbContext _dbContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="RequestRepository"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="dbContext">The Entity Framework Core database context.</param>
		public RequestRepository(ILogger<RequestRepository> logger, RelayDbContext dbContext)
		{
			_logger = logger;
			_dbContext = dbContext;
		}

		/// <inheritdoc />
		public async Task StoreRequestAsync(Request request, CancellationToken cancellationToken)
		{
			_logger.LogTrace("Storing request {@Request}", request);

			try
			{
				_dbContext.Add(request);
				await _dbContext.SaveChangesAsync(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				// Ignore this, as this will be thrown when the service shuts down gracefully
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while storing request {RequestId}", request.RequestId);
			}
		}
	}
}
