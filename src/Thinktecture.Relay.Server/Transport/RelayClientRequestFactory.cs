using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <inheritdoc />
	public class RelayClientRequestFactory<T> : IRelayClientRequestFactory<T>
		where T : IClientRequest, new()
	{
		private readonly RelayServerContext _relayServerContext;
		private readonly RelayServerOptions _relayServerOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayClientRequestFactory{T}"/> class.
		/// </summary>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		/// <param name="relayServerOptions">An <see cref="IOptions{TOptions}"/>.</param>
		public RelayClientRequestFactory(RelayServerContext relayServerContext, IOptions<RelayServerOptions> relayServerOptions)
		{
			if (relayServerOptions == null) throw new ArgumentNullException(nameof(relayServerOptions));

			_relayServerContext = relayServerContext ?? throw new ArgumentNullException(nameof(relayServerContext));
			_relayServerOptions = relayServerOptions.Value;
		}

		/// <inheritdoc />
		public Task<T> CreateAsync(Guid tenantId, Guid requestId, HttpRequest httpRequest,
			CancellationToken cancellationToken = default)
		{
			var parts = httpRequest.Path.Value.Split('/').Skip(1).ToArray();

			var request = new T()
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
				AcknowledgeMode = _relayServerOptions.AcknowledgeMode,
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
