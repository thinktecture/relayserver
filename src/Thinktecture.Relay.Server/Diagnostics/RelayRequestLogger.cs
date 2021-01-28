using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Persistence.Models;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Diagnostics
{
	/// <inheritdoc />
	public class RelayRequestLogger<TRequest, TResponse> : IRelayRequestLogger<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : class, ITargetResponse
	{
		private readonly IRequestRepository _requestRepository;
		private readonly RelayServerOptions _relayServerOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayRequestLogger{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="requestRepository">An <see cref="IRequestRepository"/>.</param>
		/// <param name="relayServerOptions">An <see cref="IOptions{TOptions}"/>.</param>
		public RelayRequestLogger(IRequestRepository requestRepository, IOptions<RelayServerOptions> relayServerOptions)
		{
			if (relayServerOptions == null) throw new ArgumentNullException(nameof(relayServerOptions));

			_requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
			_relayServerOptions = relayServerOptions.Value;
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
				RequestUrl = relayContext.ClientRequest.Url
			};

		/// <inheritdoc />
		public async Task LogSuccessAsync(IRelayContext<TRequest, TResponse> relayContext, CancellationToken cancellationToken = default)
		{
			if (!_relayServerOptions.RequestLoggerLevel.LogSucceeded()) return;

			var request = CreateRequest(relayContext);
			request.HttpStatusCode = relayContext.TargetResponse?.HttpStatusCode;
			request.ResponseBodySize = relayContext.TargetResponse?.BodySize;
			await _requestRepository.StoreRequestAsync(request, cancellationToken);
		}

		/// <inheritdoc />
		public async Task LogAbortAsync(IRelayContext<TRequest, TResponse> relayContext, CancellationToken cancellationToken = default)
		{
			if (!_relayServerOptions.RequestLoggerLevel.LogAborted()) return;

			var request = CreateRequest(relayContext);
			request.Aborted = true;
			await _requestRepository.StoreRequestAsync(request, cancellationToken);
		}

		/// <inheritdoc />
		public async Task LogFailAsync(IRelayContext<TRequest, TResponse> relayContext, CancellationToken cancellationToken = default)
		{
			if (!_relayServerOptions.RequestLoggerLevel.LogFailed()) return;

			var request = CreateRequest(relayContext);
			request.Failed = true;
			await _requestRepository.StoreRequestAsync(request, cancellationToken);
		}

		/// <inheritdoc />
		public async Task LogExpiredAsync(IRelayContext<TRequest, TResponse> relayContext, CancellationToken cancellationToken = default)
		{
			if (!_relayServerOptions.RequestLoggerLevel.LogExpired()) return;

			var request = CreateRequest(relayContext);
			request.Expired = true;
			await _requestRepository.StoreRequestAsync(request, cancellationToken);
		}

		/// <inheritdoc />
		public async Task LogErrorAsync(IRelayContext<TRequest, TResponse> relayContext, CancellationToken cancellationToken = default)
		{
			if (!_relayServerOptions.RequestLoggerLevel.LogErrored()) return;

			var request = CreateRequest(relayContext);
			request.Errored = true;
			await _requestRepository.StoreRequestAsync(request, cancellationToken);
		}
	}
}
