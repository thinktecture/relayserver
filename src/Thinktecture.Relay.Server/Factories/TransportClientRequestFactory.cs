using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server.Factories
{
	/// <inheritdoc />
	public class TransportClientRequestFactory<TRequest> : ITransportClientRequestFactory<TRequest>
		where TRequest : ITransportClientRequest, new()
	{
		/// <inheritdoc />
		public TRequest Create(HttpContext context)
		{
			var parts = context.Request.Path.Value.Split('/');

			return new TRequest()
			{
				RequestId = Guid.NewGuid(),
				RequestOriginId = Guid.Empty, // TODO get from somewhere
				Target = parts[2],
				HttpMethod = context.Request.Method,
				Url = $"/{string.Join("/", parts.Skip(3))}",
				HttpHeaders = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray()),
				BodySize = context.Request.ContentLength,
				IsBodyAvailable = context.Request.Body != null,
				BodyStream = context.Request.Body,
				AcknowledgeMode = AcknowledgeMode.Disabled // TODO get from
			};
		}
	}
}
