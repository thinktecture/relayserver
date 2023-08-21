using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.IdentityModel.Tokens;

namespace Thinktecture.Relay.IdentityServer.Stores;

/// <summary>
/// A validation key store using rotating certificate files.
/// </summary>
internal class RotateValidationKeysFileStore : IValidationKeysStore
{
	private readonly RotateCertificateFileStore _fileStore;

	/// <summary>
	/// Initializes a new instance of the <see cref="RotateValidationKeysFileStore"/> class.
	/// </summary>
	/// <param name="fileStore">An instance of an <see cref="RotateCertificateFileStore"/>.</param>
	public RotateValidationKeysFileStore(RotateCertificateFileStore fileStore)
	{
		_fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
	}

	/// <inheritdoc/>
	public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
	{
		try
		{
			var keyInfos = _fileStore.GetCertificatesToAnnounce()
				.Select(c => new X509SecurityKey(c))
				.Select(k => new SecurityKeyInfo
				{
					Key = k,
					SigningAlgorithm = SecurityAlgorithms.RsaSha256,
				});

			return Task.FromResult(keyInfos);
		}
		catch (Exception ex)
		{
			return Task.FromException<IEnumerable<SecurityKeyInfo>>(ex);
		}
	}
}
