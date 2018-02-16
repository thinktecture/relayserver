using System;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Thinktecture.Relay.OnPremiseConnector.IdentityModel
{
	public class TokenResponse
	{
		public string Raw { get; protected set; }
		public JObject Json { get; protected set; }

		public TokenResponse(string raw)
		{
			Raw = raw;
			Json = JObject.Parse(raw);
		}

		public TokenResponse(HttpStatusCode statusCode, string reason)
		{
			IsHttpError = true;
			HttpErrorStatusCode = statusCode;
			HttpErrorReason = reason;
		}

		public bool IsHttpError { get; }
		public HttpStatusCode HttpErrorStatusCode { get; }
		public string HttpErrorReason { get; }
		public string AccessToken => GetStringOrNull(OAuth2Constants.AccessToken);
		public string IdentityToken => GetStringOrNull(OAuth2Constants.IdentityToken);
		public string Error => GetStringOrNull(OAuth2Constants.Error);
		public bool IsError => (IsHttpError || !string.IsNullOrWhiteSpace(GetStringOrNull(OAuth2Constants.Error)));
		public long ExpiresIn => GetLongOrNull(OAuth2Constants.ExpiresIn);
		public string TokenType => GetStringOrNull(OAuth2Constants.TokenType);
		public string RefreshToken => GetStringOrNull(OAuth2Constants.RefreshToken);

		protected virtual string GetStringOrNull(string name)
		{
			if (Json != null && Json.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var value))
			{
				return value.ToString();
			}

			return null;
		}

		protected virtual long GetLongOrNull(string name)
		{
			if (Json != null && Json.TryGetValue(name, out var value))
			{
				if (long.TryParse(value.ToString(), out var longValue))
				{
					return longValue;
				}
			}

			return 0;
		}
	}
}
