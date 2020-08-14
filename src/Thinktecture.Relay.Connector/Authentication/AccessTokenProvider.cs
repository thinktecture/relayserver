using System;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.AccessTokenManagement;

namespace Thinktecture.Relay.Connector.Authentication
{
	internal class AccessTokenProvider : IAccessTokenProvider
	{
		private readonly IAccessTokenManagementService _accessTokenManagementService;

		public AccessTokenProvider(IAccessTokenManagementService accessTokenManagementService)
		{
			_accessTokenManagementService = accessTokenManagementService ?? throw new ArgumentNullException(nameof(accessTokenManagementService));
		}

		public Task<string> GetAccessTokenAsync()
			=> _accessTokenManagementService.GetClientAccessTokenAsync();
	}
}
