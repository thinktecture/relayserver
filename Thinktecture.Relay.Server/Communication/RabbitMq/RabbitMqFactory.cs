using System;
using System.Configuration;
using Serilog;
using RabbitMQ.Client;
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

			_logger?.Verbose("Creating RabbitMQ Bus. connection-string={0}", _configuration.RabbitMqConnectionString);

			return _factory.CreateConnection(connectionString);
		}
	}
}
