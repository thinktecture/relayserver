using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport;

/// <inheritdoc />
public partial class ResponseCoordinator<T> : IResponseCoordinator<T>
	where T : class, ITargetResponse
{
	private readonly IBodyStore _bodyStore;
	private readonly ILogger _logger;

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

	/// <inheritdoc />
	public IAsyncDisposable RegisterRequest(Guid requestId)
	{
		if (_waitingStates.TryAdd(requestId, new WaitingState()))
			return new DisposeAction(() =>
			{
				_waitingStates.TryRemove(requestId, out _);
				return Task.CompletedTask;
			});

		Log.RequestAlreadyRegistered(_logger, requestId);
		throw new InvalidOperationException($"Duplicate request registration for request id {requestId}");
	}

	/// <inheritdoc />
	public async Task<IResponseContext<T>?> GetResponseAsync(Guid requestId,
		CancellationToken cancellationToken = default)
	{
		Log.WaitingForResponse(_logger, requestId);

		if (!_waitingStates.TryGetValue(requestId, out var waitingState))
		{
			Log.NoWaitingStateFound(_logger, requestId);
			return null;
		}

		cancellationToken.Register(() =>
		{
			Log.CancelingWait(_logger, requestId);
			waitingState.TaskCompletionSource.TrySetCanceled();
		});

		var responseContext = new ResponseContext(await waitingState.TaskCompletionSource.Task);
		if (responseContext.Response.IsBodyContentOutsourced())
		{
			responseContext.Response.BodyContent = await _bodyStore.OpenResponseBodyAsync(requestId, cancellationToken);
			Log.BodyOpened(_logger, requestId, responseContext.Response.BodySize);
			responseContext.Disposable = _bodyStore.GetResponseBodyRemoveDisposable(requestId);
		}
		else if (responseContext.Response.BodySize > 0)
		{
			Log.InlinedReceived(_logger, requestId);
		}
		else
		{
			Log.NoBodyReceived(_logger, requestId);
		}

		return responseContext;
	}

	/// <inheritdoc />
	public async Task ProcessResponseAsync(T response, CancellationToken cancellationToken = default)
	{
		Log.ResponseReceived(_logger, response, response.RequestId);

		if (!_waitingStates.TryGetValue(response.RequestId, out var waitingState) ||
		    !waitingState.TaskCompletionSource.TrySetResult(response))
		{
			Log.ResponseDiscarded(_logger, response.RequestId);

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
		public TaskCompletionSource<T> TaskCompletionSource { get; } = new TaskCompletionSource<T>();
	}
}
