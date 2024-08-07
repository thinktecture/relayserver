using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
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
			.AddPingTarget()
			.AddEchoTarget()
			.AddTarget<InProcFunc>("inprocfunc")
			.AddTarget<InProcAction>("inprocaction");

		services.AddHostedService<ConnectorService>();
	}
}

internal partial class InProcFunc : IRelayTargetFunc
{
	private ILogger _logger;

	public InProcFunc(ILogger<InProcFunc> logger)
		=> _logger = logger;

	public async Task<TargetResponse> HandleAsync(ClientRequest request, CancellationToken cancellationToken = default)
	{
		Log.Executing(_logger, request.RequestId);

		var responseData = new MemoryStream(Encoding.UTF8.GetBytes("{ \"Hello\" : \"World 👋\" }"));

		if (request.BodyContent is not null)
		{
			Log.BodySize(_logger, request.RequestId, request.BodySize);

			// if echo is requested, return the received content
			if (request.HttpHeaders.TryGetValue("tt-demo-target-echo", out var value) && value.Contains("enabled"))
			{
				Log.Echo(_logger);
				request.BodyContent.TryRewind();

				responseData = new MemoryStream();
				await request.BodyContent.CopyToAsync(responseData, cancellationToken);
				responseData.TryRewind();
			}
		}

		// Test: If we wait with ack for a completed target request, delay a bit to be able to restart rabbit
		var url = $"http://localhorst/{request.Url}";
		if (TimeSpan.TryParse(HttpUtility.ParseQueryString(new Uri(url).Query).Get("delay"), out var delay))
		{
			await Task.Delay(delay, cancellationToken);
		}

		var response = request.CreateResponse();

		response.HttpStatusCode = HttpStatusCode.OK;
		response.OriginalBodySize = responseData.Length;
		response.BodySize = response.OriginalBodySize;
		response.BodyContent = responseData;

		return response;
	}
}

internal class InProcAction : IRelayTargetAction
{
	public Task HandleAsync(ClientRequest request, CancellationToken cancellationToken = default)
		=> Task.Delay(TimeSpan.FromSeconds(42), cancellationToken);
}

internal partial class ConnectorService : IHostedService
{
	private readonly RelayConnector _connector;
	private readonly ILogger _logger;

	public ConnectorService(ILogger<ConnectorService> logger, RelayConnector connector)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_connector = connector ?? throw new ArgumentNullException(nameof(connector));
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		Log.Starting(_logger);
		await _connector.ConnectAsync(cancellationToken);
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		Log.Stopping(_logger);
		await _connector.DisconnectAsync(cancellationToken);
	}
}
