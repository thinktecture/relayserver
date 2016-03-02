using System.Threading;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.Communication
{
	internal class OnPremiseConnectorCallback : IOnPremiseConnectorCallback
	{
		public string RequestId { get; private set; }
		public ManualResetEvent Handle { get; private set; }
		public IOnPremiseTargetReponse Reponse { set; get; }

		public OnPremiseConnectorCallback(string requestId)
		{
			RequestId = requestId;
			Handle = new ManualResetEvent(false);
		}
	}
}
