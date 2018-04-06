using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IOnPremiseConnectionContext
	{
		string ConnectionId { get; set; }
		Guid LinkId { get; set; }
		bool IsActive { get; set; }
		DateTime LastLocalActivity { get; set; }
		Func<IOnPremiseConnectorRequest, CancellationToken, Task> RequestAction { get; set; }
		string IpAddress { get; set; }
		string UserName { get; set; }
		string Role { get; set; }
		int ConnectorVersion { get; set; }
		string ConnectorAssemblyVersion { get; set; }
		bool SupportsAck { get; }
		bool SupportsHeartbeat { get; }
		DateTime NextHeartbeat { get; set; }
	}
}
