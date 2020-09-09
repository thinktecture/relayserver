using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Services
{
	/// <inheritdoc />
	public class RelayTargetResponseWriter<TResponse> : IRelayTargetResponseWriter<TResponse>
		where TResponse : ITargetResponse
	{
		/// <inheritdoc />
		public async Task WriteAsync(TResponse targetResponse, HttpResponse httpResponse, CancellationToken cancellationToken = default)
		{
			httpResponse.StatusCode = (int)targetResponse.HttpStatusCode;

			foreach (var (name, values) in targetResponse.HttpHeaders)
			{
				if (name == HeaderNames.TransferEncoding)
				{
					continue;
				}

				httpResponse.Headers.Add(name, values);
			}

			httpResponse.ContentLength = targetResponse.BodySize;

			if (targetResponse.BodyContent?.CanRead == true)
			{
				await targetResponse.BodyContent.CopyToAsync(httpResponse.Body, cancellationToken);
			}
		}
	}
}
