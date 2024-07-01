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
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Targets;

/// <inheritdoc cref="IDisposable"/>
public partial class ClientRequestWorker<TRequest, TResponse> : IClientRequestWorker<TRequest, TResponse>, IDisposable
	where TRequest : IClientRequest
	where TResponse : ITargetResponse, new()
{
	private readonly IClientRequestHandler<TRequest> _clientRequestHandler;
	private readonly IConnectorTransportLimit _connectorTransportLimit;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger _logger;
	private readonly RelayTargetRegistry<TRequest, TResponse> _relayTargetRegistry;
	private readonly Uri _requestEndpoint;
	private readonly Uri _responseEndpoint;
	private readonly IServiceScopeFactory _serviceProvider;

	private HttpClient? _httpClient;

	private HttpClient HttpClient
		=> _httpClient ??= _httpClientFactory.CreateClient(Constants.HttpClientNames.RelayServer);

	/// <summary>
	/// Initializes a new instance of the <see cref="ClientRequestWorker{TRequest,TResponse}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="httpClientFactory">An <see cref="IHttpClientFactory"/>.</param>
	/// <param name="relayConnectorOptions">An <see cref="IOptions{TOptions}"/>.</param>
	/// <param name="relayTargetRegistry">The <see cref="RelayTargetRegistry{TRequest,TResponse}"/>.</param>
	/// <param name="serviceProvider">An <see cref="IServiceScopeFactory"/>.</param>
	/// <param name="clientRequestHandler">An <see cref="IClientRequestHandler{T}"/>.</param>
	/// <param name="connectorTransportLimit">An <see cref="IConnectorTransportLimit"/>.</param>
	public ClientRequestWorker(ILogger<ClientRequestWorker<TRequest, TResponse>> logger,
		IHttpClientFactory httpClientFactory,
		IOptions<RelayConnectorOptions> relayConnectorOptions,
		RelayTargetRegistry<TRequest, TResponse> relayTargetRegistry,
		IServiceScopeFactory serviceProvider, IClientRequestHandler<TRequest> clientRequestHandler,
		IConnectorTransportLimit connectorTransportLimit)
	{
		if (relayConnectorOptions is null) throw new ArgumentNullException(nameof(relayConnectorOptions));

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		_relayTargetRegistry = relayTargetRegistry ?? throw new ArgumentNullException(nameof(relayTargetRegistry));
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_clientRequestHandler = clientRequestHandler ?? throw new ArgumentNullException(nameof(clientRequestHandler));
		_connectorTransportLimit =
			connectorTransportLimit ?? throw new ArgumentNullException(nameof(connectorTransportLimit));

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
			IRelayTarget? target;

			try
			{
				if (!_relayTargetRegistry.TryCreateRelayTarget(request.Target, scope.ServiceProvider, out target,
					    out timeout))
				{
					Log.NoTargetFound(_logger, request.RequestId, request.Target);
					return request.CreateResponse<TResponse>(HttpStatusCode.NotFound);
				}

				Log.FoundTarget(_logger, request.Target, request.RequestId);

				if (request.IsBodyContentOutsourced())
				{
					Log.RequestingBody(_logger, request.RequestId, request.BodySize);

					try
					{
						request.BodyContent = await HttpClient.GetStreamAsync(new Uri(_requestEndpoint,
							$"{request.RequestId:N}?delete={request.AcknowledgeMode == AcknowledgeMode.ConnectorReceived}"
								.ToLowerInvariant()
						), CancellationToken.None);
					}
					catch (OperationCanceledException)
					{
						throw;
					}
					catch (Exception ex)
					{
						Log.ErrorDownloadingBody(_logger, ex, request.RequestId);
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

			Log.RequestingTarget(_logger, request.Target, request.RequestId);

			using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);

			if (target is IRelayTargetFunc<TRequest, TResponse> func)
				return await GetResponseAsync(func, request, cts.Token);

			await ((IRelayTargetAction<TRequest>)target).HandleAsync(request, cts.Token);
			return request.CreateResponse<TResponse>(HttpStatusCode.NoContent);
		}
		catch (OperationCanceledException)
		{
			if (timeout is null || !timeout.IsCancellationRequested) throw;

			Log.RequestTimedOut(_logger, request.RequestId);

			return request.CreateResponse<TResponse>(HttpStatusCode.GatewayTimeout);
		}
		catch (Exception ex)
		{
			Log.ErrorProcessingRequest(_logger, ex, request.RequestId, request);
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

	private async Task<TResponse> GetResponseAsync(IRelayTargetFunc<TRequest, TResponse> target, TRequest request,
		CancellationToken cancellationToken)
	{
		var start = request.EnableTracing ? DateTime.UtcNow : (DateTime?)null;
		var response = await target.HandleAsync(request, cancellationToken);

		if (request.EnableTracing)
		{
			response.RequestStart = start;
			response.RequestDuration = DateTime.UtcNow - start;
		}

		if (response.BodyContent is null) return response;

		response.BodyContent.TryRewind();

		if (response.BodySize is null ||
			response.BodySize > _connectorTransportLimit.BinarySizeThreshold.GetValueOrDefault(int.MaxValue))
		{
			if (response.BodySize is null)
			{
				Log.OutsourcingUnknownBody(_logger, request.RequestId);
			}
			else
			{
				Log.OutsourcingBody(_logger, response.BodySize, _connectorTransportLimit.BinarySizeThreshold, request.RequestId);
			}

			response.BodyContent.TryRewind();
			using var content = new CountingStreamContent(response.BodyContent);
			try
			{
				var responseMessage = await HttpClient.PostAsync(
					new Uri(_responseEndpoint, response.RequestId.ToString("N")), content,
					cancellationToken);

				if (!responseMessage.IsSuccessStatusCode)
				{
					Log.UploadingBodyFailed(_logger, request.RequestId, responseMessage.StatusCode);
					return request.CreateResponse<TResponse>(HttpStatusCode.BadGateway);
				}
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Log.ErrorUploadingBody(_logger, ex, request.RequestId);
				return request.CreateResponse<TResponse>(HttpStatusCode.BadGateway);
			}

			// This is the original body size, as this is what went to the relay server
			response.OriginalBodySize = content.BytesWritten;
			response.BodySize = response.OriginalBodySize;
			response.BodyContent = null;

			Log.OutsourcedBody(_logger, content.BytesWritten, request.RequestId);
		}
		else if (response.BodySize > 0)
		{
			await using var _ = response.BodyContent;
			response.BodyContent = await response.BodyContent.CopyToMemoryStreamAsync(cancellationToken);
			Log.InlinedBody(_logger, response.BodySize, request.RequestId);
		}

		return response;
	}

	/// <inheritdoc />
	public void Dispose()
		=> _httpClient?.Dispose();

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

		protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
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
}
