using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.Communication
{
	internal class HeartbeatInformation
	{
		public Guid LinkId { get; set; }
		public string ConnectionId { get; set; }
		public int ConnectorVersion { get; set; }
		public Func<IOnPremiseTargetRequest, CancellationToken, Task> RequestAction { get; set; }
	}
}
