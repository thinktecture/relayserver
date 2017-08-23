using System.Threading;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	internal class OnPremiseConnectorCallback : IOnPremiseConnectorCallback
	{
		public string RequestId { get; }
		public ManualResetEvent Handle { get; }
		public IOnPremiseConnectorResponse Response { get; set; }

		public OnPremiseConnectorCallback(string requestId)
		{
			RequestId = requestId;
			Handle = new ManualResetEvent(false);
		}
	}
}
