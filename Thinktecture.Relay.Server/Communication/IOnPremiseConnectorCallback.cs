using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IOnPremiseConnectorCallback
	{
		string RequestId { get; }
		TaskCompletionSource<IOnPremiseConnectorResponse> Response { get; }
	}
}
