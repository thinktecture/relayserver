using System.Threading;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.Communication
{
	internal class OnPremiseConnectorCallback : IOnPremiseConnectorCallback
	{
		public string RequestId { get; }
		public ManualResetEvent Handle { get; }
		public IOnPremiseTargetResponse Response { get; set; }

		public OnPremiseConnectorCallback(string requestId)
		{
			RequestId = requestId;
			Handle = new ManualResetEvent(false);
		}
	}
}
