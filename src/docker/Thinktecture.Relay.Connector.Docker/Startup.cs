using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Connector.Targets;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Docker;

public static class Startup
{
	// This method gets called by the runtime. Use this method to add services to the container.
	public static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
	{
		var configuration = hostBuilderContext.Configuration;

		services
			.AddRelayConnector(options => configuration.GetSection("RelayConnector").Bind(options))
			.AddSignalRConnectorTransport()
			.AddTarget<InProcTarget>("inproc");

		services.AddHostedService<ConnectorService>();
	}
}

internal class InProcTarget : IRelayTarget<ClientRequest, TargetResponse>
{
	private ILogger _logger;

	public InProcTarget(ILogger<InProcTarget> logger)
		=> _logger = logger;

	public async Task<TargetResponse> HandleAsync(ClientRequest request, CancellationToken cancellationToken = default)
	{
		_logger.LogInformation(1, "Executing demo in proc target for request {RequestId}",
			request.RequestId);

		var responseData = new MemoryStream(Encoding.UTF8.GetBytes("{ \"Hello\" : \"World 👋\" }"));

		if (request.BodyContent != null)
		{
			_logger.LogInformation(2, "Request {RequestId} provided {BodySize} bytes in body",
				request.RequestId, request.BodySize);

			// if echo is requested, return the received content
			if (request.HttpHeaders.TryGetValue("tt-demo-target-echo", out var value) && value.Contains("enabled"))
			{
				_logger.LogInformation(3, "Demo in proc target is ECHOING received request body");

				request.BodyContent.TryRewind();

				responseData = new MemoryStream();
				await request.BodyContent.CopyToAsync(responseData, cancellationToken);
				responseData.TryRewind();
			}
		}

		var response = request.CreateResponse();

		response.HttpStatusCode = HttpStatusCode.OK;
		response.OriginalBodySize = responseData.Length;
		response.BodySize = response.OriginalBodySize;
		response.BodyContent = responseData;

		return response;
	}
}

internal class ConnectorService : IHostedService
{
	private readonly RelayConnector _connector;
	private readonly ILogger<ConnectorService> _logger;

	public ConnectorService(ILogger<ConnectorService> logger, RelayConnector connector)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_connector = connector ?? throw new ArgumentNullException(nameof(connector));
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Starting connector");
		await _connector.ConnectAsync(cancellationToken);
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Gracefully stopping connector");
		await _connector.DisconnectAsync(cancellationToken);
	}
}
