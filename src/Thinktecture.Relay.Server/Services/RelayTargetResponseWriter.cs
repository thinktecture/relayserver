using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Services;

/// <inheritdoc />
public partial class RelayTargetResponseWriter<TRequest, TResponse> : IRelayTargetResponseWriter<TRequest, TResponse>
	where TRequest : IClientRequest
	where TResponse : class, ITargetResponse
{
	private readonly ILogger _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="RelayTargetResponseWriter{TRequest,TResponse}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	public RelayTargetResponseWriter(ILogger<RelayTargetResponseWriter<TRequest, TResponse>> logger)
		=> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	/// <inheritdoc />
	public async Task WriteAsync(TRequest clientRequest, TResponse? targetResponse, HttpResponse httpResponse,
		CancellationToken cancellationToken = default)
	{
		if (clientRequest.EnableTracing)
		{
			httpResponse.Headers[Constants.HeaderNames.RequestId] = new[] { clientRequest.RequestId.ToString() };
			httpResponse.Headers[Constants.HeaderNames.ServerMachineName] = new[] { Environment.MachineName };
			httpResponse.Headers[Constants.HeaderNames.ServerVersion] = new[] { GetType().GetAssemblyVersion() };
		}

		if (targetResponse is null)
		{
			httpResponse.StatusCode = StatusCodes.Status204NoContent;
			return;
		}

		if (targetResponse.RequestFailed)
		{
			Log.FailedRequest(_logger, targetResponse.RequestId, targetResponse.HttpStatusCode);
		}

		httpResponse.StatusCode = (int)targetResponse.HttpStatusCode;

		if (targetResponse.HttpHeaders is not null)
		{
			foreach (var (name, values) in targetResponse.HttpHeaders)
			{
				if (name == HeaderNames.TransferEncoding) continue;

				httpResponse.Headers[name] = values;
			}
		}

		httpResponse.ContentLength = targetResponse.BodySize;

		if (targetResponse.BodyContent?.CanRead == true)
		{
			targetResponse.BodyContent.TryRewind();
			await targetResponse.BodyContent.CopyToAsync(httpResponse.Body, cancellationToken);
		}
	}
}
