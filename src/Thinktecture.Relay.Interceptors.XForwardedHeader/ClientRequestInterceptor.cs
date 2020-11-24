﻿using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.Interceptor;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Interceptors
{
	internal class ClientRequestInterceptor<TRequest, TResponse> : IClientRequestInterceptor<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		public Task OnRequestReceivedAsync(IRelayContext<TRequest, TResponse> context, CancellationToken cancellationToken = default)
		{
			context.ClientRequest.HttpHeaders.Add("X-Forwarded-For", new[] { context.HttpContext.Connection.RemoteIpAddress.ToString() });
			context.ClientRequest.HttpHeaders.Add("X-Forwarded-Host", new[] { context.HttpContext.Request.Host.Host });
			context.ClientRequest.HttpHeaders.Add("X-Forwarded-Proto", new[] { context.HttpContext.Request.Scheme });
			return Task.CompletedTask;
		}
	}
}
