using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Interceptor;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;
using System;

namespace Thinktecture.Relay.Server.Docker.Interceptors;

public partial class DemoRequestInterceptor : IClientRequestInterceptor<ClientRequest, TargetResponse>
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
				Log.RemovingOutput(_logger, context.RequestId, context.ClientRequest.BodySize);

				context.ClientRequest.BodyContent = null;
				return;
			}

			var size = (int)context.ClientRequest.BodySize * 2;
			Log.Enabled(_logger, context.RequestId, context.ClientRequest.BodySize, size);

			if (context.ClientRequest.BodyContent is not null)
			{
				// double the original content by appending it twice
				var buffer = new byte[size];
				context.ClientRequest.BodyContent.TryRewind();
				var length =
					await context.ClientRequest.BodyContent.ReadAsync(buffer.AsMemory(0, (int)context.ClientRequest.BodySize), cancellationToken);
				for (var i = 0; i < length; i++)
				{
					buffer[length + i] = buffer[i];
				}

				var newStream = new MemoryStream(buffer);
				newStream.TryRewind();

				// set new data to request and (on purpose) forget to set the new length
				context.ClientRequest.BodyContent = newStream;
				// context.ClientRequest.BodySize = newStream.Length;
			}
		}
	}
}
