using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.IdentityServer.Extensions;
using Thinktecture.Relay.IdentityServer.Services;

namespace Thinktecture.Relay.IdentityServer.Stores;

/// <summary>
/// Provides support for rotating signing certificates.
/// </summary>
internal class RotateCertificateFileStore
{
	private readonly ILogger<RotateCertificateFileStore> _logger;
	private readonly IOptions<RotateCertificateStoreOptions> _options;
	private readonly TimeSpan _longestTokenLifetime = TimeSpan.FromDays(2);

	private List<X509Certificate2> _cache = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="RotateCertificateFileStore"/> class.
	/// </summary>
	/// <param name="options"></param>
	/// <param name="logger"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public RotateCertificateFileStore(IOptions<RotateCertificateStoreOptions> options,
		ILogger<RotateCertificateFileStore> logger)
	{
		_options = options ?? throw new ArgumentNullException(nameof(options));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Returns the most current certificate (the one to sign).
	/// </summary>
	/// <returns>The most current certificate.</returns>
	public X509Certificate2 GetCurrentCertificate()
		=> GetCachedCertificates().First(IsCurrent);

	/// <summary>
	/// Returns all certificates that need their public keys announced (validation certificates).
	/// </summary>
	/// <returns>All certificates that need to be announced.</returns>
	public IEnumerable<X509Certificate2> GetCertificatesToAnnounce()
		=> GetCachedCertificates().Where(AnnouncementNeeded);

	// Announce a cert IF
	// - a token with the longest possible lifetime
	// - that got signed at the latest possible second of the cert
	// could still be out there
	// This also is always true for all future certificates
	private bool AnnouncementNeeded(X509Certificate2 certificate)
		=> certificate.NotAfterUtc() + _longestTokenLifetime >= DateTime.UtcNow;

	// A certificate is expired when there can't be a valid token for it out there anymore
	private bool IsExpired(X509Certificate2 certificate)
		=> !AnnouncementNeeded(certificate);

	private bool IsCurrent(X509Certificate2 certificate)
		=> DateTime.UtcNow.IsBetween(certificate.NotBeforeUtc(), certificate.NotAfterUtc());

	// We need a new certificate if we don't have one, or the expiry of the longest-living
	// certificate that we do have is lower than our announcement period for the next cert
	private bool NeedNewCertificate(List<X509Certificate2> list)
		=> list.Count == 0 || (list.Max(c => c.NotAfterUtc()) < (DateTime.UtcNow + _options.Value.AnnouncementPeriod));

	private List<X509Certificate2> GetCachedCertificates()
		=> GetOrUpdateCache();

	private List<X509Certificate2> GetOrUpdateCache()
	{
		lock (this)
		{
			if (_cache.Any(IsExpired) || NeedNewCertificate(_cache))
			{
				_logger.LogDebug(1, "Rebuilding certificate cache");
				var list = LoadAllCertificatesFromFolder();

				RemoveOutdated(list);

				while (NeedNewCertificate(list))
				{
					CreateAndStoreNewCertificate(list);
				}

				_cache = list.OrderBy(c => c.NotAfter).ToList();
			}
		}

		return _cache;
	}

	private void CreateAndStoreNewCertificate(List<X509Certificate2> list)
	{
		var certName = "id_server_" + Guid.NewGuid().ToString().Substring(0, 5);

		// New certificate should start overlapping before the last existing one expires (or now, if we don't have existing ones)
		var notBefore = list.Any() ? list.Max(c => c.NotAfter).AddDays(-1) : DateTimeOffset.UtcNow.AddDays(-1);
		// New certificate should end after the full certificate rotation interval (so add the overlapping day again)
		var notAfter = notBefore + _options.Value.RotateInterval + TimeSpan.FromDays(1);

		var certificate =
			CertificateBuilder.BuildSelfSignedServerCertificate(certName, _options.Value.Password, notBefore, notAfter);
		var pfxBytes = certificate.Export(X509ContentType.Pfx, _options.Value.Password);
		var fileName = GetFileName(certificate);

		_logger.LogInformation(2,
			"Create certificate [{CertificateName}], validity from {CertificateNotBefore} until {CertificateNotAfter} as {CertificateFileName}",
			certificate.SerialNumber, certificate.NotBeforeUtc(), certificate.NotAfterUtc(), fileName);

		File.WriteAllBytes(fileName, pfxBytes);
		list.Add(certificate);
	}

	private void RemoveOutdated(List<X509Certificate2> list)
	{
		var outdated = list.Where(IsExpired).ToArray();

		foreach (var c in outdated)
		{
			_logger.LogInformation(3,
				"Remove outdated certificate [{CertificateName}], was valid until {CertificateNotAfter} with grace period of {LongestTokenLifetime}",
				c.SerialNumber, c.NotAfterUtc(), _longestTokenLifetime);

			RemoveCertificateFromFolder(c);
			list.Remove(c);
		}
	}

	private List<X509Certificate2> LoadAllCertificatesFromFolder()
	{
		var result = new List<X509Certificate2>();

		var certFiles = Directory.GetFiles(_options.Value.Path, "*.pfx");
		foreach (var fileName in certFiles)
		{
			_logger.LogDebug(4, "Loading certificate from file {CertificateFileName}", fileName);
			var cert = new X509Certificate2(fileName, _options.Value.Password, X509KeyStorageFlags.MachineKeySet);
			result.Add(cert);
		}

		return result;
	}

	private void RemoveCertificateFromFolder(X509Certificate2 certificate)
	{
		var fileName = GetFileName(certificate);
		_logger.LogDebug(5, "Deleting certificate file {CertificateFileName}", fileName);

		File.Delete(fileName);
	}

	private string GetFileName(X509Certificate2 certificate)
		=> $"{_options.Value.Path}/{certificate.SerialNumber}.pfx";
}
