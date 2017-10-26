using System;
using Thinktecture.Relay.OnPremiseConnector;

namespace Thinktecture.Relay.Server.OnPremise
{
	public interface IOnPremiseConnectorRequest : IOnPremiseTargetRequest, IWriteableOnPremiseTargetRequest
	{
		DateTime RequestStarted { get; set; }
		DateTime RequestFinished { get; set; }
	}
}
