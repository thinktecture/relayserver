using System;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.OnPremise
{
	public interface IOnPremiseConnectorRequest : IOnPremiseTargetRequest, IWriteableOnPremiseTargetRequest
	{
		DateTime RequestStarted { get; set; }
		DateTime RequestFinished { get; set; }
	}
}
