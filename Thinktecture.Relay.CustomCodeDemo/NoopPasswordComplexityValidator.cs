using Thinktecture.Relay.Server.Security;

namespace Thinktecture.Relay.CustomCodeDemo
{
	internal class NoopPasswordComplexityValidator : IPasswordComplexityValidator
	{
		public bool ValidatePassword(string userName, string password, out string errorMessage)
		{
			errorMessage = null;
			return true;
		}
	}
}
