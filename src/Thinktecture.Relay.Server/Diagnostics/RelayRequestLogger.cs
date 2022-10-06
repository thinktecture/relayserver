using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Persistence.Models;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Diagnostics;

/// <inheritdoc/>
public partial class RelayRequestLogger<TRequest, TResponse> : IRelayRequestLogger<TRequest, TResponse>
	where TRequest : IClientRequest
	where TResponse : class, ITargetResponse
{
	private readonly ILogger<RelayRequestLogger<TRequest, TResponse>> _logger;
	private readonly RelayServerOptions _relayServerOptions;
	private readonly IRequestService _requestService;

	/// <summary>
	/// Initializes a new instance of the <see cref="RelayRequestLogger{TRequest,TResponse}"/> class.
	/// </summary>
	/// <param name="logger">An instance of an <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="requestService">An <see cref="IRequestService"/>.</param>
	/// <param name="relayServerOptions">An <see cref="IOptions{TOptions}"/>.</param>
	public RelayRequestLogger(ILogger<RelayRequestLogger<TRequest, TResponse>> logger,
		IRequestService requestService, IOptions<RelayServerOptions> relayServerOptions)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		if (relayServerOptions == null) throw new ArgumentNullException(nameof(relayServerOptions));

		_requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
		_relayServerOptions = relayServerOptions.Value;
	}

	[LoggerMessage(20400, LogLevel.Trace, "Writing request log for successful request {RequestId}")]
	partial void LogSuccess(Guid requestId);

	/// <inheritdoc/>
	public async Task LogSuccessAsync(IRelayContext<TRequest, TResponse> relayContext)
	{
		if (!_relayServerOptions.RequestLoggerLevel.LogSucceeded()) return;

		LogSuccess(relayContext.RequestId);

		var request = CreateRequest(relayContext);
		request.HttpStatusCode = relayContext.TargetResponse?.HttpStatusCode;
		request.ResponseBodySize = relayContext.TargetResponse?.BodySize;
		await _requestService.StoreRequestAsync(request);
	}

	[LoggerMessage(20401, LogLevel.Trace, "Writing request log for aborted request {RequestId}")]
	partial void LogAbort(Guid requestId);

	/// <inheritdoc/>
	public async Task LogAbortAsync(IRelayContext<TRequest, TResponse> relayContext)
	{
		if (!_relayServerOptions.RequestLoggerLevel.LogAborted()) return;

		LogAbort(relayContext.RequestId);

		var request = CreateRequest(relayContext);
		request.Aborted = true;
		await _requestService.StoreRequestAsync(request);
	}

	[LoggerMessage(20402, LogLevel.Trace, "Writing request log for failed request {RequestId}")]
	partial void LogFail(Guid requestId);

	/// <inheritdoc/>
	public async Task LogFailAsync(IRelayContext<TRequest, TResponse> relayContext)
	{
		if (!_relayServerOptions.RequestLoggerLevel.LogFailed()) return;

		LogFail(relayContext.RequestId);

		var request = CreateRequest(relayContext);
		request.Failed = true;
		await _requestService.StoreRequestAsync(request);
	}

	[LoggerMessage(20403, LogLevel.Trace, "Writing request log for expired request {RequestId}")]
	partial void LogExpired(Guid requestId);

	/// <inheritdoc/>
	public async Task LogExpiredAsync(IRelayContext<TRequest, TResponse> relayContext)
	{
		if (!_relayServerOptions.RequestLoggerLevel.LogExpired()) return;

		LogExpired(relayContext.RequestId);

		var request = CreateRequest(relayContext);
		request.Expired = true;
		await _requestService.StoreRequestAsync(request);
	}

	[LoggerMessage(20404, LogLevel.Trace, "Writing request log for error on request {RequestId}")]
	partial void LogError(Guid requestId);

	/// <inheritdoc/>
	public async Task LogErrorAsync(IRelayContext<TRequest, TResponse> relayContext)
	{
		if (!_relayServerOptions.RequestLoggerLevel.LogErrored()) return;

		LogError(relayContext.RequestId);

		var request = CreateRequest(relayContext);
		request.Errored = true;
		await _requestService.StoreRequestAsync(request);
	}

	private Request CreateRequest(IRelayContext<TRequest, TResponse> relayContext)
		=> new Request()
		{
			TenantId = relayContext.ClientRequest.TenantId,
			RequestId = relayContext.RequestId,
			RequestDate = relayContext.RequestStart,
			RequestDuration = (long)(DateTime.UtcNow - relayContext.RequestStart).TotalMilliseconds,
			RequestBodySize = relayContext.ClientRequest.BodySize.GetValueOrDefault(),
			Target = relayContext.ClientRequest.Target,
			HttpMethod = relayContext.ClientRequest.HttpMethod,
			RequestUrl = relayContext.ClientRequest.Url,
		};
}
