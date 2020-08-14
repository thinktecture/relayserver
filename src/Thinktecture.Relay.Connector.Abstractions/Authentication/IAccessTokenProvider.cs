using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Connector.Authentication
{
	/// <summary>
	/// Implementation that provides access tokens for server access.
	/// </summary>
	public interface IAccessTokenProvider
	{
		/// <summary>
		/// Retrieves an access token.
		/// </summary>
		/// <returns>A task that returns an access token.</returns>
		Task<string> GetAccessTokenAsync();
	}
}
