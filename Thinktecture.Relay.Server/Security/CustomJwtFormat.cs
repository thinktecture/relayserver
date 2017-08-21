using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Security;

namespace Thinktecture.Relay.Server.Security
{
	public class CustomJwtFormat : ISecureDataFormat<AuthenticationTicket>
	{
		private readonly TimeSpan _accessTokenExpirationTimespan;
		private readonly string _issuer;
		private readonly string _audience;
		private readonly byte[] _key;

		public CustomJwtFormat(TimeSpan accessTokenExpirationTimespan, byte[] key, string issuer, string audience)
		{
			_accessTokenExpirationTimespan = accessTokenExpirationTimespan;
			_issuer = issuer;
			_audience = audience;
			_key = key;
		}

		public string SignatureAlgorithm => "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";
		public string DigestAlgorithm => "http://www.w3.org/2001/04/xmlenc#sha256";

		public string Protect(AuthenticationTicket data)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));

			var now = DateTime.UtcNow;
			var expires = now.Add(_accessTokenExpirationTimespan);
			var signingCredentials = new SigningCredentials(new InMemorySymmetricSecurityKey(_key), SignatureAlgorithm, DigestAlgorithm);
			var token = new JwtSecurityToken(_issuer, _audience, data.Identity.Claims, now, expires, signingCredentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public AuthenticationTicket Unprotect(string protectedText)
		{
			throw new NotImplementedException();
		}
	}
}
