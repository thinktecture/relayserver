using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

/// <inheritdoc cref="ITenantTransport{T}"/>
public partial class TenantTransport<TRequest, TAcknowledge> : ITenantTransport<TRequest>, IHostedService, IDisposable
	where TRequest : IClientRequest
	where TAcknowledge : IAcknowledgeRequest
{
	private readonly ILogger _logger;
	private readonly IModel _model;
	private readonly IServiceProvider _serviceProvider;

	/// <inheritdoc />
	public int? BinarySizeThreshold { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TenantTransport{TRequest,TAcknowledge}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
	/// <param name="modelFactory">The <see cref="ModelFactory{TAcknowledge}"/>.</param>
	/// <param name="rabbitMqOptions">An <see cref="IOptions{TOptions}"/>.</param>
	public TenantTransport(ILogger<TenantTransport<TRequest, TAcknowledge>> logger, IServiceProvider serviceProvider,
		ModelFactory<TAcknowledge> modelFactory, IOptions<RabbitMqOptions> rabbitMqOptions)
	{
		if (modelFactory is null) throw new ArgumentNullException(nameof(modelFactory));
		if (rabbitMqOptions is null) throw new ArgumentNullException(nameof(rabbitMqOptions));

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_model = modelFactory.Create("tenant dispatcher");

		BinarySizeThreshold = rabbitMqOptions.Value.MaximumBinarySize;
	}

	/// <inheritdoc />
	public async Task TransportAsync(TRequest request)
	{
		try
		{
			await _model.PublishJsonAsync($"{Constants.RequestQueuePrefix} {request.TenantName}", request);
			Log.PublishedRequest(_logger, request.RequestId, request.TenantName);
		}
		catch (RabbitMQClientException ex)
		{
			Log.ErrorDispatchingRequest(_logger, ex, request.RequestId, request.TenantName);
			throw new TransportException(ex);
		}
	}

	/// <inheritdoc />
	public void Dispose()
		=> _model.Dispose();

	async Task IHostedService.StartAsync(CancellationToken cancellationToken)
	{
		using var scope = _serviceProvider.CreateScope();
		var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();

		var tenantNames = await tenantService.LoadAllTenantNamesAsync(cancellationToken);
		Log.LoadedTenantNames(_logger, tenantNames.Length);

		foreach (var tenantName in tenantNames)
		{
			var queueName = $"{Constants.RequestQueuePrefix} {tenantName}";
			_model.EnsureQueue(queueName);
			Log.EnsuredTenantQueue(_logger, queueName, tenantName);
		}
	}

	Task IHostedService.StopAsync(CancellationToken cancellationToken)
		=> Task.CompletedTask;
}
