using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.IdentityServer.Stores;

internal partial class RotateCertificateFileStore
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.RotateCertificateFileStoreRebuildingCertificateCache, LogLevel.Debug,
			"Rebuilding certificate cache")]
		public static partial void RebuildingCertificateCache(ILogger logger);

		[LoggerMessage(LoggingEventIds.RotateCertificateFileStoreCreateCertificate, LogLevel.Information,
			"Create certificate [{CertificateName}], validity from {CertificateNotBefore} until {CertificateNotAfter} as {CertificateFileName}")]
		public static partial void CreateCertificate(ILogger logger, string certificateName,
			DateTime certificateNotBefore, DateTime certificateNotAfter, string certificateFileName);

		[LoggerMessage(LoggingEventIds.RotateCertificateFileStoreRemoveOutdatedCertificate, LogLevel.Information,
			"Remove outdated certificate [{CertificateName}], was valid until {CertificateNotAfter} with grace period of {LongestTokenLifetime}")]
		public static partial void RemoveOutdatedCertificate(ILogger logger, string certificateName,
			DateTime certificateNotAfter, TimeSpan longestTokenLifetime);

		[LoggerMessage(LoggingEventIds.RotateCertificateFileStoreLoadCertificate, LogLevel.Debug,
			"Loading certificate from file {CertificateFileName}")]
		public static partial void LoadCertificate(ILogger logger, string certificateFileName);

		[LoggerMessage(LoggingEventIds.RotateCertificateFileStoreDeleteCertificateFile, LogLevel.Debug,
			"Deleting certificate file {CertificateFileName}")]
		public static partial void DeleteCertificateFile(ILogger logger, string certificateFileName);
	}
}
