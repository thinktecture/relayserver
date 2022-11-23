using System;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Thinktecture.Relay.IdentityServer.Services;

/// <summary>
/// Type that provides support for creating x509 certificates.
/// </summary>
internal static class CertificateBuilder
{
	/// <summary>
	/// Create a new certificate.
	/// </summary>
	/// <param name="certName">The name of the new certificate.</param>
	/// <param name="password">The password to protect the private key with.</param>
	/// <param name="notBefore">The not before date of the certificate.</param>
	/// <param name="notAfter">The not after date of the certificate.</param>
	/// <returns>A new certificate.</returns>
	public static X509Certificate2 BuildSelfSignedServerCertificate(string certName, string password,
		DateTimeOffset notBefore, DateTimeOffset notAfter)
	{
		var sanBuilder = new SubjectAlternativeNameBuilder();
		sanBuilder.AddIpAddress(IPAddress.Loopback);
		sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);

		var distinguishedName = new X500DistinguishedName($"CN={certName}");

		using var rsa = RSA.Create(2048);
		var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

		request.CertificateExtensions.Add(new X509KeyUsageExtension(
			X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature,
			false));

		request.CertificateExtensions.Add(sanBuilder.Build());

		var certificate = request.CreateSelfSigned(notBefore, notAfter);

		return new X509Certificate2(certificate.Export(X509ContentType.Pfx, password), password,
			X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
	}
}
