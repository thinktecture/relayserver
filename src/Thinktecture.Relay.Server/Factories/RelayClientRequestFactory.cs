using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Factories
{
	/// <inheritdoc />
	public class RelayClientRequestFactory<TRequest> : IRelayClientRequestFactory<TRequest>
		where TRequest : IClientRequest, new()
	{
		private readonly RelayServerContext _relayServerContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayClientRequestFactory{TRequest}"/> class.
		/// </summary>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		public RelayClientRequestFactory(RelayServerContext relayServerContext)
			=> _relayServerContext = relayServerContext ?? throw new ArgumentNullException(nameof(relayServerContext));

		/// <inheritdoc />
		public Task<TRequest> CreateAsync(Guid tenantId, Guid requestId, HttpRequest httpRequest,
			CancellationToken cancellationToken = default)
		{
			var parts = httpRequest.Path.Value.Split('/').Skip(1).ToArray();

			var request = new TRequest()
			{
				RequestId = requestId,
				RequestOriginId = _relayServerContext.OriginId,
				Target = parts.Length > 1 ? parts[1] : string.Empty,
				TenantId = tenantId,
				HttpMethod = httpRequest.Method,
				Url = $"{string.Join("/", parts.Skip(2))}{httpRequest.QueryString}",
				HttpHeaders = httpRequest.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray()),
				BodySize = httpRequest.Body.Length,
				BodyContent = httpRequest.Body.Length == 0 ? null : httpRequest.Body,
				AcknowledgeMode = AcknowledgeMode.Disabled, // TODO get acknowledge mode
				EnableTracing = httpRequest.Headers.ContainsKey(Constants.HeaderNames.EnableTracing)
			};

			if (request.EnableTracing)
			{
				request.HttpHeaders.Remove(Constants.HeaderNames.EnableTracing);
			}

			return Task.FromResult(request);
		}
	}
}
