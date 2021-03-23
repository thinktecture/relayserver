using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Security
{
	internal class AuthorizationServerProvider : OAuthAuthorizationServerProvider
	{
		private readonly ILinkRepository _linkRepository;
		private readonly IUserRepository _userRepository;

		public const string OnPremiseIdClaimName = "OnPremiseId";

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

		public override Task GrantCustomExtension(OAuthGrantCustomExtensionContext context)
		{
			if ((context.GrantType == "renew_token")
			    && (context.OwinContext.Authentication.User?.Identity is ClaimsIdentity identity)
			    && identity.IsAuthenticated)
			{
				// If we still have a valid token for this user, issue a new one
				context.Validated(identity);
			}
			
			return base.GrantCustomExtension(context);
		}

		public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
		{
			if (_linkRepository.Authenticate(context.UserName, context.Password, out var linkId))
			{
				var identity = new ClaimsIdentity(context.Options.AuthenticationType);
				identity.AddClaim(new Claim(identity.NameClaimType, context.UserName));
				identity.AddClaim(new Claim(OnPremiseIdClaimName, linkId.ToString()));
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
