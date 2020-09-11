namespace Thinktecture.Relay.ManagementApi.Docker.Controllers
{
	/// <summary>
	/// Holds a secret.
	/// </summary>
	public class Secret
	{
		/// <summary>
		/// A secret, that can be used to authenticate against a security token service.
		/// </summary>
		public string AuthenticationSecret { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Secret"/> class.
		/// </summary>
		/// <param name="secret">The secret to show.</param>
		public Secret(string secret)
		{
			AuthenticationSecret = secret;
		}
	}
}
