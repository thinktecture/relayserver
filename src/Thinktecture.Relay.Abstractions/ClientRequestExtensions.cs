using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay
{
	/// <summary>
	/// Extension methods for <see cref="IClientRequest"/>.
	/// </summary>
	public static class ClientRequestExtensions
	{
		/// <summary>
		/// Checks if the body content is currently outsourced.
		/// </summary>
		/// <param name="request">An <see cref="IClientRequest"/>.</param>
		/// <returns>true, if the body content is outsourced; otherwise, false.</returns>
		public static bool IsBodyContentOutsourced(this IClientRequest request) => request.BodySize > 0 && request.BodyContent == null;
	}
}
