using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	public class OnPremiseConnectionContext : IOnPremiseConnectionContext
	{
		public string ConnectionId { get; set; }
		public Guid LinkId { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime LastLocalActivity { get; set; } = DateTime.UtcNow;
		public Func<IOnPremiseConnectorRequest, CancellationToken, Task> RequestAction { get; set; }
		public string IpAddress { get; set; }
		public string UserName { get; set; }
		public string Role { get; set; }
		public int ConnectorVersion { get; set; }
		public string ConnectorAssemblyVersion { get; set; }
		public bool SupportsAck => ConnectorVersion >= 1;
		public bool SupportsHeartbeat => ConnectorVersion >= 2;
	}
}
