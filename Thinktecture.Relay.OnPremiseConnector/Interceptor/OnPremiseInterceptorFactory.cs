using System;
using Microsoft.Extensions.DependencyInjection;

namespace Thinktecture.Relay.OnPremiseConnector.Interceptor
{
	internal class OnPremiseInterceptorFactory : IOnPremiseInterceptorFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public OnPremiseInterceptorFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public IOnPremiseRequestInterceptor CreateOnPremiseRequestInterceptor()
		{
			return _serviceProvider.GetService<IOnPremiseRequestInterceptor>();
		}

		public IOnPremiseResponseInterceptor CreateOnPremiseResponseInterceptor()
		{
			return _serviceProvider.GetService<IOnPremiseResponseInterceptor>();
		}
	}
}