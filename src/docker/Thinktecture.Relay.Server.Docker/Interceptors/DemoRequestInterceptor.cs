using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Interceptor;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Docker.Interceptors;

public class DemoRequestInterceptor : IClientRequestInterceptor<ClientRequest, TargetResponse>
{
	private readonly ILogger _logger;

	public DemoRequestInterceptor(ILogger<DemoRequestInterceptor> logger)
		=> _logger = logger;

	public async Task OnRequestReceivedAsync(IRelayContext<ClientRequest, TargetResponse> context,
		CancellationToken cancellationToken = default)
	{
		if (context.ClientRequest.BodySize > 0
		    && context.ClientRequest.HttpHeaders.TryGetValue("tt-demo-request-stream-interceptor", out var value)
		    && value.Any(v => v == "enabled"))
		{

			if (context.ClientRequest.HttpHeaders.TryGetValue("tt-demo-request-stream-nulling", out var nulling) &&
			    nulling.Any(v => v == "enabled"))
			{
				_logger.LogInformation(1,
					"Request stream interceptor enabled for request {RequestId}, input was {OriginalRequestSize}, output will be NULL",
					context.RequestId, context.ClientRequest.BodySize);

				context.ClientRequest.BodyContent = null;
				return;
			}

			int size = (int)context.ClientRequest.BodySize * 2;
			_logger.LogInformation(1,
				"Request stream interceptor enabled for request {RequestId}, input was {OriginalRequestSize}, output will be {RequestSize} bytes",
				context.RequestId, context.ClientRequest.BodySize, size);

			// double the original content by appending it twice
			var buffer = new byte[size];
			context.ClientRequest.BodyContent.TryRewind();
			context.ClientRequest.BodyContent.Read(buffer, 0, (int)context.ClientRequest.BodySize);
			for (var i = 0; i < context.ClientRequest.BodySize; i++)
			{
				buffer[(int)context.ClientRequest.BodySize + i] = buffer[i];
			}

			var newStream = new MemoryStream(buffer);
			newStream.TryRewind();

			// set new data to request and (on purpose) forget to set the new length
			context.ClientRequest.BodyContent = newStream;
			// context.ClientRequest.BodySize = newStream.Length;
		}
	}
}