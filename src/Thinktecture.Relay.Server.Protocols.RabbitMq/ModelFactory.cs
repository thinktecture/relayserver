using System;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

/// <summary>
/// An implementation of a factory to create an instance of a class implementing <see cref="IModel"/>.
/// </summary>
public class ModelFactory
{
	private readonly IConnection _connection;
	private readonly ILogger<ModelFactory> _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="ModelFactory"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="connection">The <see cref="IConnection"/>.</param>
	public ModelFactory(ILogger<ModelFactory> logger, IConnection connection)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_connection = connection;

		if (connection is IAutorecoveringConnection autorecoveringConnection)
		{
			autorecoveringConnection.RecoverySucceeded += (sender, args)
				=> _logger.LogInformation("Connection successful recovered");
		}

		connection.ConnectionShutdown += (sender, args)
			=> _logger.LogDebug("Connection closed ({ShutdownReason}", args.ReplyText);
	}

	/// <summary>
	/// Creates a new instance of <see cref="IModel"/>.
	/// </summary>
	/// <param name="context">A context for identifying the model usage.</param>
	/// <returns>An <see cref="IModel"/>.</returns>
	public IModel Create(string context)
	{
		// TODO model context information in logging is not yet satisfying
		var model = _connection.CreateModel();
		_logger.LogTrace("Model for {ModelContext} with channel {ModelChannel} created", context, model.ChannelNumber);
		model.CallbackException += (sender, args) => _logger.LogError(args.Exception,
			"An error occured in a model callback for {ModelContext} with channel {ModelChannel}", context,
			model.ChannelNumber);
		model.ModelShutdown += (sender, args)
			=> _logger.LogTrace("Model for {ModelContext} with channel {ModelChannel} closed ({ShutdownReason})", context,
				model.ChannelNumber, args.ReplyText);
		return model;
	}
}
