using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Middleware;

/// <inheritdoc cref="IRelayContext{TRequest,TResponse}"/>
public class RelayContext<TRequest, TResponse> : IRelayContext<TRequest, TResponse>, IAsyncDisposable
	where TRequest : IClientRequest
	where TResponse : class, ITargetResponse
{
	private readonly IConnectionService _connectionService;

	/// <inheritdoc />
	public DateTime RequestStart { get; } = DateTime.UtcNow;

	/// <inheritdoc />
	public Guid RequestId { get; } = Guid.NewGuid();

	/// <inheritdoc />
	public Guid OriginId { get; }

	/// <inheritdoc />
	public TRequest ClientRequest { get; set; } = default!;

	/// <inheritdoc />
	public TResponse? TargetResponse { get; set; }

	/// <inheritdoc />
	public bool ConnectorAvailable => _connectionService.IsConnectionAvailableAsync(ClientRequest.TenantName)
		.GetAwaiter().GetResult();

	/// <inheritdoc />
	public bool ForceConnectorDelivery { get; set; }

	/// <inheritdoc />
	public IList<IAsyncDisposable> ResponseDisposables { get; } = new List<IAsyncDisposable>();

	/// <inheritdoc />
	public HttpContext HttpContext { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RelayContext{TRequest,TResponse}"/> class.
	/// </summary>
	/// <param name="httpContextAccessor">An <see cref="IHttpContextAccessor"/>.</param>
	/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
	/// <param name="connectionService">An <see cref="IConnectionService"/>.</param>
	public RelayContext(IHttpContextAccessor httpContextAccessor, RelayServerContext relayServerContext,
		IConnectionService connectionService)
	{
		if (httpContextAccessor == null) throw new ArgumentNullException(nameof(httpContextAccessor));
		if (relayServerContext == null) throw new ArgumentNullException(nameof(relayServerContext));

		_connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
		HttpContext = httpContextAccessor.HttpContext ?? throw new Exception("No HttpContext available.");
		OriginId = relayServerContext.OriginId;
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		foreach (var responseDisposable in ResponseDisposables)
		{
			await responseDisposable.DisposeAsync();
		}
	}
}
