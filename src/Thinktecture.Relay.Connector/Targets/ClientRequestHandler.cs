using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector.Options;
using Thinktecture.Relay.Connector.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Targets
{
	/// <inheritdoc />
	public class ClientRequestHandler<TRequest, TResponse, TAcknowledge> : IClientRequestHandler<TRequest>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse, new()
		where TAcknowledge : IAcknowledgeRequest, new()
	{
		private readonly ILogger<ClientRequestHandler<TRequest, TResponse, TAcknowledge>> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly IResponseTransport<TResponse> _responseTransport;
		private readonly IAcknowledgeTransport<TAcknowledge> _acknowledgeTransport;
		private readonly Uri _acknowledgeEndpoint;
		private readonly int _minWorkerThreads;
		private readonly int _maxWorkerThreads;
		private readonly int _maxCompletionPortThreads;

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientRequestHandler{TRequest,TResponse,TAcknowledge}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		/// <param name="relayConnectorOptions">An <see cref="IOptions{TOptions}"/>.</param>
		/// <param name="responseTransport">An <see cref="IResponseTransport{T}"/>.</param>
		/// <param name="acknowledgeTransport">An <see cref="IAcknowledgeTransport{T}"/>.</param>
		public ClientRequestHandler(ILogger<ClientRequestHandler<TRequest, TResponse, TAcknowledge>> logger, IServiceProvider serviceProvider,
			IOptions<RelayConnectorOptions> relayConnectorOptions, IResponseTransport<TResponse> responseTransport,
			IAcknowledgeTransport<TAcknowledge> acknowledgeTransport)
		{
			if (relayConnectorOptions == null) throw new ArgumentNullException(nameof(relayConnectorOptions));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_responseTransport = responseTransport ?? throw new ArgumentNullException(nameof(responseTransport));
			_acknowledgeTransport = acknowledgeTransport ?? throw new ArgumentNullException(nameof(acknowledgeTransport));

			_acknowledgeEndpoint = new Uri($"{relayConnectorOptions.Value.DiscoveryDocument.AcknowledgeEndpoint}/");

			ThreadPool.GetMinThreads(out _minWorkerThreads, out _);
			ThreadPool.GetMaxThreads(out _maxWorkerThreads, out _maxCompletionPortThreads);
		}

		/// <inheritdoc />
		public int? BackgroundTaskLimit
		{
			get
			{
				ThreadPool.GetMaxThreads(out var workerThreads, out _);
				return workerThreads;
			}
			set
			{
				// no lower than minimum worker threads (or system's default fallback)
				var workerThreads = Math.Max(_minWorkerThreads, value ?? _maxWorkerThreads);
				// no lower than processor count
				workerThreads = Math.Max(Environment.ProcessorCount, workerThreads);
				ThreadPool.SetMaxThreads(workerThreads, _maxCompletionPortThreads);
			}
		}

		/// <inheritdoc />
		public async Task HandleAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			if (QueueWorker(request, cancellationToken)) return;

			if (request.AcknowledgeMode == AcknowledgeMode.ConnectorReceived)
			{
				await AcknowledgeRequestAsync(request, false);
			}

			await DeliverResponseAsync(request.CreateResponse<TResponse>(HttpStatusCode.ServiceUnavailable), request.EnableTracing);
		}

		private bool QueueWorker(TRequest request, CancellationToken cancellationToken = default)
			=> ThreadPool.QueueUserWorkItem(_ => WorkerCallAsync(request, cancellationToken).GetAwaiter().GetResult());

		private async Task WorkerCallAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			if (cancellationToken.IsCancellationRequested) return;

			using var scope = _serviceProvider.CreateScope();
			var worker = scope.ServiceProvider.GetRequiredService<IClientRequestWorker<TRequest, TResponse>>();

			if (request.AcknowledgeMode == AcknowledgeMode.Manual)
			{
				var url = new Uri(_acknowledgeEndpoint, $"{request.AcknowledgeOriginId}/{request.RequestId}").ToString();
				request.HttpHeaders[Constants.HeaderNames.AcknowledgeUrl] = new[] { url };
			}

			if (request.EnableTracing)
			{
				request.HttpHeaders[Constants.HeaderNames.RequestId] = new[] { request.RequestId.ToString() };
				request.HttpHeaders[Constants.HeaderNames.OriginId] = new[] { request.RequestOriginId.ToString() };
			}

			try
			{
				var response = await worker.HandleAsync(request, cancellationToken);
				await DeliverResponseAsync(response, request.EnableTracing);
			}
			catch (OperationCanceledException)
			{
				// Ignore this, as this will be thrown when the service shuts down gracefully
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured during handling of request {RequestId}", request.RequestId);
			}
		}

		private async Task DeliverResponseAsync(TResponse response, bool enableTracing)
		{
			_logger.LogDebug("Delivering response for request {RequestId}", response.RequestId);

			if (enableTracing)
			{
				response.HttpHeaders ??= new Dictionary<string, string[]>();
				response.HttpHeaders[Constants.HeaderNames.ConnectorMachineName] = new[] { Environment.MachineName };
				response.HttpHeaders[Constants.HeaderNames.ConnectorVersion] = new[] { RelayConnector.AssemblyVersion };
			}

			await _responseTransport.TransportAsync(response);
		}

		/// <inheritdoc />
		public async Task AcknowledgeRequestAsync(TRequest request, bool removeRequestBodyContent)
		{
			_logger.LogDebug("Acknowledging request {RequestId} on origin {OriginId}", request.RequestId, request.AcknowledgeOriginId);
			await _acknowledgeTransport.TransportAsync(request.CreateAcknowledge<TAcknowledge>(removeRequestBodyContent));
		}
	}
}
