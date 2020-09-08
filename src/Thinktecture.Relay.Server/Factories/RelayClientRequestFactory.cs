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
		private readonly Guid _originId;

		/// <summary>
		/// Initializes a new instance of <see cref="RelayClientRequestFactory{TRequest}"/>.
		/// </summary>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		public RelayClientRequestFactory(RelayServerContext relayServerContext)
			=> _originId = relayServerContext?.OriginId ?? throw new ArgumentNullException(nameof(relayServerContext));

		/// <inheritdoc />
		public Task<TRequest> CreateAsync(Guid tenantId, HttpRequest request, CancellationToken cancellationToken = default)
		{
			var parts = request.Path.Value.Split('/').Skip(1).ToArray();

			return Task.FromResult(new TRequest()
			{
				RequestId = Guid.NewGuid(),
				RequestOriginId = _originId,
				Target = parts.Length > 1 ? parts[1] : string.Empty,
				TenantId = tenantId,
				HttpMethod = request.Method,
				Url = $"{string.Join("/", parts.Skip(2))}{request.QueryString}",
				HttpHeaders = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray()),
				BodySize = request.Body.Length,
				BodyContent = request.Body.Length == 0 ? null : request.Body,
				AcknowledgeMode = AcknowledgeMode.Disabled // TODO get acknowledge mode
			});
		}
	}
}
