using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal interface IRelayServerConnection : IDisposable
	{
		void RegisterOnPremiseTarget(string key, Uri baseUri);
	    String RelayedRequestHeader { set; }
	    Task Connect();
		void Disconnect();
	    List<string> GetOnPremiseTargetKeys();
	}
}