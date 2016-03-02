namespace Thinktecture.Relay.Server.Security
{
    public interface IPasswordHash
    {
        /// <summary>
        /// Generates a random password in length of x
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        byte[] GeneratePassword(int length);

        /// <summary>
        /// Creates a salted PBKDF2 hash of the password.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hash of the password.</returns>
        PasswordInformation CreatePasswordInformation(byte[] password);
        /// <summary>
        /// Validates a password (base64 string) given a hash of the correct one.
        /// </summary>
        /// <param name="password">The password to check.</param>
        /// <param name="correctHash">A hash of the correct password.</param>
        /// <returns>True if the password is correct. False otherwise.</returns>
        bool ValidatePassword(byte[] password, PasswordInformation passwordInformation);
    }
}