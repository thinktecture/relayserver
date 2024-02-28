using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Targets;

/// <inheritdoc />
public partial class ClientRequestHandler<TRequest, TResponse, TAcknowledge> : IClientRequestHandler<TRequest>
// ReSharper disable RedundantNameQualifier; (this is needed in 6.0, see https://github.com/dotnet/runtime/issues/58550)
	where TRequest : Thinktecture.Relay.Transport.IClientRequest
	where TResponse : Thinktecture.Relay.Transport.ITargetResponse, new()
	where TAcknowledge : Thinktecture.Relay.Acknowledgement.IAcknowledgeRequest, new()
// ReSharper restore RedundantNameQualifier
{
	private readonly Uri _acknowledgeEndpoint;
	private readonly IAcknowledgeTransport<TAcknowledge> _acknowledgeTransport;
	private readonly ILogger<ClientRequestHandler<TRequest, TResponse, TAcknowledge>> _logger;
	private readonly IResponseTransport<TResponse> _responseTransport;
	private readonly IServiceScopeFactory _serviceProvider;

	/// <summary>
	/// Initializes a new instance of the <see cref="ClientRequestHandler{TRequest,TResponse,TAcknowledge}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="serviceProvider">An <see cref="IServiceScopeFactory"/>.</param>
	/// <param name="relayConnectorOptions">An <see cref="IOptions{TOptions}"/>.</param>
	/// <param name="responseTransport">An <see cref="IResponseTransport{T}"/>.</param>
	/// <param name="acknowledgeTransport">An <see cref="IAcknowledgeTransport{T}"/>.</param>
	public ClientRequestHandler(ILogger<ClientRequestHandler<TRequest, TResponse, TAcknowledge>> logger,
		IServiceScopeFactory serviceProvider,
		IOptions<RelayConnectorOptions> relayConnectorOptions, IResponseTransport<TResponse> responseTransport,
		IAcknowledgeTransport<TAcknowledge> acknowledgeTransport)
	{
		if (relayConnectorOptions == null) throw new ArgumentNullException(nameof(relayConnectorOptions));

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_responseTransport = responseTransport ?? throw new ArgumentNullException(nameof(responseTransport));
		_acknowledgeTransport = acknowledgeTransport ?? throw new ArgumentNullException(nameof(acknowledgeTransport));

		_acknowledgeEndpoint = new Uri($"{relayConnectorOptions.Value.DiscoveryDocument.AcknowledgeEndpoint}/");
	}

	/// <inheritdoc />
	public async Task HandleAsync(TRequest request, CancellationToken cancellationToken = default)
	{
		if (QueueWorker(request, cancellationToken)) return;

		// we could not queue the work item

		if (request.AcknowledgeMode == AcknowledgeMode.ConnectorReceived)
		{
			await AcknowledgeRequestAsync(request, false);
		}

		await DeliverResponseAsync(request, request.CreateResponse<TResponse>(HttpStatusCode.ServiceUnavailable));
	}

	[LoggerMessage(10400, LogLevel.Debug, "Acknowledging request {RelayRequestId} on origin {OriginId}")]
	partial void LogAcknowledgeRequest(Guid relayRequestId, Guid? originId);

	[LoggerMessage(10403, LogLevel.Debug, "Discarding response for request {RelayRequestId}")]
	partial void LogDiscardResponse(Guid relayRequestId);

	/// <inheritdoc />
	public async Task AcknowledgeRequestAsync(TRequest request, bool removeRequestBodyContent)
	{
		LogAcknowledgeRequest(request.RequestId, request.AcknowledgeOriginId);
		await _acknowledgeTransport.TransportAsync(request.CreateAcknowledge<TAcknowledge>(removeRequestBodyContent));
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
			request.HttpHeaders[Constants.HeaderNames.RequestOriginId] = new[] { request.RequestOriginId.ToString() };
		}

		try
		{
			var response = await worker.HandleAsync(request, cancellationToken);
			if (request.DiscardConnectorResponse)
			{
				LogDiscardResponse(request.RequestId);
				return;
			}

			await DeliverResponseAsync(request, response);
		}
		catch (OperationCanceledException)
		{
			// Ignore this, as this will be thrown when the service shuts down gracefully
		}
		catch (Exception ex)
		{
			_logger.LogError(10401, ex, "An error occured during handling of request {RelayRequestId}", request.RequestId);
		}
	}

	[LoggerMessage(10402, LogLevel.Debug, "Delivering response for request {RelayRequestId}")]
	partial void LogDeliverResponse(Guid relayRequestId);

	private async Task DeliverResponseAsync(TRequest request, TResponse response)
	{
		LogDeliverResponse(response.RequestId);

		if (request.EnableTracing)
		{
			response.HttpHeaders ??= new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

			response.HttpHeaders[Constants.HeaderNames.ConnectorMachineName] = new[] { Environment.MachineName };
			response.HttpHeaders[Constants.HeaderNames.ConnectorVersion] = new[] { Constants.AssemblyVersion };
			response.HttpHeaders[Constants.HeaderNames.RequestId] = new[] { request.RequestId.ToString() };
			response.HttpHeaders[Constants.HeaderNames.RequestOriginId] = new[] { request.RequestOriginId.ToString() };

			if (request.AcknowledgeOriginId.HasValue)
			{
				response.HttpHeaders[Constants.HeaderNames.AcknowledgeOriginId] =
					new[] { request.AcknowledgeOriginId.Value.ToString() };
			}
			if (response.RequestStart.HasValue)
			{
				response.HttpHeaders[Constants.HeaderNames.TargetStart] =
					new[] { response.RequestStart.Value.ToString("R") };
			}
			if (response.RequestDuration.HasValue)
			{
				response.HttpHeaders[Constants.HeaderNames.TargetDuration] =
					new[] { response.RequestDuration.Value.ToString("g") };
			}
		}

		await _responseTransport.TransportAsync(response);
	}
}
