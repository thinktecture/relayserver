using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay
{
	/// <summary>
	/// Extension methods for <see cref="ITargetResponse"/>.
	/// </summary>
	public static class TargetResponseExtensions
	{
		/// <summary>
		/// Checks if the body content is currently outsourced.
		/// </summary>
		/// <param name="response">An <see cref="ITargetResponse"/>.</param>
		/// <returns>true, if the body content is outsourced; otherwise, false.</returns>
		public static bool IsBodyContentOutsourced(this ITargetResponse response) => response.BodySize > 0 && response.BodyContent == null;
	}
}
