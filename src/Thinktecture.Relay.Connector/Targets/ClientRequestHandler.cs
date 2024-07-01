using System;
using System.Collections.Generic;
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
	where TRequest : IClientRequest
	where TResponse : ITargetResponse, new()
	where TAcknowledge : IAcknowledgeRequest, new()
{
	private readonly Uri _acknowledgeEndpoint;
	private readonly IAcknowledgeTransport<TAcknowledge> _acknowledgeTransport;
	private readonly ILogger _logger;
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
		if (relayConnectorOptions is null) throw new ArgumentNullException(nameof(relayConnectorOptions));

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_responseTransport = responseTransport ?? throw new ArgumentNullException(nameof(responseTransport));
		_acknowledgeTransport = acknowledgeTransport ?? throw new ArgumentNullException(nameof(acknowledgeTransport));

		_acknowledgeEndpoint = new Uri($"{relayConnectorOptions.Value.DiscoveryDocument.AcknowledgeEndpoint}/");
	}

	/// <inheritdoc />
	public Task HandleAsync(TRequest request, CancellationToken cancellationToken = default)
	{
		_ = WorkerCallAsync(request, cancellationToken);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public async Task AcknowledgeRequestAsync(TRequest request, bool removeRequestBodyContent)
	{
		Log.AcknowledgeRequest(_logger, request.RequestId, request.AcknowledgeOriginId);
		await _acknowledgeTransport.TransportAsync(request.CreateAcknowledge<TAcknowledge>(removeRequestBodyContent));
	}

	private async Task WorkerCallAsync(TRequest request, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested) return;

		using var scope = _serviceProvider.CreateScope();
		var worker = scope.ServiceProvider.GetRequiredService<IClientRequestWorker<TRequest, TResponse>>();

		if (request.AcknowledgeMode == AcknowledgeMode.Manual)
		{
			var url = new Uri(_acknowledgeEndpoint, $"{request.AcknowledgeOriginId}/{request.RequestId}").ToString();
			request.HttpHeaders[Constants.HeaderNames.AcknowledgeUrl] = [url];
		}

		if (request.EnableTracing)
		{
			request.HttpHeaders[Constants.HeaderNames.RequestId] = [request.RequestId.ToString()];
			request.HttpHeaders[Constants.HeaderNames.RequestOriginId] = [request.RequestOriginId.ToString()];
		}

		try
		{
			var response = await worker.HandleAsync(request, cancellationToken);
			if (request.DiscardConnectorResponse)
			{
				Log.DiscardResponse(_logger, request.RequestId);
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
			Log.ErrorHandlingRequest(_logger, ex, request.RequestId);
		}
	}

	private async Task DeliverResponseAsync(TRequest request, TResponse response)
	{
		Log.DeliverResponse(_logger, response.RequestId);

		if (request.EnableTracing)
		{
			response.HttpHeaders ??= new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

			response.HttpHeaders[Constants.HeaderNames.ConnectorMachineName] = [Environment.MachineName];
			response.HttpHeaders[Constants.HeaderNames.ConnectorVersion] = [GetType().GetAssemblyVersion()];
			response.HttpHeaders[Constants.HeaderNames.RequestId] = [request.RequestId.ToString()];
			response.HttpHeaders[Constants.HeaderNames.RequestOriginId] = [request.RequestOriginId.ToString()];

			if (request.AcknowledgeOriginId.HasValue)
			{
				response.HttpHeaders[Constants.HeaderNames.AcknowledgeOriginId] =
					[request.AcknowledgeOriginId.Value.ToString()];
			}
			if (response.RequestStart.HasValue)
			{
				response.HttpHeaders[Constants.HeaderNames.TargetStart] = [response.RequestStart.Value.ToString("R")];
			}
			if (response.RequestDuration.HasValue)
			{
				response.HttpHeaders[Constants.HeaderNames.TargetDuration] = [response.RequestDuration.Value.ToString("g")];
			}
		}

		await _responseTransport.TransportAsync(response);
	}
}
