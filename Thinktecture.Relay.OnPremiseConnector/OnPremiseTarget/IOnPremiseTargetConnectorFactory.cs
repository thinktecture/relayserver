using System;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal interface IOnPremiseTargetConnectorFactory
	{
		IOnPremiseTargetConnector Create(Uri baseUri, TimeSpan requestTimeout, bool followRedirects = true, bool logSensitiveData = false);
		IOnPremiseTargetConnector Create(Type handlerType, TimeSpan requestTimeout, bool logSensitiveData = false);
		IOnPremiseTargetConnector Create(Func<IOnPremiseInProcHandler> handlerFactory, TimeSpan requestTimeout, bool logSensitiveData = false);
		IOnPremiseTargetConnector Create<T>(TimeSpan requestTimeout, bool logSensitiveData = false) where T : IOnPremiseInProcHandler, new();
	}
}
