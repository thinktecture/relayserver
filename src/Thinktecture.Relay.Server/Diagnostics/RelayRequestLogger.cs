using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Server.Extensions;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Persistence.Models;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Diagnostics;

/// <inheritdoc />
public partial class RelayRequestLogger<TRequest, TResponse> : IRelayRequestLogger<TRequest, TResponse>
	where TRequest : IClientRequest
	where TResponse : class, ITargetResponse
{
	private readonly ILogger<RelayRequestLogger<TRequest, TResponse>> _logger;
	private readonly RelayServerOptions _relayServerOptions;
	private readonly IRequestService _requestService;
	private readonly ITenantService _tenantService;

	/// <summary>
	/// Initializes a new instance of the <see cref="RelayRequestLogger{TRequest,TResponse}"/> class.
	/// </summary>
	/// <param name="logger">An instance of an <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="requestService">An <see cref="IRequestService"/>.</param>
	/// <param name="relayServerOptions">An <see cref="IOptions{TOptions}"/>.</param>
	/// <param name="tenantService">An <see cref="ITenantService"/>.</param>
	public RelayRequestLogger(ILogger<RelayRequestLogger<TRequest, TResponse>> logger, IRequestService requestService,
		IOptions<RelayServerOptions> relayServerOptions, ITenantService tenantService)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
		_tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));

		if (relayServerOptions == null) throw new ArgumentNullException(nameof(relayServerOptions));

		_relayServerOptions = relayServerOptions.Value;
	}

	[LoggerMessage(20400, LogLevel.Trace, "Writing request log for successful request {RelayRequestId}")]
	partial void LogSuccess(Guid relayRequestId);

	/// <inheritdoc />
	public async Task LogSuccessAsync(IRelayContext relayContext, long bodySize, HttpRequest httpRequest,
		TResponse? targetResponse)
	{
		if (!_relayServerOptions.RequestLoggerLevel.LogSucceeded()) return;

		LogSuccess(relayContext.RequestId);

		var request = CreateRequest(relayContext, bodySize, httpRequest);
		request.HttpStatusCode = targetResponse?.HttpStatusCode;
		request.ResponseOriginalBodySize = targetResponse?.OriginalBodySize;
		request.ResponseBodySize = targetResponse?.BodySize;
		await _requestService.StoreRequestAsync(request);
	}

	[LoggerMessage(20401, LogLevel.Trace, "Writing request log for aborted request {RelayRequestId}")]
	partial void LogAbort(Guid relayRequestId);

	/// <inheritdoc />
	public async Task LogAbortAsync(IRelayContext relayContext, long bodySize, HttpRequest httpRequest)
	{
		if (!_relayServerOptions.RequestLoggerLevel.LogAborted()) return;

		LogAbort(relayContext.RequestId);

		var request = CreateRequest(relayContext, bodySize, httpRequest);
		request.Aborted = true;
		await _requestService.StoreRequestAsync(request);
	}

	[LoggerMessage(20402, LogLevel.Trace, "Writing request log for failed request {RelayRequestId}")]
	partial void LogFail(Guid relayRequestId);

	/// <inheritdoc />
	public async Task LogFailAsync(IRelayContext relayContext, long bodySize, HttpRequest httpRequest)
	{
		if (!_relayServerOptions.RequestLoggerLevel.LogFailed()) return;

		LogFail(relayContext.RequestId);

		var request = CreateRequest(relayContext, bodySize, httpRequest);
		request.Failed = true;
		await _requestService.StoreRequestAsync(request);
	}

	[LoggerMessage(20403, LogLevel.Trace, "Writing request log for expired request {RelayRequestId}")]
	partial void LogExpired(Guid relayRequestId);

	/// <inheritdoc />
	public async Task LogExpiredAsync(IRelayContext relayContext, long bodySize, HttpRequest httpRequest)
	{
		if (!_relayServerOptions.RequestLoggerLevel.LogExpired()) return;

		LogExpired(relayContext.RequestId);

		var request = CreateRequest(relayContext, bodySize, httpRequest);
		request.Expired = true;
		await _requestService.StoreRequestAsync(request);
	}

	[LoggerMessage(20404, LogLevel.Trace, "Writing request log for error on request {RelayRequestId}")]
	partial void LogError(Guid relayRequestId);

	/// <inheritdoc />
	public async Task LogErrorAsync(IRelayContext relayContext, long bodySize, HttpRequest httpRequest)
	{
		if (!_relayServerOptions.RequestLoggerLevel.LogErrored()) return;

		LogError(relayContext.RequestId);

		var request = CreateRequest(relayContext, bodySize, httpRequest);
		request.Errored = true;
		await _requestService.StoreRequestAsync(request);
	}

	private Request CreateRequest(IRelayContext relayContext, long bodySize, HttpRequest httpRequest)
	{
		var (_, tenantName, target, url) = httpRequest.GetRelayRequest();

		return new Request()
		{
			TenantName = _tenantService.NormalizeName(tenantName),
			RequestId = relayContext.RequestId,
			RequestDate = relayContext.RequestStart,
			RequestDuration = (long)(DateTime.UtcNow - relayContext.RequestStart).TotalMilliseconds,
			RequestOriginalBodySize = httpRequest.Body.Length,
			RequestBodySize = bodySize,
			Target = target,
			HttpMethod = httpRequest.Method,
			RequestUrl = url,
		};
	}
}
