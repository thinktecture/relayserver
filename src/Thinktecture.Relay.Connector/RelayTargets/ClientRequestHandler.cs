using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	internal class ClientRequestHandler<TRequest, TResponse> : IClientRequestHandler<TRequest, TResponse>, IDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly Dictionary<string, RelayTargetRegistration<TRequest, TResponse>> _targets
			= new Dictionary<string, RelayTargetRegistration<TRequest, TResponse>>(StringComparer.InvariantCultureIgnoreCase);

		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<ClientRequestHandler<TRequest, TResponse>> _logger;
		private readonly HttpClient _httpClient;
		private readonly Uri _requestEndpoint;
		private readonly Uri _responseEndpoint;

		private class CountingStreamContent : StreamContent
		{
			private readonly int _bufferSize;
			private readonly Stream _content;

			public long BytesWritten { get; private set; }

			public CountingStreamContent(Stream content, int bufferSize = 80 * 1024) : base(content, bufferSize)
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

		public ClientRequestHandler(IServiceProvider serviceProvider, IEnumerable<RelayTargetRegistration<TRequest, TResponse>> targets,
			IHttpClientFactory httpClientFactory, IOptions<RelayConnectorOptions> options,
			ILogger<ClientRequestHandler<TRequest, TResponse>> logger)
		{
			if (httpClientFactory == null)
			{
				throw new ArgumentNullException(nameof(httpClientFactory));
			}

			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClient = httpClientFactory.CreateClient(Constants.RelayServerHttpClientName);
			_requestEndpoint = new Uri($"{options.Value.DiscoveryDocument.RequestEndpoint}/");
			_responseEndpoint = new Uri($"{options.Value.DiscoveryDocument.ResponseEndpoint}/");

			foreach (var target in targets)
			{
				_targets[target.Id] = target;
			}
		}

		public event AsyncEventHandler<IAcknowledgeRequest> Acknowledge;

		public async Task<TResponse> HandleAsync(TRequest request, int? binarySizeThreshold, CancellationToken cancellationToken = default)
		{
			IRelayTarget<TRequest, TResponse> target;

			try
			{
				if (!TryGetTarget(request.Target, out target) && !TryGetTarget(Constants.RelayTargetCatchAllId, out target))
				{
					_logger.LogError("Could not find any target for request {RequestId} named {Target}", request.RequestId, request.Target);
					return default;
				}

				_logger.LogTrace("Found target {Target} for request {RequestId} as {TargetClass}", request.Target, request.RequestId,
					target.GetType().Name);

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

			try
			{
				_logger.LogInformation("Requesting target {Target} for request {RequestId}", request.Target, request.RequestId);
				var response = await target.HandleAsync(request, cancellationToken);

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
					await _httpClient.PostAsync(new Uri(_responseEndpoint, response.RequestId.ToString("N")), content, cancellationToken);

					// TODO error handling when post fails

					response.BodySize = content.BytesWritten;
					response.BodyContent = Stream.Null; // stream was disposed by stream content already - no need to keep it
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
			finally
			{
				if (request.AcknowledgeMode == AcknowledgeMode.ConnectorFinished)
				{
					await AcknowledgeRequest(request, true);
				}

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

		private bool TryGetTarget(string id, out IRelayTarget<TRequest, TResponse> target)
		{
			if (_targets.TryGetValue(id, out var registration))
			{
				target = registration.Factory(_serviceProvider);
				return true;
			}

			target = null;
			return false;
		}

		public void Dispose() => _httpClient.Dispose();
	}
}
