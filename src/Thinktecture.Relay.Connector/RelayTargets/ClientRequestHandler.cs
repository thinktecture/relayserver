using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector.Options;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	/// <inheritdoc cref="IClientRequestHandler{TRequest,TResponse}" />
	public class ClientRequestHandler<TRequest, TResponse> : IClientRequestHandler<TRequest, TResponse>, IDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse, new()
	{
		private readonly ILogger<ClientRequestHandler<TRequest, TResponse>> _logger;
		private readonly RelayTargetRegistry<TRequest, TResponse> _relayTargetRegistry;
		private readonly IServiceProvider _serviceProvider;
		private readonly HttpClient _httpClient;
		private readonly Uri _requestEndpoint;
		private readonly Uri _responseEndpoint;

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
		/// Initializes a new instance of the <see cref="ClientRequestHandler{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="httpClientFactory">An <see cref="IHttpClientFactory"/>.</param>
		/// <param name="relayConnectorOptions">An <see cref="IOptions{TOptions}"/>.</param>
		/// <param name="relayTargetRegistry">The <see cref="RelayTargetRegistry{TRequest,TResponse}"/>.</param>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		public ClientRequestHandler(ILogger<ClientRequestHandler<TRequest, TResponse>> logger, IHttpClientFactory httpClientFactory,
			IOptions<RelayConnectorOptions> relayConnectorOptions, RelayTargetRegistry<TRequest, TResponse> relayTargetRegistry,
			IServiceProvider serviceProvider)
		{
			if (relayConnectorOptions == null) throw new ArgumentNullException(nameof(relayConnectorOptions));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_relayTargetRegistry = relayTargetRegistry ?? throw new ArgumentNullException(nameof(relayTargetRegistry));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

			_httpClient = httpClientFactory?.CreateClient(Constants.RelayServerHttpClientName) ??
				throw new ArgumentNullException(nameof(httpClientFactory));

			_requestEndpoint = new Uri($"{relayConnectorOptions.Value.DiscoveryDocument.RequestEndpoint}/");
			_responseEndpoint = new Uri($"{relayConnectorOptions.Value.DiscoveryDocument.ResponseEndpoint}/");
		}

		/// <inheritdoc />
		public event AsyncEventHandler<IAcknowledgeRequest> Acknowledge;

		/// <inheritdoc />
		public async Task<TResponse> HandleAsync(TRequest request, int? binarySizeThreshold, CancellationToken cancellationToken = default)
		{
			IRelayTarget<TRequest, TResponse> target = null;
			CancellationTokenSource timeout = null;

			try
			{
				try
				{
					if (!_relayTargetRegistry.TryCreateRelayTarget(request.Target, _serviceProvider, out target, out timeout))
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
						request.BodyContent = await _httpClient.GetStreamAsync(new Uri(_requestEndpoint,
							$"{request.RequestId:N}?delete={request.AcknowledgeMode == AcknowledgeMode.ConnectorReceived}".ToLowerInvariant()));

						// TODO error handling when get fails
					}
				}
				finally
				{
					if (request.AcknowledgeMode == AcknowledgeMode.ConnectorReceived)
					{
						await AcknowledgeRequest(request, false);
					}
				}

				_logger.LogInformation("Requesting target {Target} for request {RequestId}", request.Target, request.RequestId);

				using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
				var response = await target.HandleAsync(request, cts.Token);

				if (response.BodySize == null || response.BodySize > binarySizeThreshold)
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
							response.BodySize, binarySizeThreshold, request.RequestId);
					}

					using var content = new CountingStreamContent(response.BodyContent);
					try
					{
						await _httpClient.PostAsync(new Uri(_responseEndpoint, response.RequestId.ToString("N")), content, cancellationToken);
					}
					catch (TaskCanceledException)
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
					using var _ = response.BodyContent;
					response.BodyContent = await response.BodyContent.CopyToMemoryStreamAsync(cancellationToken);
					_logger.LogDebug("Inlined from response {BodySize} bytes for request {RequestId}", response.BodySize, request.RequestId);
				}

				return response;
			}
			catch (TaskCanceledException)
			{
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
					await AcknowledgeRequest(request, true);
				}

				timeout?.Dispose();
				(target as IDisposable)?.Dispose();
			}
		}

		private async Task AcknowledgeRequest(TRequest request, bool removeRequestBodyContent)
		{
			_logger.LogDebug("Acknowledging request {RequestId} on origin {OriginId}", request.RequestId, request.AcknowledgeOriginId);
			await Acknowledge.InvokeAsync(this,
				new AcknowledgeRequest()
				{
					OriginId = request.AcknowledgeOriginId!.Value,
					RequestId = request.RequestId,
					RemoveRequestBodyContent = removeRequestBodyContent
				});
		}

		/// <inheritdoc />
		public void Dispose() => _httpClient.Dispose();
	}
}
