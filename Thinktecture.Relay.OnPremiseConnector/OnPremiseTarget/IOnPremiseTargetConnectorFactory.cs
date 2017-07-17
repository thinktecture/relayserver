using System;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal interface IOnPremiseTargetConnectorFactory
	{
		IOnPremiseTargetConnector Create(Uri baseUri, int requestTimeout);
		IOnPremiseTargetConnector Create(Type handlerType, int requestTimeout);
		IOnPremiseTargetConnector Create(Func<IOnPremiseInProcHandler> handlerFactory, int requestTimeout);
		IOnPremiseTargetConnector Create<T>(int requestTimeout) where T : IOnPremiseInProcHandler, new();
	}
}