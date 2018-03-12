using System;
using System.Configuration;
using RabbitMQ.Client;
using Serilog;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal class RabbitMqFactory : IRabbitMqFactory
	{
		private readonly IConnectionFactory _factory;
		private readonly IConfiguration _configuration;
		private readonly ILogger _logger;

		public RabbitMqFactory(IConnectionFactory factory, IConfiguration configuration, ILogger logger)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_logger = logger;
		}

		public IConnection CreateConnection()
		{
			var connectionString = _configuration.RabbitMqConnectionString;
			if (connectionString == null)
			{
				_logger?.Fatal("Not connection string found for RabbitMQ. Can not create a bus. Aborting...");
				throw new ConfigurationErrorsException("Could not find a connection string for RabbitMQ. Please add a connection string in the <connectionStrings> section of the application's configuration file. For example: <add name=\"RabbitMQ\" connectionString=\"host=localhost\" />");
			}

			try
			{
				_factory.Uri = new Uri(connectionString);

				if (_configuration.RabbitMqClusterHosts == null)
				{
					_logger?.Verbose("Creating RabbitMQ connection. connection-string={RabbitConnectionString}", _configuration.RabbitMqConnectionString);
					return _factory.CreateConnection();
				}

				_logger?.Verbose("Creating RabbitMQ cluster connection. connection-string={RabbitConnectionString}, cluster-hosts={RabbitClusterHosts}", _configuration.RabbitMqConnectionString, _configuration.RabbitMqConnectionString, _configuration.RabbitMqClusterHosts);
				return _factory.CreateConnection(AmqpTcpEndpoint.ParseMultiple(_configuration.RabbitMqClusterHosts));
			}
			catch (Exception ex)
			{
				_logger?.Fatal(ex, "Cannot connect to RabbitMQ using the provided configuration.");
				throw;
			}
		}
	}
}
