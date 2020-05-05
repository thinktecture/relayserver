using Microsoft.Extensions.DependencyInjection;

namespace Thinktecture.Relay.Connector.DependencyInjection
{
	/// <summary>
	/// Connector builder interface.
	/// </summary>
	public interface IRelayConnectorBuilder
	{
		/// <summary>
		/// Gets the application service collection.
		/// </summary>
		IServiceCollection Services { get; }
	}
}
