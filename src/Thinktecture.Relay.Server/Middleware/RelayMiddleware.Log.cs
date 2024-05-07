using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Middleware;

public partial class RelayMiddleware<TRequest, TResponse, TAcknowledge>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.RelayMiddlewareInvalidRequest, LogLevel.Warning,
			"Invalid request received {Path}{Query}, does not contain a tenant id")]
		public static partial void InvalidRequest(ILogger logger, string path, QueryString query);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareUnknownTenant, LogLevel.Information,
			"Unknown tenant {Tenant} in request received {Path}{Query}")]
		public static partial void UnknownTenant(ILogger logger, string tenant, string path, QueryString query);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareRequestParsed, LogLevel.Trace, "Parsed request {@Request}")]
		public static partial void RequestParsed(ILogger logger, IClientRequest request);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareResponseReceived, LogLevel.Trace,
			"Received response for request {RelayRequestId}")]
		public static partial void ResponseReceived(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareClientAborted, LogLevel.Debug,
			"Client aborted request {RelayRequestId}")]
		public static partial void ClientAborted(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareRequestExpired, LogLevel.Information,
			"Request {RelayRequestId} expired")]
		public static partial void RequestExpired(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareErrorHandlingRequest, LogLevel.Error,
			"Could not handle request {RelayRequestId}")]
		public static partial void ErrorHandlingRequest(ILogger logger, Exception ex, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareNoActiveConnection, LogLevel.Debug,
			"Request to tenant {Tenant} was rejected due to no active connection")]
		public static partial void NoActiveConnection(ILogger logger, string tenant);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareAcknowledgeModeChange, LogLevel.Debug,
			$"Acknowledge mode of request {{RelayRequestId}} was changed to {nameof(AcknowledgeMode.ConnectorFinished)}")]
		public static partial void AcknowledgeModeChange(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareDiscardConnectorResponse, LogLevel.Debug,
			"Discarding connector response of request {RelayRequestId}")]
		public static partial void DiscardConnectorResponse(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareExecutingRequestInterceptors, LogLevel.Debug,
			"Executing client request interceptors for request {RelayRequestId}")]
		public static partial void ExecutingRequestInterceptors(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareExecutingRequestInterceptor, LogLevel.Trace,
			"Executing interceptor {Interceptor} for request {RelayRequestId}")]
		public static partial void ExecutingRequestInterceptor(ILogger logger, string? interceptor, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareDeliveringRequest, LogLevel.Trace,
			"Delivering request {RelayRequestId} to connector")]
		public static partial void DeliveringRequest(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareWaitForResponse, LogLevel.Trace,
			"Waiting for connector response for request {RelayRequestId}")]
		public static partial void WaitForResponse(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareExecutingResponseInterceptors, LogLevel.Debug,
			"Executing target response interceptors for request {RelayRequestId}")]
		public static partial void ExecutingResponseInterceptors(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareExecutingResponseInterceptor, LogLevel.Trace,
			"Executing interceptor {Interceptor} for request {RelayRequestId}")]
		public static partial void ExecutingResponseInterceptor(ILogger logger, string? interceptor, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareOutsourcingRequestBody, LogLevel.Debug,
			"Outsourcing from request {BodySize} bytes (original {OriginalBodySize} bytes) because of a maximum of {BinarySizeThreshold} for request {RelayRequestId}")]
		public static partial void OutsourcingRequestBody(ILogger logger, long? bodySize, long? originalBodySize,
			int binarySizeThreshold,
			Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareOutsourcedRequestBody, LogLevel.Trace,
			"Outsourced from request {BodySize} bytes (original {OriginalBodySize} bytes) for request {RelayRequestId}")]
		public static partial void OutsourcedRequestBody(ILogger logger, long? bodySize, long? originalBodySize,
			Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.RelayMiddlewareInlinedRequestBody, LogLevel.Debug,
			"Inlined from request {BodySize} bytes (original {OriginalBodySize} bytes) for request {RelayRequestId}")]
		public static partial void InlinedRequestBody(ILogger logger, long? bodySize, long? originalBodySize,
			Guid relayRequestId);
	}
}
