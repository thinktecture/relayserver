using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Services
{
	/// <inheritdoc />
	public class RelayTargetResponseWriter<TResponse> : IRelayTargetResponseWriter<TResponse>
		where TResponse : class, ITargetResponse
	{
		private readonly ILogger<RelayTargetResponseWriter<TResponse>> _logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayTargetResponseWriter{TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		public RelayTargetResponseWriter(ILogger<RelayTargetResponseWriter<TResponse>> logger)
			=> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

		/// <inheritdoc />
		public async Task WriteAsync(TResponse? targetResponse, HttpResponse httpResponse, CancellationToken cancellationToken = default)
		{
			if (targetResponse == null)
			{
				httpResponse.StatusCode = StatusCodes.Status204NoContent;
				return;
			}

			if (targetResponse.RequestFailed)
			{
				_logger.LogWarning("The request {RequestId} failed internally with {HttpStatusCode}", targetResponse.RequestId,
					targetResponse.HttpStatusCode);
			}

			httpResponse.StatusCode = (int)targetResponse.HttpStatusCode;

			if (targetResponse.HttpHeaders != null)
			{
				foreach (var (name, values) in targetResponse.HttpHeaders)
				{
					if (name == HeaderNames.TransferEncoding) continue;

					httpResponse.Headers.Add(name, values);
				}
			}

			httpResponse.ContentLength = targetResponse.BodySize;

			if (targetResponse.BodyContent?.CanRead == true)
			{
				await targetResponse.BodyContent.CopyToAsync(httpResponse.Body, cancellationToken);
			}
		}
	}
}
