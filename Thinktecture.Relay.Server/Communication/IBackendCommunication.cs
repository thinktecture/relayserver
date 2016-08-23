using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.OnPremiseConnector.SignalR;
using Thinktecture.Relay.OnPremiseConnector.SignalR.Messages;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IBackendCommunication : IDisposable
	{
		string OriginId { get; }
		Task<IOnPremiseTargetReponse> GetResponseAsync(string requestId);
		Task SendOnPremiseConnectorRequest(string onPremiseId, IOnPremiseConnectorRequest onPremiseConnectorRequest);
	    void RegisterOnPremise(RegistrationInformation registrationInformation);
		void UnregisterOnPremise(string connectionId);
		Task SendOnPremiseTargetResponse(string originId, IOnPremiseTargetReponse reponse);
	    bool IsRegistered(string connectionId);
	    List<string> GetConnections(string linkId);
	    void HeartbeatReceived(string connectionId);
	    void EnableConnectionFeatures(Features features, string connectionId);
	}
}