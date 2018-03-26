using Autofac;

namespace Thinktecture.Relay.Server.SignalR
{
	internal interface IDataStoreLoader
	{
		void RegisterDataStore(ContainerBuilder builder);
	}
}
