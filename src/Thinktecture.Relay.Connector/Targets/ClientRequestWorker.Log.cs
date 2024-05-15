using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Targets;

public partial class ClientRequestWorker<TRequest, TResponse>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.ClientRequestWorkerNoTargetFound, LogLevel.Information,
			"Could not find any target for request {RelayRequestId} named {Target}")]
		public static partial void NoTargetFound(ILogger logger, Guid relayRequestId, string target);

		[LoggerMessage(LoggingEventIds.ClientRequestWorkerFoundTarget, LogLevel.Trace,
			"Found target {Target} for request {RelayRequestId}")]
		public static partial void FoundTarget(ILogger logger, string target, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ClientRequestWorkerRequestingBody, LogLevel.Debug,
			"Requesting outsourced request body for request {RelayRequestId} with {BodySize} bytes")]
		public static partial void RequestingBody(ILogger logger, Guid relayRequestId, long? bodySize);

		[LoggerMessage(LoggingEventIds.ClientRequestWorkerRequestingTarget, LogLevel.Debug,
			"Requesting target {Target} for request {RelayRequestId}")]
		public static partial void RequestingTarget(ILogger logger, string target, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ClientRequestWorkerOutsourcingUnknownBody, LogLevel.Debug,
			"Unknown response body size triggered mandatory outsourcing for request {RelayRequestId}")]
		public static partial void OutsourcingUnknownBody(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ClientRequestWorkerOutsourcingBody, LogLevel.Debug,
			"Outsourcing from response {BodySize} bytes because of a maximum of {BinarySizeThreshold} for request {RelayRequestId}")]
		public static partial void OutsourcingBody(ILogger logger, long? bodySize, int? binarySizeThreshold,
			Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ClientRequestWorkerOutsourcedBody, LogLevel.Debug,
			"Outsourced from response {BodySize} bytes for request {RelayRequestId}")]
		public static partial void OutsourcedBody(ILogger logger, long bodySize, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ClientRequestWorkerOutsourcingBodyFailed, LogLevel.Error,
			"Uploading body of request {RelayRequestId} failed with http status {HttpStatusCode}")]
		public static partial void OutsourcingBodyFailed(ILogger logger, Guid relayRequestId,
			HttpStatusCode httpStatusCode);

		[LoggerMessage(LoggingEventIds.ClientRequestWorkerErrorOutsourcingBody, LogLevel.Error,
			"An error occured while uploading the body of request {RelayRequestId}")]
		public static partial void ErrorOutsourcingBody(ILogger logger, Exception ex, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ClientRequestWorkerInlineBody, LogLevel.Debug,
			"Inlined from response {BodySize} bytes for request {RelayRequestId}")]
		public static partial void InlinedBody(ILogger logger, long? bodySize, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ClientRequestWorkerErrorDownloadingBody, LogLevel.Error,
			"An error occured while downloading the body of request {RelayRequestId}")]
		public static partial void ErrorDownloadingBody(ILogger logger, Exception exception, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ClientRequestWorkerRequestTimedOut, LogLevel.Warning,
			"The request {RelayRequestId} timed out")]
		public static partial void RequestTimedOut(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ClientRequestWorkerErrorProcessingRequest, LogLevel.Error,
			"An error occured while processing request {RelayRequestId} {@Request}")]
		public static partial void ErrorProcessingRequest(ILogger logger, Exception exception, Guid relayRequestId,
			TRequest request);

		[LoggerMessage(LoggingEventIds.ClientRequestWorkerUploadingBodyFailed, LogLevel.Error,
			"Uploading body of request {RelayRequestId} failed with http status {HttpStatusCode}")]
		public static partial void UploadingBodyFailed(ILogger logger, Guid relayRequestId,
			HttpStatusCode httpStatusCode);

		[LoggerMessage(LoggingEventIds.ClientRequestWorkerErrorUploadingBody, LogLevel.Error,
			"An error occured while uploading the body of request {RelayRequestId}")]
		public static partial void ErrorUploadingBody(ILogger logger, Exception ex, Guid relayRequestId);
	}
}
