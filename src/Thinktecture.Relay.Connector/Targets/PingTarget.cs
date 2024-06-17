using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Targets;

/// <inheritdoc />
public class PingTarget<TRequest, TResponse> : IRelayTargetFunc<TRequest, TResponse>
	where TRequest : IClientRequest
	where TResponse : ITargetResponse, new()
{
	/// <inheritdoc />
	public Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
	{
		if (!request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
			return Task.FromResult(request.CreateResponse<TResponse>(HttpStatusCode.NotFound));

		var result = request.CreateResponse<TResponse>(HttpStatusCode.OK);

		result.HttpHeaders = new Dictionary<string, string[]>()
		{
			{ "Content-Type", ["text/plain"] },
		};
		result.BodyContent = new MemoryStream("PONG"u8.ToArray());
		result.BodySize = result.BodyContent.Length;

		return Task.FromResult(result);
	}
}
