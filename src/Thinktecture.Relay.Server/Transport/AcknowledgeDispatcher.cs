using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport;

/// <inheritdoc/>
public partial class AcknowledgeDispatcher<TResponse, TAcknowledge> : IAcknowledgeDispatcher<TAcknowledge>
	where TResponse : ITargetResponse
	where TAcknowledge : IAcknowledgeRequest
{
	private readonly IAcknowledgeCoordinator<TAcknowledge> _acknowledgeCoordinator;
	private readonly ILogger<AcknowledgeDispatcher<TResponse, TAcknowledge>> _logger;
	private readonly RelayServerContext _relayServerContext;
	private readonly RelayServerOptions _relayServerOptions;
	private readonly IServerTransport<TResponse, TAcknowledge> _serverTransport;

	/// <summary>
	/// Initializes a new instance of the <see cref="AcknowledgeDispatcher{TResponse,TAcknowledge}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
	/// <param name="acknowledgeCoordinator">An <see cref="IAcknowledgeCoordinator{T}"/>.</param>
	/// <param name="relayServerOptions">An <see cref="IOptions{TOptions}"/>.</param>
	/// <param name="serverTransport">An <see cref="IServerTransport{TResponse,TAcknowledge}"/>.</param>
	public AcknowledgeDispatcher(ILogger<AcknowledgeDispatcher<TResponse, TAcknowledge>> logger,
		RelayServerContext relayServerContext,
		IAcknowledgeCoordinator<TAcknowledge> acknowledgeCoordinator, IOptions<RelayServerOptions> relayServerOptions,
		IServerTransport<TResponse, TAcknowledge> serverTransport)
	{
		if (relayServerOptions == null) throw new ArgumentNullException(nameof(relayServerOptions));

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_relayServerContext = relayServerContext ?? throw new ArgumentNullException(nameof(relayServerContext));
		_acknowledgeCoordinator =
			acknowledgeCoordinator ?? throw new ArgumentNullException(nameof(acknowledgeCoordinator));
		_serverTransport = serverTransport ?? throw new ArgumentNullException(nameof(serverTransport));
		_relayServerOptions = relayServerOptions.Value;
	}

	[LoggerMessage(20900, LogLevel.Trace, "Locally dispatching acknowledge for request {RelayRequestId}")]
	partial void LogLocalAcknowledge(Guid relayRequestId);

	[LoggerMessage(20901, LogLevel.Trace,
		"Remotely dispatching acknowledge for request {RelayRequestId} to origin {OriginId}")]
	partial void LogRedirectAcknowledge(Guid relayRequestId, Guid originId);

	/// <inheritdoc/>
	public async Task DispatchAsync(TAcknowledge request, CancellationToken cancellationToken = default)
	{
		if (_relayServerOptions.EnableServerTransportShortcut && request.OriginId == _relayServerContext.OriginId)
		{
			LogLocalAcknowledge(request.RequestId);
			await _acknowledgeCoordinator.ProcessAcknowledgeAsync(request, cancellationToken);
			return;
		}

		LogRedirectAcknowledge(request.RequestId, request.OriginId);
		await _serverTransport.DispatchAcknowledgeAsync(request);
	}
}
