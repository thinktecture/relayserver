using System;

namespace Thinktecture.Relay.OnPremiseConnector.IdentityModel
{
	/// <inheritdoc />
	public class AuthenticationException : Exception
	{
		/// <inheritdoc />
		public AuthenticationException()
		{
		}

		/// <inheritdoc />
		public AuthenticationException(string message)
			: base(message)
		{
		}
	}
}
