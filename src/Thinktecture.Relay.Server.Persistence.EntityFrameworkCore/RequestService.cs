using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

/// <inheritdoc />
public partial class RequestService : IRequestService
{
	private readonly RelayDbContext _dbContext;
	private readonly ILogger _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="RequestService"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="dbContext">The Entity Framework Core database context.</param>
	public RequestService(ILogger<RequestService> logger, RelayDbContext dbContext)
	{
		_logger = logger;
		_dbContext = dbContext;
	}

	/// <inheritdoc />
	public async Task StoreRequestAsync(Request request)
	{
		Log.StoringRequest(_logger, request);

		try
		{
			_dbContext.Add(request);
			await _dbContext.SaveChangesAsync();
		}
		catch (OperationCanceledException)
		{
			// Ignore this, as this will be thrown when the service shuts down gracefully
		}
		catch (Exception ex)
		{
			Log.ErrorStoringRequest(_logger, ex, request.RequestId);
		}
	}
}
