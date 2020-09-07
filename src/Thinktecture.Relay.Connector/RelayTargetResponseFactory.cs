using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector
{
	/// <inheritdoc />
	public class RelayTargetResponseFactory<TResponse> : IRelayTargetResponseFactory<TResponse>
		where TResponse : ITargetResponse, new()
	{
		/// <inheritdoc />
		public async Task<TResponse> CreateAsync(HttpResponseMessage message, CancellationToken cancellationToken = default)
		{
			var hasBody = message.StatusCode != HttpStatusCode.NoContent;

			return new TResponse()
			{
				HttpStatusCode = message.StatusCode,
				HttpHeaders = message.Headers.Concat(message.Content.Headers).ToDictionary(h => h.Key, h => h.Value.ToArray()),
				BodySize = hasBody ? message.Content.Headers.ContentLength : 0,
				BodyContent = hasBody ? await message.Content.ReadAsStreamAsync() : null
			};
		}
	}
}
