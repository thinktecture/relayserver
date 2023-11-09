using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Targets;

public partial class ClientRequestWorker<TRequest, TResponse>
{
	private static partial class Log
	{
		[LoggerMessage(LoggerEventIds.ClientRequestWorkerNoTargetFound, LogLevel.Information,
			"Could not find any target for request {RelayRequestId} named {Target}")]
		public static partial void NoTargetFound(ILogger logger, Guid relayRequestId, string target);

		[LoggerMessage(LoggerEventIds.ClientRequestWorkerFoundTarget, LogLevel.Trace,
			"Found target {Target} for request {RelayRequestId}")]
		public static partial void FoundTarget(ILogger logger, string target, Guid relayRequestId);

		[LoggerMessage(LoggerEventIds.ClientRequestWorkerRequestingBody, LogLevel.Debug,
			"Requesting outsourced request body for request {RelayRequestId} with {BodySize} bytes")]
		public static partial void RequestingBody(ILogger logger, Guid relayRequestId, long? bodySize);

		[LoggerMessage(LoggerEventIds.ClientRequestWorkerRequestingTarget, LogLevel.Debug,
			"Requesting target {Target} for request {RelayRequestId}")]
		public static partial void RequestingTarget(ILogger logger, string target, Guid relayRequestId);

		[LoggerMessage(LoggerEventIds.ClientRequestWorkerOutsourcingUnknownBody, LogLevel.Debug,
			"Unknown response body size triggered mandatory outsourcing for request {RelayRequestId}")]
		public static partial void OutsourcingUnknownBody(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggerEventIds.ClientRequestWorkerOutsourcingBody, LogLevel.Debug,
			"Outsourcing from response {BodySize} bytes because of a maximum of {BinarySizeThreshold} for request {RelayRequestId}")]
		public static partial void OutsourcingBody(ILogger logger, long? bodySize, int? binarySizeThreshold,
			Guid relayRequestId);

		[LoggerMessage(LoggerEventIds.ClientRequestWorkerOutsourcedBody, LogLevel.Debug,
			"Outsourced from response {BodySize} bytes for request {RelayRequestId}")]
		public static partial void OutsourcedBody(ILogger logger, long bodySize, Guid relayRequestId);

		[LoggerMessage(LoggerEventIds.ClientRequestWorkerOutsourcingBodyFailed, LogLevel.Error,
			"Uploading body of request {RelayRequestId} failed with http status {HttpStatusCode}")]
		public static partial void OutsourcingBodyFailed(ILogger logger, Guid relayRequestId,
			HttpStatusCode httpStatusCode);

		[LoggerMessage(LoggerEventIds.ClientRequestWorkerErrorOutsourcingBody, LogLevel.Error,
			"An error occured while uploading the body of request {RelayRequestId}")]
		public static partial void ErrorOutsourcingBody(ILogger logger, Exception ex, Guid relayRequestId);

		[LoggerMessage(LoggerEventIds.ClientRequestWorkerInlineBody, LogLevel.Debug,
			"Inlined from response {BodySize} bytes for request {RelayRequestId}")]
		public static partial void InlinedBody(ILogger logger, long? bodySize, Guid relayRequestId);

		[LoggerMessage(LoggerEventIds.ClientRequestWorkerErrorDownloadingBody, LogLevel.Error,
			"An error occured while downloading the body of request {RelayRequestId}")]
		public static partial void ErrorDownloadingBody(ILogger logger, Exception exception, Guid relayRequestId);

		[LoggerMessage(LoggerEventIds.ClientRequestWorkerRequestTimedOut, LogLevel.Warning,
			"The request {RelayRequestId} timed out")]
		public static partial void RequestTimedOut(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggerEventIds.ClientRequestWorkerErrorProcessingRequest, LogLevel.Error,
			"An error occured while processing request {RelayRequestId} {@Request}")]
		public static partial void ErrorProcessingRequest(ILogger logger, Exception exception, Guid relayRequestId,
			TRequest request);

		[LoggerMessage(LoggerEventIds.ClientRequestWorkerUploadingBodyFailed, LogLevel.Error,
			"Uploading body of request {RelayRequestId} failed with http status {HttpStatusCode}")]
		public static partial void UploadingBodyFailed(ILogger logger, Guid relayRequestId,
			HttpStatusCode httpStatusCode);

		[LoggerMessage(LoggerEventIds.ClientRequestWorkerErrorUploadingBody, LogLevel.Error,
			"An error occured while uploading the body of request {RelayRequestId}")]
		public static partial void ErrorUploadingBody(ILogger logger, Exception ex, Guid relayRequestId);
	}
}
