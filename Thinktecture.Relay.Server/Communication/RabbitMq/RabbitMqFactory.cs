using System;
using System.Configuration;
using NLog;
using RabbitMQ.Client;
using Thinktecture.Relay.Server.Configuration;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal class RabbitMqFactory : IRabbitMqFactory
	{
		private readonly IConnectionFactory _factory;
		private readonly IConfiguration _configuration;
	    private readonly ILogger _logger;

	    public RabbitMqFactory(IConnectionFactory factory, IConfiguration configuration, ILogger logger)
	    {
		    if (factory == null)
			    throw new ArgumentNullException(nameof(factory));
		    if (configuration == null)
			    throw new ArgumentNullException(nameof(configuration));
		    if (logger == null)
			    throw new ArgumentNullException(nameof(logger));
			
		    _factory = factory;
		    _configuration = configuration;
	        _logger = logger;
	    }

	    public IConnection CreateConnection()
		{
            var connectionString = _configuration.RabbitMqConnectionString;
			if (connectionString == null)
			{
                _logger.Fatal("Not connection string found for RabbitMq. Can not create a bus. Aborting...");
				throw new ConfigurationErrorsException("Could not find a connection string for RabbitMQ. Please add a connection string in the <connectionStrings> section of the application's configuration file. For example: <add name=\"RabbitMQ\" connectionString=\"host=localhost\" />");
			}

	        _logger.Trace("Creating RabbitMq Bus with connection string {0}", _configuration.RabbitMqConnectionString);

			return _factory.CreateConnection(connectionString);
		}
	}
}
