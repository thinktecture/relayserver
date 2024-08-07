using System;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Authentication;

internal partial class AccessTokenProvider : IAccessTokenProvider
{
	private readonly IClientAccessTokenManagementService _clientAccessTokenManagementService;
	private readonly ILogger _logger;

	public AccessTokenProvider(ILogger<AccessTokenProvider> logger,
		IClientAccessTokenManagementService clientAccessTokenManagementService)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_clientAccessTokenManagementService =
			clientAccessTokenManagementService ?? throw new ArgumentNullException(nameof(clientAccessTokenManagementService));
	}

	public Task<string?> GetAccessTokenAsync()
	{
		Log.RequestingAccessToken(_logger);
		return _clientAccessTokenManagementService.GetClientAccessTokenAsync(Constants.HttpClientNames.RelayServer);
	}
}
