namespace Thinktecture.Relay.Server.Security
{
	/// <summary>
	/// Validates a password against given complexity rules
	/// </summary>
	public interface IPasswordComplexityValidator
	{
		/// <summary>
		/// Validates a provided password against a given set of complexity rules
		/// </summary>
		/// <param name="userName">The name of the user</param>
		/// <param name="password">The password to validate</param>
		/// <param name="errorMessage">The error message that will be displayed to the user when the password does not match the complexity rules</param>
		/// <returns>True, if the password can be used; otherwise false</returns>
		bool ValidatePassword(string userName, string password, out string errorMessage);
	}
}
