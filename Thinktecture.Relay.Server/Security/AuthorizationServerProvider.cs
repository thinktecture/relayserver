using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Security
{
	internal class AuthorizationServerProvider : OAuthAuthorizationServerProvider
	{
		private readonly ILinkRepository _linkRepository;
		private readonly IUserRepository _userRepository;

		public AuthorizationServerProvider(ILinkRepository linkRepository, IUserRepository userRepository)
		{
			_linkRepository = linkRepository;
			_userRepository = userRepository;
		}

		public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
		{
			context.Validated();

			return base.ValidateClientAuthentication(context);
		}

		public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
		{
			Guid linkId;
			if (_linkRepository.Authenticate(context.UserName, context.Password, out linkId))
			{
				var identity = new ClaimsIdentity(context.Options.AuthenticationType);
				identity.AddClaim(new Claim(identity.NameClaimType, context.UserName));
				identity.AddClaim(new Claim("OnPremiseId", linkId.ToString()));
				identity.AddClaim(new Claim(identity.RoleClaimType, "OnPremise"));

				context.Validated(identity);
			}

			if (!context.IsValidated)
			{
				var user = _userRepository.Authenticate(context.UserName, context.Password);

				if (user != null)
				{
					var identity = new ClaimsIdentity(context.Options.AuthenticationType);
					identity.AddClaim(new Claim(identity.NameClaimType, context.UserName));
					identity.AddClaim(new Claim(identity.RoleClaimType, "Admin"));
					identity.AddClaim(new Claim("UserId", user.Id.ToString()));

					context.Validated(identity);
				}
			}

			return base.GrantResourceOwnerCredentials(context);
		}
	}
}
