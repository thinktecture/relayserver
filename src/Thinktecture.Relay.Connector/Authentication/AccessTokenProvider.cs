using System;
using System.Threading.Tasks;
using Duende.AccessTokenManagement;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Authentication;

internal partial class AccessTokenProvider : IAccessTokenProvider
{
	private readonly IClientCredentialsTokenManagementService _clientCredentialsTokenManagementService;
	private readonly ILogger _logger;

	public AccessTokenProvider(ILogger<AccessTokenProvider> logger,
		IClientCredentialsTokenManagementService clientCredentialsTokenManagementService)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_clientCredentialsTokenManagementService =
			clientCredentialsTokenManagementService ??
			throw new ArgumentNullException(nameof(clientCredentialsTokenManagementService));
	}

	public async Task<string?> GetAccessTokenAsync()
	{
		Log.RequestingAccessToken(_logger);
		var token =
			await _clientCredentialsTokenManagementService.GetAccessTokenAsync(Constants.RelayServerClientName);
		return token.AccessToken;
	}
}
