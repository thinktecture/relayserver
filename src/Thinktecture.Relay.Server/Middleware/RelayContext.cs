using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Middleware
{
	/// <inheritdoc cref="IRelayContext{TRequest,TResponse}" />
	public class RelayContext<TRequest, TResponse> : IRelayContext<TRequest, TResponse>, IAsyncDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
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
		public async ValueTask DisposeAsync()
		{
			foreach (var responseDisposable in ResponseDisposables)
			{
				await responseDisposable.DisposeAsync();
			}
		}
	}
}
