using System;
using System.Threading.Tasks;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal interface IOnPremiseTargetConnector
	{
		Task<IOnPremiseTargetReponse> GetResponseAsync(string url, IOnPremiseTargetRequest onPremiseTargetRequest);
		Uri BaseUri { get; }
	}
}