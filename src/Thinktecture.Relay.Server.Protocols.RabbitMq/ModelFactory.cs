using System;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

/// <summary>
/// An implementation of a factory to create an instance of a class implementing <see cref="IModel"/>.
/// </summary>
public partial class ModelFactory<TAcknowledge>
	where TAcknowledge : IAcknowledgeRequest
{
	private readonly IConnection _connection;
	private readonly IAcknowledgeCoordinator<TAcknowledge> _acknowledgeCoordinator;
	private readonly ILogger _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="ModelFactory{TAcknowledge}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="connection">The <see cref="IConnection"/>.</param>
	/// <param name="acknowledgeCoordinator">An <see cref="IAcknowledgeCoordinator{T}"/></param>
	public ModelFactory(ILogger<ModelFactory<TAcknowledge>> logger, IConnection connection,
		IAcknowledgeCoordinator<TAcknowledge> acknowledgeCoordinator)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_connection = connection;
		_acknowledgeCoordinator =
			acknowledgeCoordinator ?? throw new ArgumentNullException(nameof(acknowledgeCoordinator));

		if (connection is IAutorecoveringConnection autorecoveringConnection)
		{
			autorecoveringConnection.RecoverySucceeded += (_, _) => Log.ConnectionRecovered(_logger);
		}

		connection.ConnectionShutdown += (_, args) => Log.ConnectionClosed(_logger, args.ReplyText);
	}

	/// <summary>
	/// Creates a new instance of <see cref="IModel"/>.
	/// </summary>
	/// <param name="context">A context for identifying the model usage.</param>
	/// <param name="pruneAcknowledgeIds">The model should prune the outstanding acknowledgements in case of a model shutdown.</param>
	/// <returns>An <see cref="IModel"/>.</returns>
	public IModel Create(string context, bool pruneAcknowledgeIds = false)
	{
		// TODO model context information in logging is not yet satisfying
		var model = _connection.CreateModel();
		Log.ModelCreated(_logger, context, model.ChannelNumber);

		model.CallbackException += (_, args)
			=> Log.ModelCallbackError(_logger, args.Exception, context, model.ChannelNumber);

		model.ModelShutdown += (_, args)
			=>
		{
			if (pruneAcknowledgeIds)
			{
				_acknowledgeCoordinator.PruneOutstandingAcknowledgeIds();
			}

			Log.ModelClosed(_logger, context, model.ChannelNumber, args.ReplyText);
		};

		return model;
	}
}
