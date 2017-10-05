using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	internal class OnPremiseConnectorCallback : IOnPremiseConnectorCallback
	{
		public string RequestId { get; }
		public TaskCompletionSource<IOnPremiseConnectorResponse> Response { get; }

		public OnPremiseConnectorCallback(string requestId)
		{
			RequestId = requestId;
			Response = new TaskCompletionSource<IOnPremiseConnectorResponse>();
		}
	}
}
