using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.Communication
{
	public class RegistrationInformation
	{
		public string ConnectionId { get; set; }
		public Guid LinkId { get; set; }
		public Func<IOnPremiseTargetRequest, CancellationToken, Task> RequestAction { get; set; }
		public string IpAddress { get; set; }
		public string UserName { get; set; }
		public string Role { get; set; }
		public int ConnectorVersion { get; set; }

		public bool SupportsAck()
		{
			return ConnectorVersion >= 1;
		}

		public bool SupportsHeartbeat()
		{
			return ConnectorVersion >= 2;
		}
	}
}
