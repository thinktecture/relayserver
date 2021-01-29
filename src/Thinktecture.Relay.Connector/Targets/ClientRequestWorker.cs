using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector.Options;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Targets
{
	/// <inheritdoc cref="IDisposable" />
	public class ClientRequestWorker<TRequest, TResponse> : IClientRequestWorker<TRequest, TResponse>, IDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse, new()
	{
		private readonly ILogger<ClientRequestWorker<TRequest, TResponse>> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly RelayTargetRegistry<TRequest, TResponse> _relayTargetRegistry;
		private readonly IServiceProvider _serviceProvider;
		private readonly IClientRequestHandler<TRequest> _clientRequestHandler;
		private readonly IConnectorTransportLimit _connectorTransportLimit;
		private readonly Uri _requestEndpoint;
		private readonly Uri _responseEndpoint;

		private HttpClient? _httpClient;
		private HttpClient HttpClient => _httpClient ??= _httpClientFactory.CreateClient(Constants.HttpClientNames.RelayServer);

		private class CountingStreamContent : StreamContent
		{
			private readonly int _bufferSize;
			private readonly Stream _content;

			public long BytesWritten { get; private set; }

			public CountingStreamContent(Stream content, int bufferSize = 80 * 1024)
				: base(content, bufferSize)
			{
				_content = content;
				_bufferSize = bufferSize;
			}

			protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
			{
				var buffer = new byte[_bufferSize];

				while (true)
				{
					var length = await _content.ReadAsync(buffer, 0, buffer.Length);
					if (length <= 0)
					{
						break;
					}

					BytesWritten += length;

					await stream.WriteAsync(buffer, 0, length);
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientRequestWorker{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="httpClientFactory">An <see cref="IHttpClientFactory"/>.</param>
		/// <param name="relayConnectorOptions">An <see cref="IOptions{TOptions}"/>.</param>
		/// <param name="relayTargetRegistry">The <see cref="RelayTargetRegistry{TRequest,TResponse}"/>.</param>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		/// <param name="clientRequestHandler">An <see cref="IClientRequestHandler{T}"/>.</param>
		/// <param name="connectorTransportLimit">An <see cref="IConnectorTransportLimit"/>.</param>
		public ClientRequestWorker(ILogger<ClientRequestWorker<TRequest, TResponse>> logger, IHttpClientFactory httpClientFactory,
			IOptions<RelayConnectorOptions> relayConnectorOptions, RelayTargetRegistry<TRequest, TResponse> relayTargetRegistry,
			IServiceProvider serviceProvider, IClientRequestHandler<TRequest> clientRequestHandler,
			IConnectorTransportLimit connectorTransportLimit)
		{
			if (relayConnectorOptions == null) throw new ArgumentNullException(nameof(relayConnectorOptions));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_relayTargetRegistry = relayTargetRegistry ?? throw new ArgumentNullException(nameof(relayTargetRegistry));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_clientRequestHandler = clientRequestHandler ?? throw new ArgumentNullException(nameof(clientRequestHandler));
			_connectorTransportLimit = connectorTransportLimit ?? throw new ArgumentNullException(nameof(connectorTransportLimit));

			_requestEndpoint = new Uri($"{relayConnectorOptions.Value.DiscoveryDocument.RequestEndpoint}/");
			_responseEndpoint = new Uri($"{relayConnectorOptions.Value.DiscoveryDocument.ResponseEndpoint}/");
		}

		/// <inheritdoc />
		public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			using var scope = _serviceProvider.CreateScope();
			CancellationTokenSource? timeout = null;

			try
			{
				IRelayTarget<TRequest, TResponse>? target;

				try
				{
					if (!_relayTargetRegistry.TryCreateRelayTarget(request.Target, scope.ServiceProvider, out target, out timeout))
					{
						_logger.LogInformation("Could not find any target for request {RequestId} named {Target}", request.RequestId,
							request.Target);
						return request.CreateResponse<TResponse>(HttpStatusCode.NotFound);
					}

					_logger.LogTrace("Found target {Target} for request {RequestId}", request.Target, request.RequestId);

					if (request.IsBodyContentOutsourced())
					{
						_logger.LogDebug("Requesting outsourced request body for request {RequestId} with {BodySize} bytes", request.RequestId,
							request.BodySize);

						try
						{
							request.BodyContent = await HttpClient.GetStreamAsync(new Uri(_requestEndpoint,
								$"{request.RequestId:N}?delete={request.AcknowledgeMode == AcknowledgeMode.ConnectorReceived}".ToLowerInvariant()));
						}
						catch (OperationCanceledException)
						{
							throw;
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "An error occured while downloading the body of request {RequestId}", request.RequestId);
							return request.CreateResponse<TResponse>(HttpStatusCode.BadGateway);
						}
					}
				}
				finally
				{
					if (request.AcknowledgeMode == AcknowledgeMode.ConnectorReceived)
					{
						await _clientRequestHandler.AcknowledgeRequestAsync(request, false);
					}
				}

				_logger.LogInformation("Requesting target {Target} for request {RequestId}", request.Target, request.RequestId);

				using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);

				var start = request.EnableTracing ? DateTime.UtcNow : (DateTime?)null;
				var response = await target.HandleAsync(request, cts.Token);

				if (request.EnableTracing)
				{
					response.RequestStart = start;
					response.RequestDuration = DateTime.UtcNow - start;
				}

				if (response.BodyContent == null) return response;

				if (response.BodySize == null ||
					response.BodySize > _connectorTransportLimit.BinarySizeThreshold.GetValueOrDefault(int.MaxValue))
				{
					if (response.BodySize == null)
					{
						_logger.LogWarning("Unknown response body size triggered mandatory outsourcing for request {RequestId}",
							request.RequestId);
					}
					else
					{
						_logger.LogInformation(
							"Outsourcing from response {BodySize} bytes because of a maximum of {BinarySizeThreshold} for request {RequestId}",
							response.BodySize, _connectorTransportLimit.BinarySizeThreshold, request.RequestId);
					}

					using var content = new CountingStreamContent(response.BodyContent);
					try
					{
						var responseMessage = await HttpClient.PostAsync(new Uri(_responseEndpoint, response.RequestId.ToString("N")), content,
							cancellationToken);

						if (!responseMessage.IsSuccessStatusCode)
						{
							_logger.LogError("Uploading body of request {RequestId} failed with http status {HttpStatusCode}", request.RequestId,
								responseMessage.StatusCode);
							return request.CreateResponse<TResponse>(HttpStatusCode.BadGateway);
						}
					}
					catch (OperationCanceledException)
					{
						throw;
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "An error occured while uploading the body of request {RequestId}", request.RequestId);
						return request.CreateResponse<TResponse>(HttpStatusCode.BadGateway);
					}

					response.BodySize = content.BytesWritten;
					response.BodyContent = null;
					_logger.LogDebug("Outsourced from response {BodySize} bytes for request {RequestId}", content.BytesWritten,
						request.RequestId);
				}
				else if (response.BodySize > 0)
				{
					await using var _ = response.BodyContent;
					response.BodyContent = await response.BodyContent.CopyToMemoryStreamAsync(cancellationToken);
					_logger.LogDebug("Inlined from response {BodySize} bytes for request {RequestId}", response.BodySize, request.RequestId);
				}

				return response;
			}
			catch (OperationCanceledException)
			{
				if (timeout == null || !timeout.IsCancellationRequested) throw;

				_logger.LogWarning("The request {RequestId} timed out", request.RequestId);
				return request.CreateResponse<TResponse>(HttpStatusCode.GatewayTimeout);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while processing request {RequestId} {@Request}", request.RequestId, request);
				return request.CreateResponse<TResponse>(HttpStatusCode.BadGateway);
			}
			finally
			{
				if (request.AcknowledgeMode == AcknowledgeMode.ConnectorFinished)
				{
					await _clientRequestHandler.AcknowledgeRequestAsync(request, true);
				}

				timeout?.Cancel();
				timeout?.Dispose();
			}
		}

		/// <inheritdoc />
		public void Dispose() => _httpClient?.Dispose();
	}
}
