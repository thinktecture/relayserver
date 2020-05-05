using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector
{
	/// <inheritdoc />
	public class RelayTargetResponseFactory<TResponse> : IRelayTargetResponseFactory<TResponse>
		where TResponse : IRelayTargetResponse, new()
	{
		/// <inheritdoc />
		public async Task<TResponse> CreateAsync(HttpResponseMessage message)
		{
			return new TResponse()
			{
				HttpStatusCode = message.StatusCode,
				HttpHeaders = message.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray()),
				BodySize = message.Content.Headers.ContentLength,
				BodyContent = await message.Content.ReadAsStreamAsync()
			};
		}
	}
}
