using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Factories
{
	/// <inheritdoc />
	public class RelayClientRequestFactory<TRequest> : IRelayClientRequestFactory<TRequest>
		where TRequest : IRelayClientRequest, new()
	{
		/// <inheritdoc />
		public Task<TRequest> CreateAsync(HttpContext context, Guid tenantId)
		{
			var relayServerContext = context.RequestServices.GetService<RelayServerContext>();
			var parts = context.Request.Path.Value.Split('/').Skip(1).ToArray();

			return Task.FromResult(new TRequest()
			{
				RequestId = Guid.NewGuid(),
				RequestOriginId = relayServerContext.OriginId,
				Target = parts.Length > 1 ? parts[1] : string.Empty,
				TenantId = tenantId,
				HttpMethod = context.Request.Method,
				Url = $"/{string.Join("/", parts.Skip(2))}{context.Request.QueryString}",
				HttpHeaders = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray()),
				BodySize = context.Request.Body.Length,
				BodyContent = context.Request.Body.Length == 0 ? null : context.Request.Body,
				AcknowledgeMode = AcknowledgeMode.Disabled // TODO get from somewhere
			});
		}
	}
}
