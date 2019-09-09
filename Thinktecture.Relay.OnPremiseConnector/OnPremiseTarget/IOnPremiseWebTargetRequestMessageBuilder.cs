using System;
using System.Net.Http;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal interface IOnPremiseWebTargetRequestMessageBuilder
	{
		HttpRequestMessage CreateLocalTargetRequestMessage(Uri baseUri, string url, IOnPremiseTargetRequest request, string relayedRequestHeader, bool logSensitiveData);
	}
}
