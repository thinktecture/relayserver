using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <inheritdoc />
	public class ResponseDispatcher<TResponse, TAcknowledge> : IResponseDispatcher<TResponse>
		where TResponse : ITargetResponse
		where TAcknowledge : IAcknowledgeRequest
	{
		private readonly ILogger<ResponseDispatcher<TResponse, TAcknowledge>> _logger;
		private readonly IResponseCoordinator<TResponse> _responseCoordinator;
		private readonly RelayServerContext _relayServerContext;
		private readonly IServerTransport<TResponse, TAcknowledge> _serverTransport;
		private readonly RelayServerOptions _relayServerOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="ResponseDispatcher{TResponse,TAcknowledge}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		/// <param name="responseCoordinator">An <see cref="IResponseCoordinator{T}"/>.</param>
		/// <param name="relayServerOptions">An <see cref="IOptions{TOptions}"/>.</param>
		/// <param name="serverTransport">An <see cref="IServerTransport{TResponse,TAcknowledge}"/>.</param>
		public ResponseDispatcher(ILogger<ResponseDispatcher<TResponse, TAcknowledge>> logger, RelayServerContext relayServerContext,
			IResponseCoordinator<TResponse> responseCoordinator, IOptions<RelayServerOptions> relayServerOptions,
			IServerTransport<TResponse, TAcknowledge> serverTransport)
		{
			if (relayServerOptions == null) throw new ArgumentNullException(nameof(relayServerOptions));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_relayServerContext = relayServerContext ?? throw new ArgumentNullException(nameof(relayServerContext));
			_responseCoordinator = responseCoordinator ?? throw new ArgumentNullException(nameof(responseCoordinator));
			_serverTransport = serverTransport ?? throw new ArgumentNullException(nameof(serverTransport));
			_relayServerOptions = relayServerOptions.Value;
		}

		/// <inheritdoc />
		public async Task DispatchAsync(TResponse response, CancellationToken cancellationToken = default)
		{
			if (_relayServerOptions.EnableServerTransportShortcut && response.RequestOriginId == _relayServerContext.OriginId)
			{
				await _responseCoordinator.ProcessResponseAsync(response, cancellationToken);
				return;
			}

			_logger.LogDebug("Redirecting response for request {RequestId} to origin {OriginId}", response.RequestId,
				response.RequestOriginId);
			await _serverTransport.DispatchResponseAsync(response);
		}
	}
}
