using System;
using Serilog;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseInProcTargetConnector : OnPremiseInProcTargetConnectorBase
	{
		private readonly Func<IOnPremiseInProcHandler> _handlerFactory;

		public OnPremiseInProcTargetConnector(ILogger logger, TimeSpan requestTimeout, Type handlerType, bool logSensitiveData)
			: this(logger, requestTimeout, CreateFactory(handlerType), logSensitiveData)
		{
		}

		public OnPremiseInProcTargetConnector(ILogger logger, TimeSpan requestTimeout, Func<IOnPremiseInProcHandler> handlerFactory, bool logSensitiveData)
			: base(logger, requestTimeout, logSensitiveData)
		{
			_handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
		}

		private static Func<IOnPremiseInProcHandler> CreateFactory(Type type)
		{
			return () => (IOnPremiseInProcHandler)Activator.CreateInstance(type);
		}

		protected override IOnPremiseInProcHandler CreateHandler()
		{
			return _handlerFactory();
		}
	}

	internal class OnPremiseInProcTargetConnector<T> : OnPremiseInProcTargetConnectorBase
		where T : IOnPremiseInProcHandler, new()
	{
		public OnPremiseInProcTargetConnector(TimeSpan requestTimeout, ILogger logger, bool logSensitiveData)
			: base(logger, requestTimeout, logSensitiveData)
		{
		}

		protected override IOnPremiseInProcHandler CreateHandler()
		{
			return new T();
		}
	}
}
