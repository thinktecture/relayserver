using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Connector.Options
{
	/// <summary>
	/// Options for relay target registration via dependency injection.
	/// </summary>
	public class RelayTargetOptions
	{
		/// <summary>
		/// A single target registration.
		/// </summary>
		public class RelayTargetRegistration
		{
			/// <summary>
			/// The unique id of the target.
			/// </summary>
			public string Id { get; set; } = default!;

			/// <summary>
			/// The <see cref="Type"/> of the target handling requests.
			/// </summary>
			public Type Type { get; set; } = default!;

			/// <summary>
			/// An optional <see cref="TimeSpan"/> when the target times out.
			/// </summary>
			/// <remarks>The default value is 100 seconds.</remarks>
			public TimeSpan? Timeout { get; set; }

			/// <summary>
			/// Constructor arguments not provided by the <see cref="IServiceProvider"/>.
			/// </summary>
			public object[] Parameters { get; set; } = default!;
		}

		/// <summary>
		/// The <see cref="List{T}"/> of <see cref="RelayTargetRegistration"/>.
		/// </summary>
		public List<RelayTargetRegistration> Targets { get; } = new List<RelayTargetRegistration>();
	}
}
