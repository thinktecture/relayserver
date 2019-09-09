using System;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal interface IOnPremiseTargetConnectorFactory
	{
		IOnPremiseTargetConnector Create(Uri baseUri, TimeSpan requestTimeout, bool followRedirects = true, bool logSensitiveData = true);
		IOnPremiseTargetConnector Create(Type handlerType, TimeSpan requestTimeout, bool logSensitiveData = true);
		IOnPremiseTargetConnector Create(Func<IOnPremiseInProcHandler> handlerFactory, TimeSpan requestTimeout, bool logSensitiveData = true);
		IOnPremiseTargetConnector Create<T>(TimeSpan requestTimeout, bool logSensitiveData = true) where T : IOnPremiseInProcHandler, new();
	}
}
