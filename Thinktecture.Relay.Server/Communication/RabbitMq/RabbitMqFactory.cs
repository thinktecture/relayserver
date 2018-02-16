using System;
using System.Collections.Generic;
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
				_logger?.Verbose("Creating RabbitMQ Bus. connection-string={RabbitConnectionString}", _configuration.RabbitMqConnectionString);

				_factory.Uri = new Uri(connectionString);

				var endpoints = BuildEndpointListFromConfig();
				return endpoints.Count > 0
					? _factory.CreateConnection(endpoints)
					: _factory.CreateConnection();
			}
			catch (Exception ex)
			{
				_logger?.Fatal(ex, "Cannot connect to RabbitMQ using the provided configuration.");
				throw;
			}
		}

		private List<AmqpTcpEndpoint> BuildEndpointListFromConfig()
		{
			var result = new List<AmqpTcpEndpoint>();

			foreach (var host in _configuration.RabbitHosts)
			{
				var parts = host.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length == 1)
				{
					result.Add(new AmqpTcpEndpoint(host));
					_logger?.Information("Adding RabbitMQ host {RabbitMQHost} to available list.", host);
				}
				else if (parts.Length == 2 && Int32.TryParse(parts[1], out var port))
				{
					result.Add(new AmqpTcpEndpoint(parts[0], port));
					_logger?.Information("Adding RabbitMQ host {RabbitMQHost} to available list.", host);
				}
				else
				{
					_logger?.Error("Provided RabbitMQ host {RabbitMQHost} cannot be build into an AmqpTcpEndpoint. Skipping this entry.", host);
				}
			}

			return result;
		}
	}
}
