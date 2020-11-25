using System;

namespace Thinktecture.Relay.Connector.Options
{
	/// <summary>
	/// Options for a web target.
	/// </summary>
	// ReSharper disable once ClassNeverInstantiated.Global
	public class RelayWebTargetOptions
	{
		/// <summary>
		/// The url to the target.
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// An optional <see cref="TimeSpan"/> as the request timeout.
		/// </summary>
		public TimeSpan? Timeout { get; set; }
	}
}
