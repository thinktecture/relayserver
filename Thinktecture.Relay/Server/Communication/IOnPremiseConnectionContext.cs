using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	/// <summary>
	/// Represents the context for an on-premise connection.
	/// </summary>
	public interface IOnPremiseConnectionContext
	{
		/// <summary>
		/// The unique id of the connection.
		/// </summary>
		string ConnectionId { get; set; }

		/// <summary>
		/// The unique id of the link.
		/// </summary>
		Guid LinkId { get; set; }

		/// <summary>
		/// Marks the connection active or inactive.
		/// </summary>
		bool IsActive { get; set; }

		/// <summary>
		/// The <see cref="DateTime"/> of the last activity (e.g. a receiption of a heartbeat result).
		/// </summary>
		DateTime LastLocalActivity { get; set; }

		/// <summary>
		/// The request action called when the connection should handle an incoming relay request.
		/// </summary>
		Func<IOnPremiseConnectorRequest, CancellationToken, Task> RequestAction { get; set; }

		/// <summary>
		/// The IP address of the on-premise connector.
		/// </summary>
		string IpAddress { get; set; }

		/// <summary>
		/// The user name used to authenticate.
		/// </summary>
		string UserName { get; set; }

		/// <summary>
		/// The role assigned to the authenticated user.
		/// </summary>
		string Role { get; set; }

		/// <summary>
		/// The version of the on-premise connector.
		/// </summary>
		int ConnectorVersion { get; set; }

		/// <summary>
		/// The assembly version of the on-premise connector.
		/// </summary>
		string ConnectorAssemblyVersion { get; set; }

		/// <summary>
		/// Indicates if the on-premise connector supports acknowledging.
		/// </summary>
		bool SupportsAck { get; }

		/// <summary>
		/// Indicates if the on-premise connector supports heartbeating.
		/// </summary>
		bool SupportsHeartbeat { get; }

		/// <summary>
		/// Indicates if the on-premise connector supports server-side configuring.
		/// </summary>
		bool SupportsConfiguration { get; }

		/// <summary>
		/// The <see cref="DateTime"/> when the next heartbeat should be sent.
		/// </summary>
		DateTime NextHeartbeat { get; set; }
	}
}
