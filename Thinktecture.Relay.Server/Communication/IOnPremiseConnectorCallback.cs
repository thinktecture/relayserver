using System.Threading;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.Communication
{
	internal interface IOnPremiseConnectorCallback
	{
		string RequestId { get; }
		ManualResetEvent Handle { get; }
		IOnPremiseTargetReponse Reponse { get; set; }
	}
}