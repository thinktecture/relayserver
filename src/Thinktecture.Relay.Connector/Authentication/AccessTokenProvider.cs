using System;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Authentication
{
	internal class AccessTokenProvider : IAccessTokenProvider
	{
		private readonly IAccessTokenManagementService _accessTokenManagementService;
		private readonly ILogger<AccessTokenProvider> _logger;

		public AccessTokenProvider(ILogger<AccessTokenProvider> logger,
			IAccessTokenManagementService accessTokenManagementService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_accessTokenManagementService =
				accessTokenManagementService ?? throw new ArgumentNullException(nameof(accessTokenManagementService));
		}

		public Task<string?> GetAccessTokenAsync()
		{
			_logger.LogDebug("Requesting access token");
			return _accessTokenManagementService.GetClientAccessTokenAsync();
		}
	}
}
