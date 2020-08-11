using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a coordinator for responses.
	/// </summary>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public class ResponseCoordinator<TResponse> : IDisposable
		where TResponse : IRelayTargetResponse
	{
		private readonly IServerHandler<TResponse> _serverHandler;

		private class WaitingState
		{
			public DateTime Creation { get; } = DateTime.UtcNow;
			public TaskCompletionSource<TResponse> TaskCompletionSource { get; } = new TaskCompletionSource<TResponse>();
		}

		private readonly ConcurrentDictionary<Guid, WaitingState> _waitingStates = new ConcurrentDictionary<Guid, WaitingState>();

		/// <summary>
		/// Initializes a new instance of <see cref="ResponseCoordinator{TResponse}"/>.
		/// </summary>
		/// <param name="serverHandler">The <see cref="IServerHandler{TResponse}"/>.</param>
		public ResponseCoordinator(IServerHandler<TResponse> serverHandler)
		{
			_serverHandler = serverHandler ?? throw new ArgumentNullException(nameof(serverHandler));
			serverHandler.ResponseReceived += OnResponseReceived;
		}

		/// <inheritdoc />
		public void Dispose() => _serverHandler.ResponseReceived -= OnResponseReceived;

		private Task OnResponseReceived(object sender, TResponse response)
		{
			var waitingState = _waitingStates.GetOrAdd(response.RequestId, _ => new WaitingState());
			if (!waitingState.TaskCompletionSource.TrySetResult(response))
			{
				_waitingStates.TryRemove(response.RequestId, out _);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets the response for the request.
		/// </summary>
		/// <param name="requestId">The unique id of the request.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the response.</returns>
		public async Task<TResponse> GetResponseAsync(Guid requestId)
		{
			var waitingState = _waitingStates.GetOrAdd(requestId, _ => new WaitingState());
			try
			{
				return await waitingState.TaskCompletionSource.Task;
			}
			finally
			{
				_waitingStates.TryRemove(requestId, out _);
			}
		}
	}
}
