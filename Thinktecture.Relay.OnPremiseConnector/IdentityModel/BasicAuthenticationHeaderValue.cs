using System;
using System.Net.Http.Headers;
using System.Text;

namespace Thinktecture.Relay.OnPremiseConnector.IdentityModel
{
	public class BasicAuthenticationHeaderValue : AuthenticationHeaderValue

	{
		public BasicAuthenticationHeaderValue(string userName, string password)
			: base("Basic", EncodeCredential(userName, password))

		{
		}

		private static string EncodeCredential(string userName, string password)
		{
			var encoding = Encoding.UTF8;
			var credential = String.Format("{0}:{1}", userName, password);

			return Convert.ToBase64String(encoding.GetBytes(credential));
		}
	}
}
