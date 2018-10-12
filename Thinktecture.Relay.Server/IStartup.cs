using Owin;

namespace Thinktecture.Relay.Server
{
	internal interface IStartup
	{
		void Configuration(IAppBuilder app);
	}
}
