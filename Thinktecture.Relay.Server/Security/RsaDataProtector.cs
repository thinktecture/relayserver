using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Owin.Security.DataProtection;

namespace Thinktecture.Relay.Server.Security
{
	public class RsaDataProtector : IDataProtector
	{
		private readonly X509Certificate2 _cert;

		public RsaDataProtector(X509Certificate2 cert)
		{
			if (cert == null)
				throw new ArgumentNullException(nameof(cert));

			if (!cert.HasPrivateKey)
				throw new ArgumentException("The certificate must have a primary key.");

			_cert = cert;
		}

		public byte[] Protect(byte[] userData)
		{
			using (var rsa = _cert.GetRSAPublicKey())
			{
				return rsa.Encrypt(userData, RSAEncryptionPadding.Pkcs1);
			}
		}

		public byte[] Unprotect(byte[] protectedData)
		{
			using (var rsa = _cert.GetRSAPrivateKey())
			{
				return rsa.Decrypt(protectedData, RSAEncryptionPadding.Pkcs1);
			}
		}
	}
}
