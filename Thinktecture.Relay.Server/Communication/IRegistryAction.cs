using System;

namespace Thinktecture.Relay.Server.Communication
{
	public  interface IRegistryAction
	{
		DateTime CreationDate { get; }
		bool IsRegistration { get; }
		string ConnectionId { get; }
		void Execute();
	}
}