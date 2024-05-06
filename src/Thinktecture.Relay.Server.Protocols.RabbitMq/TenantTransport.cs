using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

/// <inheritdoc cref="ITenantTransport{T}"/>
public partial class TenantTransport<TRequest, TAcknowledge> : ITenantTransport<TRequest>, IDisposable
	where TRequest : IClientRequest
	where TAcknowledge : IAcknowledgeRequest
{
	private readonly ILogger<TenantTransport<TRequest, TAcknowledge>> _logger;
	private readonly IModel _model;

	/// <inheritdoc />
	public int? BinarySizeThreshold { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TenantTransport{TRequest,TAcknowledge}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCatgeory}"/>.</param>
	/// <param name="modelFactory">The <see cref="ModelFactory{TAcknowledge}"/>.</param>
	/// <param name="rabbitMqOptions">An <see cref="IOptions{TOptions}"/>.</param>
	public TenantTransport(ILogger<TenantTransport<TRequest, TAcknowledge>> logger, ModelFactory<TAcknowledge> modelFactory,
		IOptions<RabbitMqOptions> rabbitMqOptions)
	{
		if (modelFactory is null) throw new ArgumentNullException(nameof(modelFactory));
		if (rabbitMqOptions is null) throw new ArgumentNullException(nameof(rabbitMqOptions));

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));

		BinarySizeThreshold = rabbitMqOptions.Value.MaximumBinarySize;

		_model = modelFactory.Create("tenant dispatcher");
	}

	// ReSharper disable once PartialMethodWithSinglePart; Justification: Source generator
	[LoggerMessage(25400, LogLevel.Trace, "Published request {RelayRequestId} to tenant {TenantName}")]
	partial void LogPublishedRequest(Guid relayRequestId, string tenantName);

	/// <inheritdoc />
	public async Task TransportAsync(TRequest request)
	{
		try
		{
			await _model.PublishJsonAsync($"{Constants.RequestQueuePrefix} {request.TenantName}", request, autoDelete: false);
			LogPublishedRequest(request.RequestId, request.TenantName);
		}
		catch (RabbitMQClientException ex)
		{
			_logger.LogError(25401, ex,
				"An error occured while dispatching request {RelayRequestId} to tenant {TenantName} queue",
				request.RequestId, request.TenantName);
			throw new TransportException(ex);
		}
	}

	/// <inheritdoc />
	public void Dispose()
		=> _model.Dispose();
}
