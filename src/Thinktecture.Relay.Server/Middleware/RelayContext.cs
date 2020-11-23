using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Middleware
{
	/// <inheritdoc cref="IRelayContext{TRequest,TResponse}" />
	public class RelayContext<TRequest, TResponse> : IRelayContext<TRequest, TResponse>, IAsyncDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RelayContext{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="httpContextAccessor">An <see cref="IHttpContextAccessor"/>.</param>
		public RelayContext(IHttpContextAccessor httpContextAccessor) => HttpContext = httpContextAccessor.HttpContext;

		/// <inheritdoc />
		public Guid RequestId { get; } = Guid.NewGuid();

		/// <inheritdoc />
		public TRequest ClientRequest { get; set; }

		/// <inheritdoc />
		public TResponse TargetResponse { get; set; }

		/// <inheritdoc />
		public bool ConnectorAvailable { get; }

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
