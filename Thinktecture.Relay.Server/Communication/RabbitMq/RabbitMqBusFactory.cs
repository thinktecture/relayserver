using EasyNetQ;
using EasyNetQ.Loggers;
using NLog.Interface;
using Thinktecture.Relay.Server.Configuration;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal class RabbitMqBusFactory : IRabbitMqBusFactory
	{
		private readonly IConfiguration _configuration;
	    private readonly ILogger _logger;

	    public RabbitMqBusFactory(IConfiguration configuration, ILogger logger)
	    {
	        _configuration = configuration;
	        _logger = logger;
	    }

	    public IBus CreateBus()
		{
            var connectionString = _configuration.RabbitMqConnectionString;
			if (connectionString == null)
			{
                _logger.Fatal("Not connection string found for RabbitMq. Can not create a bus. Aborting...");
				throw new EasyNetQException("Could not find a connection string for RabbitMQ. Please add a connection string in the <connectionStrings> section of the application's configuration file. For example: <add name=\"RabbitMQ\" connectionString=\"host=localhost\" />");
			}

	        _logger.Trace("Creating RabbitMq Bus with connection string {0}", _configuration.RabbitMqConnectionString);
			return RabbitHutch.CreateBus(connectionString, r => r.Register<IEasyNetQLogger>(p => new NullLogger()));
		}
	}
}
