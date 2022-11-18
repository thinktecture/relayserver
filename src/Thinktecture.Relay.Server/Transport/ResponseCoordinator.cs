using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport;

/// <inheritdoc/>
public partial class ResponseCoordinator<T> : IResponseCoordinator<T>
	where T : class, ITargetResponse
{
	private readonly IBodyStore _bodyStore;
	private readonly ILogger<ResponseCoordinator<T>> _logger;

	private readonly Action<ILogger, ITargetResponse, Guid, Exception?> _logResponseReceived =
		LoggerMessage.Define<ITargetResponse, Guid>(LogLevel.Trace, 21407,
			"Response {@Response} for request {RelayRequestId} received");

	private readonly ConcurrentDictionary<Guid, WaitingState> _waitingStates =
		new ConcurrentDictionary<Guid, WaitingState>();

	/// <summary>
	/// Initializes a new instance of the <see cref="ResponseCoordinator{T}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
	public ResponseCoordinator(ILogger<ResponseCoordinator<T>> logger, IBodyStore bodyStore)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_bodyStore = bodyStore ?? throw new ArgumentNullException(nameof(bodyStore));
	}

	/// <inheritdoc/>
	public IAsyncDisposable RegisterRequest(Guid requestId)
	{
		if (_waitingStates.TryAdd(requestId, new WaitingState()))
			return new DisposeAction(() =>
			{
				_waitingStates.TryRemove(requestId, out _);
				return Task.CompletedTask;
			});

		_logger.LogError(21400, "Request {RelayRequestId} is already registered", requestId);
		throw new InvalidOperationException($"Duplicate request registration for request id {requestId}");
	}

	[LoggerMessage(21401, LogLevel.Debug, "Waiting for response for request {RelayRequestId}")]
	partial void LogWaitingForResponse(Guid relayRequestId);

	[LoggerMessage(21404, LogLevel.Debug,
		"Opened outsourced response body for request {RelayRequestId} with {BodySize} bytes")]
	partial void LogBodyOpened(Guid relayRequestId, long? bodySize);

	[LoggerMessage(21405, LogLevel.Debug, "Response with inlined body for request {RelayRequestId} received")]
	partial void LogInlinedReceived(Guid relayRequestId);

	[LoggerMessage(21406, LogLevel.Debug, "Response for request {RelayRequestId} without body received")]
	partial void LogNoBodyReceived(Guid relayRequestId);

	/// <inheritdoc/>
	public async Task<IResponseContext<T>?> GetResponseAsync(Guid requestId,
		CancellationToken cancellationToken = default)
	{
		LogWaitingForResponse(requestId);

		if (!_waitingStates.TryGetValue(requestId, out var waitingState))
		{
			_logger.LogWarning(21402, "No waiting state for request {RelayRequestId} found", requestId);
			return null;
		}

		cancellationToken.Register(() =>
		{
			_logger.LogTrace(21403, "Canceling response wait for request {RelayRequestId}", requestId);
			waitingState.TaskCompletionSource.TrySetCanceled();
		});

		var responseContext = new ResponseContext(await waitingState.TaskCompletionSource.Task);
		if (responseContext.Response.IsBodyContentOutsourced())
		{
			responseContext.Response.BodyContent = await _bodyStore.OpenResponseBodyAsync(requestId, cancellationToken);
			LogBodyOpened(requestId, responseContext.Response.BodySize);
			responseContext.Disposable = _bodyStore.GetResponseBodyRemoveDisposable(requestId);
		}
		else if (responseContext.Response.BodySize > 0)
		{
			LogInlinedReceived(requestId);
		}
		else
		{
			LogNoBodyReceived(requestId);
		}

		return responseContext;
	}

	[LoggerMessage(21408, LogLevel.Debug, "Response for request {RelayRequestId} discarded")]
	partial void LogResponseDiscarded(Guid relayRequestId);

	/// <inheritdoc/>
	public async Task ProcessResponseAsync(T response, CancellationToken cancellationToken = default)
	{
		if (_logger.IsEnabled(LogLevel.Trace))
			_logResponseReceived(_logger, response, response.RequestId, null);

		if (!_waitingStates.TryGetValue(response.RequestId, out var waitingState) ||
		    !waitingState.TaskCompletionSource.TrySetResult(response))
		{
			LogResponseDiscarded(response.RequestId);

			if (response.IsBodyContentOutsourced())
			{
				await _bodyStore.RemoveResponseBodyAsync(response.RequestId, cancellationToken);
			}
		}
	}

	private class ResponseContext : IResponseContext<T>
	{
		public T Response { get; }
		public IAsyncDisposable? Disposable { get; set; }

		public ResponseContext(T response)
			=> Response = response;
	}

	private class WaitingState
	{
		public DateTime Creation { get; } = DateTime.UtcNow;
		public TaskCompletionSource<T> TaskCompletionSource { get; } = new TaskCompletionSource<T>();
	}
}
