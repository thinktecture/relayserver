using System.Threading.Tasks;

namespace Thinktecture.Relay.Connector.Authentication
{
	/// <summary>
	/// An implementation that provides access tokens for server access.
	/// </summary>
	public interface IAccessTokenProvider
	{
		/// <summary>
		/// Retrieves an access token.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the access token.</returns>
		Task<string> GetAccessTokenAsync();
	}
}
