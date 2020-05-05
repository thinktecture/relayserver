using System;

namespace Thinktecture.Relay.Connector
{
	/// <summary>
	/// An implementation of options for an <see cref="IRelayTarget{TRequest,TResponse}"/>.
	/// </summary>
	public interface IRelayTargetOptions
	{
		/// <summary>
		/// Gets the <see cref="TimeSpan"/> to wait before the request times out.
		/// </summary>
		/// <remarks>The default value is 100 seconds.</remarks>
		TimeSpan Timeout { get; }
	}
}
