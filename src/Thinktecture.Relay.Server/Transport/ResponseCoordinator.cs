using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <inheritdoc />
	public class ResponseCoordinator<T> : IResponseCoordinator<T>
		where T : class, ITargetResponse
	{
		private readonly ILogger<ResponseCoordinator<T>> _logger;
		private readonly IBodyStore _bodyStore;

		private class WaitingState
		{
			public DateTime Creation { get; } = DateTime.UtcNow;
			public TaskCompletionSource<T> TaskCompletionSource { get; } = new TaskCompletionSource<T>();
		}

		private readonly ConcurrentDictionary<Guid, WaitingState> _waitingStates = new ConcurrentDictionary<Guid, WaitingState>();

		private class ResponseContext : IResponseContext<T>
		{
			public ResponseContext(T response) => Response = response;

			public T Response { get; }
			public IAsyncDisposable? Disposable { get; set; }
		}

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

			_logger.LogError("Request {RequestId} is already registered", requestId);
			throw new InvalidOperationException($"Duplicate request registration for {requestId}");
		}

		/// <inheritdoc />
		public async Task<IResponseContext<T>?> GetResponseAsync(Guid requestId, CancellationToken cancellationToken = default)
		{
			_logger.LogDebug("Waiting for response for request {RequestId}", requestId);

			if (!_waitingStates.TryGetValue(requestId, out var waitingState))
			{
				_logger.LogWarning("No waiting state for request {RequestId} found", requestId);
				return null;
			}

			cancellationToken.Register(() =>
			{
				_logger.LogTrace("Canceling response wait for request {RequestId}", requestId);
				waitingState.TaskCompletionSource.TrySetCanceled();
			});

			var responseContext = new ResponseContext(await waitingState.TaskCompletionSource.Task);
			if (responseContext.Response.IsBodyContentOutsourced())
			{
				responseContext.Response.BodyContent = await _bodyStore.OpenResponseBodyAsync(requestId, cancellationToken);
				_logger.LogDebug("Opened outsourced response body for request {RequestId} with {BodySize} bytes", requestId,
					responseContext.Response.BodySize);
				responseContext.Disposable = _bodyStore.GetResponseBodyRemoveDisposable(requestId);
			}
			else if (responseContext.Response.BodySize > 0)
			{
				_logger.LogDebug("Response with inlined body for request {RequestId} received", requestId);
			}
			else
			{
				_logger.LogDebug("Response for request {RequestId} without body received", requestId);
			}

			return responseContext;
		}

		/// <inheritdoc />
		public async Task ProcessResponseAsync(T response, CancellationToken cancellationToken = default)
		{
			_logger.LogTrace("Response {@Response} for request {RequestId} received", response, response.RequestId);

			if (!_waitingStates.TryGetValue(response.RequestId, out var waitingState) ||
				!waitingState.TaskCompletionSource.TrySetResult(response))
			{
				_logger.LogDebug("Response for request {RequestId} discarded", response.RequestId);

				if (response.IsBodyContentOutsourced())
				{
					await _bodyStore.RemoveResponseBodyAsync(response.RequestId, cancellationToken);
				}
			}
		}
	}
}
