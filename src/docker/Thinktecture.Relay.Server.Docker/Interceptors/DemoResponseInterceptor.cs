using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Interceptor;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Docker.Interceptors;

public partial class DemoResponseInterceptor : ITargetResponseInterceptor<ClientRequest, TargetResponse>
{
	private readonly ILogger _logger;

	public DemoResponseInterceptor(ILogger<DemoResponseInterceptor> logger)
		=> _logger = logger;

	public async Task OnResponseReceivedAsync(IRelayContext<ClientRequest, TargetResponse> context,
		CancellationToken cancellationToken = default)
	{
		if (context.TargetResponse?.BodySize > 0
		    && context.ClientRequest.HttpHeaders.TryGetValue("tt-demo-response-stream-interceptor", out var value)
		    && value.Any(v => v == "enabled"))
		{
			if (context.ClientRequest.HttpHeaders.TryGetValue("tt-demo-response-stream-nulling", out var nulling) &&
			    nulling.Any(v => v == "enabled"))
			{
				Log.RemovingOutput(_logger, context.RequestId, context.TargetResponse.BodySize);

				context.TargetResponse.BodyContent = null;
				return;
			}

			var size = (int)context.TargetResponse.BodySize * 2;

			Log.Enabled(_logger, context.RequestId, context.TargetResponse.BodySize, size);

			if (context.TargetResponse.BodyContent is not null)
			{
				// Reverse the original content
				var buffer = new byte[size];
				context.TargetResponse.BodyContent.TryRewind();
				var length = await context.TargetResponse.BodyContent.ReadAsync(buffer.AsMemory(0, (int)context.TargetResponse.BodySize), cancellationToken);
				Array.Reverse(buffer, 0, length);

				// Double the original reversed content by appending it a second time
				for (var i = 0; i < length; i++)
				{
					buffer[length + i] = buffer[i];
				}

				var newStream = new MemoryStream(buffer);
				newStream.TryRewind();

				// set new data to response and (on purpose) forget to set the new length
				context.TargetResponse.BodyContent = newStream;
				// context.TargetResponse.BodySize = newStream.Length;
			}
		}
	}
}
