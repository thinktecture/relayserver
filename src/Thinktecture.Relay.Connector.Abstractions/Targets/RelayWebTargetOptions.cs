using System;

namespace Thinktecture.Relay.Connector.Targets
{
	/// <summary>
	/// Used to set additional web target options.
	/// </summary>
	[Flags]
	public enum RelayWebTargetOptions
	{
		/// <summary>
		/// No additional options.
		/// </summary>
		/// <remarks>This is the default value.</remarks>
		None = 0,

		/// <summary>
		/// Enables following redirects.
		/// </summary>
		FollowRedirect = 1
	}
}
