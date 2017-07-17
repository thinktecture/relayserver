using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseInProcTargetConnector : OnPremiseInProcTargetConnectorBase
	{
		private readonly Func<IOnPremiseInProcHandler> _handlerFactory;

		public OnPremiseInProcTargetConnector(ILogger logger, int requestTimeout, Type handlerType)
			: this(logger, requestTimeout, CreateFactory(handlerType))
		{
		}

		public OnPremiseInProcTargetConnector(ILogger logger, int requestTimeout, Func<IOnPremiseInProcHandler> handlerFactory)
			: base(logger, requestTimeout)
		{
			if (handlerFactory == null)
				throw new ArgumentNullException(nameof(handlerFactory));

			_handlerFactory = handlerFactory;
		}

		private static Func<IOnPremiseInProcHandler> CreateFactory(Type type)
		{
			return () => (IOnPremiseInProcHandler) Activator.CreateInstance(type);
		}

		protected override IOnPremiseInProcHandler CreateHandler()
		{
			return _handlerFactory();
		}
	}

	internal class OnPremiseInProcTargetConnector<T> : OnPremiseInProcTargetConnectorBase
		where T : IOnPremiseInProcHandler, new()
	{
		public OnPremiseInProcTargetConnector(int requestTimeout, ILogger logger)
			: base(logger, requestTimeout)
		{
		}

		protected override IOnPremiseInProcHandler CreateHandler()
		{
			return new T();
		}
	}
}