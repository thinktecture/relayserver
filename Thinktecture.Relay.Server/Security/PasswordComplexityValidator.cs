using System;
using System.Linq;

namespace Thinktecture.Relay.Server.Security
{
	public class PasswordComplexityValidator : IPasswordComplexityValidator
	{
		public bool ValidatePassword(string userName, string password, out string errorMessage)
		{
			errorMessage = null;

			// user must not use the username as password
			if (userName.Equals(password, StringComparison.InvariantCultureIgnoreCase))
			{
				errorMessage += "Username and password must not be the same.\r\n";
			}

			if (password.Length < 8)
			{
				errorMessage += "Password needs to be at least 8 characters long.\r\n";
			}

			if (!password.Any(Char.IsLower))
			{
				errorMessage += "Password must contain at least one lower case character.\r\n";
			}

			if (!password.Any(Char.IsUpper))
			{
				errorMessage += "Password must contain at least one upper case character.\r\n";
			}

			if (!password.Any(Char.IsDigit))
			{
				errorMessage += "Password must contain at least one number.\r\n";
			}

			if (password.All(Char.IsLetterOrDigit))
			{
				errorMessage += "Password must contain at least one special character.\r\n";
			}

			return String.IsNullOrWhiteSpace(errorMessage);
		}
	}
}
