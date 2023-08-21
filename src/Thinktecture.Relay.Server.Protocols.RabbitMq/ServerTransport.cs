using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

/// <inheritdoc cref="IServerTransport{TResponse,TAcknowledge}"/>
public partial class ServerTransport<TResponse, TAcknowledge> : IServerTransport<TResponse, TAcknowledge>, IDisposable
	where TResponse : ITargetResponse
	where TAcknowledge : IAcknowledgeRequest
{
	private readonly IModel _acknowledgeConsumeModel;
	private readonly AsyncEventingBasicConsumer _acknowledgeConsumer;
	private readonly IAcknowledgeCoordinator<TAcknowledge> _acknowledgeCoordinator;
	private readonly IModel _acknowledgeDispatchModel;
	private readonly ILogger<ServerTransport<TResponse, TAcknowledge>> _logger;
	private readonly IModel _responseConsumeModel;
	private readonly AsyncEventingBasicConsumer _responseConsumer;
	private readonly IResponseCoordinator<TResponse> _responseCoordinator;
	private readonly IModel _responseDispatchModel;

	/// <inheritdoc/>
	public int? BinarySizeThreshold { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ServerTransport{TResponse,TAcknowledge}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCatgegory}"/>.</param>
	/// <param name="modelFactory">The <see cref="ModelFactory{TAcknowledge}"/>.</param>
	/// <param name="rabbitMqOptions">An <see cref="IOptions{TOptions}"/>.</param>
	/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
	/// <param name="responseCoordinator">An <see cref="IResponseCoordinator{T}"/>.</param>
	/// <param name="acknowledgeCoordinator">An <see cref="IAcknowledgeCoordinator{T}"/>.</param>
	public ServerTransport(ILogger<ServerTransport<TResponse, TAcknowledge>> logger,
		ModelFactory<TAcknowledge> modelFactory, IOptions<RabbitMqOptions> rabbitMqOptions,
		RelayServerContext relayServerContext, IResponseCoordinator<TResponse> responseCoordinator,
		IAcknowledgeCoordinator<TAcknowledge> acknowledgeCoordinator)
	{
		if (modelFactory == null) throw new ArgumentNullException(nameof(modelFactory));
		if (rabbitMqOptions == null) throw new ArgumentNullException(nameof(rabbitMqOptions));

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_responseCoordinator = responseCoordinator ?? throw new ArgumentNullException(nameof(responseCoordinator));
		_acknowledgeCoordinator =
			acknowledgeCoordinator ?? throw new ArgumentNullException(nameof(acknowledgeCoordinator));

		BinarySizeThreshold = rabbitMqOptions.Value.MaximumBinarySize;

		_responseDispatchModel = modelFactory.Create("response dispatcher");
		_acknowledgeDispatchModel = modelFactory.Create("acknowledge dispatcher");

		_responseConsumeModel = modelFactory.Create("response handler");
		_responseConsumer =
			_responseConsumeModel.ConsumeQueue($"{Constants.ResponseQueuePrefix}{relayServerContext.OriginId}");
		_responseConsumer.Received += ResponseConsumerReceived;

		_acknowledgeConsumeModel = modelFactory.Create("acknowledge handler");
		_acknowledgeConsumer =
			_acknowledgeConsumeModel.ConsumeQueue($"{Constants.AcknowledgeQueuePrefix}{relayServerContext.OriginId}");
		_acknowledgeConsumer.Received += AcknowledgeConsumerReceived;
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		_responseDispatchModel.Dispose();
		_acknowledgeDispatchModel.Dispose();

		_responseConsumer.Received -= ResponseConsumerReceived;
		_acknowledgeConsumer.Received -= AcknowledgeConsumerReceived;

		_responseConsumeModel.CancelConsumerTags(_responseConsumer.ConsumerTags);
		_acknowledgeConsumeModel.CancelConsumerTags(_acknowledgeConsumer.ConsumerTags);

		_responseConsumeModel.Dispose();
		_acknowledgeConsumeModel.Dispose();
	}

	/// <inheritdoc/>
	public async Task DispatchResponseAsync(TResponse response)
	{
		await _responseDispatchModel.PublishJsonAsync($"{Constants.ResponseQueuePrefix}{response.RequestOriginId}",
			response,
			durable: false,
			persistent: false);
		LogDispatchedResponse(response.RequestId, response.RequestOriginId);
	}

	/// <inheritdoc/>
	public async Task DispatchAcknowledgeAsync(TAcknowledge request)
	{
		_logger.LogTrace("Dispatching acknowledge {@AcknowledgeRequest}", request);
		await _acknowledgeDispatchModel.PublishJsonAsync($"{Constants.AcknowledgeQueuePrefix}{request.OriginId}", request,
			durable: false,
			persistent: false);
		LogDispatchedAcknowledge(request.RequestId, request.OriginId);
	}

	// ReSharper disable once PartialMethodWithSinglePart; Justification: Source generator
	[LoggerMessage(25200, LogLevel.Trace, "Dispatched response for request {RelayRequestId} to origin {OriginId}")]
	partial void LogDispatchedResponse(Guid relayRequestId, Guid originId);

	// ReSharper disable once PartialMethodWithSinglePart; Justification: Source generator
	[LoggerMessage(25201, LogLevel.Trace,
		"Dispatched acknowledgement for request {RelayRequestId} to origin {OriginId}")]
	partial void LogDispatchedAcknowledge(Guid relayRequestId, Guid originId);

	// ReSharper disable once PartialMethodWithSinglePart; Justification: Source generator
	[LoggerMessage(25202, LogLevel.Trace,
		"Received response for request {RelayRequestId} from queue {QueueName} by consumer {ConsumerTag}")]
	partial void LogResponseConsumed(Guid relayRequestId, string queueName, string consumerTag);

	private async Task ResponseConsumerReceived(object sender, BasicDeliverEventArgs @event)
	{
		var response = JsonSerializer.Deserialize<TResponse>(@event.Body.Span) ??
			throw new Exception("Could not deserialize response.");
		LogResponseConsumed(response.RequestId, @event.RoutingKey, @event.ConsumerTag);
		await _responseCoordinator.ProcessResponseAsync(response);
	}

	// ReSharper disable once PartialMethodWithSinglePart; Justification: Source generator
	[LoggerMessage(25203, LogLevel.Trace,
		"Received acknowledge for request {RelayRequestId} from queue {QueueName} by consumer {ConsumerTag}")]
	partial void LogAcknowledgeConsumed(Guid relayRequestId, string queueName, string consumerTag);

	private async Task AcknowledgeConsumerReceived(object sender, BasicDeliverEventArgs @event)
	{
		var request = JsonSerializer.Deserialize<TAcknowledge>(@event.Body.Span) ??
			throw new Exception("Could not deserialize acknowledge request.");

		LogAcknowledgeConsumed(request.RequestId, @event.RoutingKey, @event.ConsumerTag);
		await _acknowledgeCoordinator.ProcessAcknowledgeAsync(request);
	}
}
