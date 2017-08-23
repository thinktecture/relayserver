using System.Threading;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	internal interface IOnPremiseConnectorCallback
	{
		string RequestId { get; }
		ManualResetEvent Handle { get; }
		IOnPremiseConnectorResponse Response { get; set; }
	}
}
