using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a coordinator for responses.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public class ResponseCoordinator<TRequest, TResponse> : IDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly IServerHandler<TResponse> _serverHandler;
		private readonly IBodyStore _bodyStore;
		private readonly ILogger<ResponseCoordinator<TRequest, TResponse>> _logger;

		private class WaitingState
		{
			public DateTime Creation { get; } = DateTime.UtcNow;
			public TaskCompletionSource<TResponse> TaskCompletionSource { get; } = new TaskCompletionSource<TResponse>();
		}

		private readonly ConcurrentDictionary<Guid, WaitingState> _waitingStates = new ConcurrentDictionary<Guid, WaitingState>();

		/// <summary>
		/// Initializes a new instance of <see cref="ResponseCoordinator{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="serverHandler">The <see cref="IServerHandler{TResponse}"/>.</param>
		/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		public ResponseCoordinator(IServerHandler<TResponse> serverHandler, IBodyStore bodyStore,
			ILogger<ResponseCoordinator<TRequest, TResponse>> logger)
		{
			_serverHandler = serverHandler ?? throw new ArgumentNullException(nameof(serverHandler));
			_bodyStore = bodyStore ?? throw new ArgumentNullException(nameof(bodyStore));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			serverHandler.ResponseReceived += OnResponseReceived;
		}

		/// <inheritdoc />
		public void Dispose() => _serverHandler.ResponseReceived -= OnResponseReceived;

		private Task OnResponseReceived(object sender, TResponse response)
		{
			var waitingState = _waitingStates.GetOrAdd(response.RequestId, _ => new WaitingState());
			if (waitingState.TaskCompletionSource.TrySetResult(response))
			{
				_logger.LogDebug("Response for request {RequestId} received", response.RequestId);
				_logger.LogTrace("Response received {@Response}", response);
			}
			else
			{
				_logger.LogDebug("Response for request {RequestId} ignored", response.RequestId);
				_waitingStates.TryRemove(response.RequestId, out _);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets the response for the request.
		/// </summary>
		/// <param name="relayContext">An <see cref="IRelayContext{TRequest,TResponse}"/>.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="ITargetResponse"/>.</returns>
		public async Task<TResponse> GetResponseAsync(IRelayContext<TRequest, TResponse> relayContext,
			CancellationToken cancellationToken = default)
		{
			_logger.LogDebug("Waiting for response for request {RequestId}", relayContext.RequestId);

			var waitingState = _waitingStates.GetOrAdd(relayContext.RequestId, _ => new WaitingState());
			try
			{
				cancellationToken.Register(() => waitingState.TaskCompletionSource.TrySetCanceled());

				var response = await waitingState.TaskCompletionSource.Task;

				_logger.LogTrace("Response received {@Response}", response);

				if (response.BodySize > 0 && response.BodyContent == null)
				{
					response.BodyContent = await _bodyStore.OpenResponseBodyAsync(relayContext.RequestId, cancellationToken);
					_logger.LogDebug("Opened outsourced response body for {RequestId} with {BodySize} bytes", relayContext.RequestId,
						response.BodySize);
					relayContext.ResponseDisposable = _bodyStore.GetResponseRemoveDisposable(relayContext.RequestId);
				}
				else if (response.BodySize > 0)
				{
					_logger.LogDebug("Response with inlined response body for request {RequestId} received", relayContext.RequestId);
				}
				else
				{
					_logger.LogDebug("Response for request {RequestId} received", relayContext.RequestId);
				}

				return response;
			}
			finally
			{
				_waitingStates.TryRemove(relayContext.RequestId, out _);
			}
		}
	}
}
