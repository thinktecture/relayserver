using System;
using System.Net.Http.Headers;
using System.Text;

namespace Thinktecture.Relay.OnPremiseConnector.IdentityModel
{
	/// <inheritdoc />
	public class BasicAuthenticationHeaderValue : AuthenticationHeaderValue
	{
		/// <inheritdoc />
		public BasicAuthenticationHeaderValue(string userName, string password)
			: base("Basic", EncodeCredentials(userName, password))
		{
		}

		private static string EncodeCredentials(string userName, string password)
		{
			var credential = String.Format("{0}:{1}", userName, password);

			return Convert.ToBase64String(Encoding.UTF8.GetBytes(credential));
		}
	}
}
