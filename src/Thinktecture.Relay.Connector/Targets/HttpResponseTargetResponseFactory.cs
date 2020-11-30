using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Targets
{
	/// <inheritdoc />
	public class HttpResponseTargetResponseFactory<TResponse> : ITargetResponseFactory<TResponse>
		where TResponse : ITargetResponse, new()
	{
		/// <inheritdoc />
		public async Task<TResponse> CreateAsync(IClientRequest request, HttpResponseMessage message,
			CancellationToken cancellationToken = default)
		{
			var hasBody = message.StatusCode != HttpStatusCode.NoContent;

			var response = request.CreateResponse<TResponse>();
			response.HttpStatusCode = message.StatusCode;
			response.HttpHeaders = message.Headers.Concat(message.Content.Headers).ToDictionary(h => h.Key, h => h.Value.ToArray());
			response.BodySize = hasBody ? message.Content.Headers.ContentLength : 0;
			response.BodyContent = hasBody ? await message.Content.ReadAsStreamAsync() : null;

			return response;
		}
	}
}
