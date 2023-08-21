using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityServer4.Stores;
using Microsoft.IdentityModel.Tokens;

namespace Thinktecture.Relay.IdentityServer.Stores;

/// <summary>
/// A signing credential store using rotating certificate files.
/// </summary>
internal class RotateSigningCredentialFileStore : ISigningCredentialStore
{
	private readonly RotateCertificateFileStore _fileStore;

	/// <summary>
	/// Initializes a new instance of the <see cref="RotateSigningCredentialFileStore"/>.
	/// </summary>
	/// <param name="fileStore">An instance of an <see cref="RotateCertificateFileStore"/>.</param>
	public RotateSigningCredentialFileStore(RotateCertificateFileStore fileStore)
	{
		_fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
	}

	/// <inheritdoc/>
	public Task<SigningCredentials> GetSigningCredentialsAsync()
	{
		try
		{
			return Task.FromResult(ToSigningCredential(_fileStore.GetCurrentCertificate()));
		}
		catch (Exception ex)
		{
			return Task.FromException<SigningCredentials>(ex);
		}
	}

	private SigningCredentials ToSigningCredential(X509Certificate2 certificate)
		=> new SigningCredentials(new X509SecurityKey(certificate), SecurityAlgorithms.RsaSha256);
}
