using System;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	/// <inheritdoc />
	public class RelayWebTargetOptions : IRelayTargetOptions
	{
		/// <summary>
		/// The base <see cref="Uri"/> used in a HTTP request.
		/// </summary>
		public Uri BaseAddress { get; }

		/// <inheritdoc />
		public TimeSpan Timeout { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="RelayWebTargetOptions"/>.
		/// </summary>
		/// <param name="baseAddress">The base <see cref="Uri"/> used in a HTTP request.</param>
		/// <param name="timeout">The <see cref="TimeSpan"/> to wait before the request times out.</param>
		public RelayWebTargetOptions(Uri baseAddress, TimeSpan? timeout = null)
		{
			BaseAddress = baseAddress;
			Timeout = timeout ?? TimeSpan.FromSeconds(100);
		}
	}
}
