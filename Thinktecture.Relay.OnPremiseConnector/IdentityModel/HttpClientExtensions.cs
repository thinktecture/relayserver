using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Thinktecture.Relay.OnPremiseConnector.IdentityModel
{
	/// <summary>
	/// Provides extension methods for the <see cref="HttpClient"/>.
	/// </summary>
	public static class HttpClientExtensions
	{
		/// <summary>
		/// Sets headers for basic authentication.
		/// </summary>
		/// <param name="client">The <see cref="HttpClient"/> to set the headers on.</param>
		/// <param name="userName">The username to set.</param>
		/// <param name="password">The password to set.</param>
		public static void SetBasicAuthentication(this HttpClient client, string userName, string password)
		{
			if (client == null) throw new ArgumentNullException(nameof(client));
			if (userName == null) throw new ArgumentNullException(nameof(userName));
			if (password == null) throw new ArgumentNullException(nameof(password));

			client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(userName, password);
		}

		/// <summary>
		/// Sets headers for token authentication.
		/// </summary>
		/// <param name="client">The <see cref="HttpClient"/> to set the headers on.</param>
		/// <param name="scheme">The authentication scheme to use.</param>
		/// <param name="token">The token to set.</param>
		public static void SetToken(this HttpClient client, string scheme, string token)
		{
			if (client == null) throw new ArgumentNullException(nameof(client));
			if (scheme == null) throw new ArgumentNullException(nameof(scheme));
			if (token == null) throw new ArgumentNullException(nameof(token));

			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
		}

		/// <summary>
		/// Gets header value for token authentication.
		/// </summary>
		/// <param name="client">The <see cref="HttpClient"/> to get the header value from.</param>
		public static string GetToken(this HttpClient client)
		{
			if (client == null) throw new ArgumentNullException(nameof(client));

			return client.DefaultRequestHeaders?.Authorization?.Parameter;
		}

		/// <summary>
		/// Sets headers for bearer token authentication.
		/// </summary>
		/// <param name="client">The <see cref="HttpClient"/> to set the headers on.</param>
		/// <param name="token">The token to set as a bearer token.</param>
		public static void SetBearerToken(this HttpClient client, string token)
		{
			if (client == null) throw new ArgumentNullException(nameof(client));
			if (token == null) throw new ArgumentNullException(nameof(token));

			client.SetToken("Bearer", token);
		}
	}
}
