using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <inheritdoc cref="IResponseCoordinator{TResponse}" />
	public class ResponseCoordinator<TRequest, TResponse> : IResponseCoordinator<TResponse>, IDisposable
		where TRequest : IClientRequest
		where TResponse : class, ITargetResponse
	{
		private readonly ILogger<ResponseCoordinator<TRequest, TResponse>> _logger;
		private readonly IServerHandler<TResponse> _serverHandler;
		private readonly IBodyStore _bodyStore;
		private readonly IServerDispatcher<TResponse> _serverDispatcher;
		private readonly Guid _originId;

		private class WaitingState
		{
			public DateTime Creation { get; } = DateTime.UtcNow;
			public TaskCompletionSource<TResponse> TaskCompletionSource { get; } = new TaskCompletionSource<TResponse>();
		}

		private readonly ConcurrentDictionary<Guid, WaitingState> _waitingStates = new ConcurrentDictionary<Guid, WaitingState>();

		private class ResponseContext : IResponseContext<TResponse>
		{
			public TResponse Response { get; set; }
			public IAsyncDisposable Disposable { get; set; }

			void IResponseContext<TResponse>.Deconstruct(out TResponse response, out IAsyncDisposable disposable)
			{
				response = Response;
				disposable = Disposable;
			}
		}

		/// <summary>
		/// Initializes a new instance of <see cref="ResponseCoordinator{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="serverHandler">The <see cref="IServerHandler{TResponse}"/>.</param>
		/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		/// <param name="serverDispatcher">An <see cref="IServerDispatcher{TResponse}"/>.</param>
		public ResponseCoordinator(ILogger<ResponseCoordinator<TRequest, TResponse>> logger, IServerHandler<TResponse> serverHandler,
			IBodyStore bodyStore, RelayServerContext relayServerContext, IServerDispatcher<TResponse> serverDispatcher)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serverHandler = serverHandler ?? throw new ArgumentNullException(nameof(serverHandler));
			_bodyStore = bodyStore ?? throw new ArgumentNullException(nameof(bodyStore));
			_serverDispatcher = serverDispatcher ?? throw new ArgumentNullException(nameof(serverDispatcher));
			_originId = relayServerContext?.OriginId ?? throw new ArgumentNullException(nameof(relayServerContext));

			serverHandler.ResponseReceived += OnResponseReceived;
		}

		/// <inheritdoc />
		public void Dispose() => _serverHandler.ResponseReceived -= OnResponseReceived;

		private Task OnResponseReceived(object sender, TResponse response) => ProcessResponseAsync(response);

		/// <inheritdoc />
		public async Task<IResponseContext<TResponse>> GetResponseAsync(Guid requestId, CancellationToken cancellationToken = default)
		{
			_logger.LogDebug("Waiting for response for request {RequestId}", requestId);

			var waitingState = _waitingStates.GetOrAdd(requestId, _ => new WaitingState());
			try
			{
				cancellationToken.Register(() => waitingState.TaskCompletionSource.TrySetCanceled());

				var responseContext = new ResponseContext
				{
					Response = await waitingState.TaskCompletionSource.Task
				};

				_logger.LogTrace("Response received {@Response}", responseContext.Response);

				if (responseContext.Response.IsBodyContentOutsourced())
				{
					responseContext.Response.BodyContent = await _bodyStore.OpenResponseBodyAsync(requestId, cancellationToken);
					_logger.LogDebug("Opened outsourced response body for {RequestId} with {BodySize} bytes", requestId,
						responseContext.Response.BodySize);
					responseContext.Disposable = _bodyStore.GetResponseBodyRemoveDisposable(requestId);
				}
				else if (responseContext.Response.BodySize > 0)
				{
					_logger.LogDebug("Response with inlined response body for request {RequestId} received", requestId);
				}
				else
				{
					_logger.LogDebug("Response for request {RequestId} received", requestId);
				}

				return responseContext;
			}
			finally
			{
				_waitingStates.TryRemove(requestId, out _);
			}
		}

		/// <inheritdoc />
		public async Task ProcessResponseAsync(TResponse response)
		{
			if (response.RequestOriginId != _originId)
			{
				_logger.LogDebug("Redirecting response for request {RequestId} to origin {OriginId}", response.RequestId,
					response.RequestOriginId);
				await _serverDispatcher.DispatchResponseAsync(response);
				return;
			}

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
		}
	}
}
