using System;
using Serilog;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseInProcTargetConnector : OnPremiseInProcTargetConnectorBase
	{
		private readonly Func<IOnPremiseInProcHandler> _handlerFactory;

		public OnPremiseInProcTargetConnector(ILogger logger, TimeSpan requestTimeout, Type handlerType)
			: this(logger, requestTimeout, CreateFactory(handlerType))
		{
		}

		public OnPremiseInProcTargetConnector(ILogger logger, TimeSpan requestTimeout, Func<IOnPremiseInProcHandler> handlerFactory)
			: base(logger, requestTimeout)
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
		public OnPremiseInProcTargetConnector(TimeSpan requestTimeout, ILogger logger)
			: base(logger, requestTimeout)
		{
		}

		protected override IOnPremiseInProcHandler CreateHandler()
		{
			return new T();
		}
	}
}
