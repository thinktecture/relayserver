using System;
using System.Security.Cryptography.X509Certificates;

namespace Thinktecture.Relay.IdentityServer.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="X509Certificate2"/> class.
/// </summary>
internal static class X509Certificate2Extensions
{
	/// <summary>
	/// Returns the NotBefore value of the certificate in UTC.
	/// </summary>
	/// <param name="cert">The certificate to retrieve the value from.</param>
	/// <returns>The "Not Before" value of the certificate in UTC.</returns>
	public static DateTime NotBeforeUtc(this X509Certificate2 cert)
		=> cert.NotBefore.ToUniversalTime();

	/// <summary>
	/// Returns the NotAfter value of the certificate in UTC.
	/// </summary>
	/// <param name="cert">The certificate to retrieve the value from.</param>
	/// <returns>The "Not After" value of the certificate in UTC.</returns>
	public static DateTime NotAfterUtc(this X509Certificate2 cert)
		=> cert.NotAfter.ToUniversalTime();
}
