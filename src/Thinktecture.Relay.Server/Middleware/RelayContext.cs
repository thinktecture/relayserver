using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Middleware
{
	/// <inheritdoc cref="IRelayContext{TRequest,TResponse}" />
	public class RelayContext<TRequest, TResponse> : IRelayContext<TRequest, TResponse>, IAsyncDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly IConnectionRepository _connectionRepository;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayContext{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="httpContextAccessor">An <see cref="IHttpContextAccessor"/>.</param>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		/// <param name="connectionRepository">An <see cref="IConnectionRepository"/>.</param>
		public RelayContext(IHttpContextAccessor httpContextAccessor, RelayServerContext relayServerContext,
			IConnectionRepository connectionRepository)
		{
			_connectionRepository = connectionRepository ?? throw new ArgumentNullException(nameof(connectionRepository));
			HttpContext = httpContextAccessor?.HttpContext ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			OriginId = relayServerContext?.OriginId ?? throw new ArgumentNullException(nameof(relayServerContext));
		}

		/// <inheritdoc />
		public DateTime RequestStart { get; } = DateTime.UtcNow;

		/// <inheritdoc />
		public Guid RequestId { get; } = Guid.NewGuid();

		/// <inheritdoc />
		public Guid OriginId { get; }

		/// <inheritdoc />
		public TRequest ClientRequest { get; set; }

		/// <inheritdoc />
		public TResponse TargetResponse { get; set; }

		/// <inheritdoc />
		public bool ConnectorAvailable => _connectionRepository.IsConnectionAvailableAsync(ClientRequest.TenantId).GetAwaiter().GetResult();

		/// <inheritdoc />
		public bool ForceConnectorDelivery { get; set; }

		/// <inheritdoc />
		public IList<IAsyncDisposable> ResponseDisposables { get; } = new List<IAsyncDisposable>();

		/// <inheritdoc />
		public HttpContext HttpContext { get; }

		/// <inheritdoc />
		public async ValueTask DisposeAsync()
		{
			foreach (var responseDisposable in ResponseDisposables)
			{
				await responseDisposable.DisposeAsync();
			}
		}
	}
}
