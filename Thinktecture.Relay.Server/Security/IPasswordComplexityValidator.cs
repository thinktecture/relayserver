namespace Thinktecture.Relay.Server.Security
{
	public interface IPasswordComplexityValidator
	{
		bool ValidatePassword(string userName, string password, out string errorMessage);
	}
}
