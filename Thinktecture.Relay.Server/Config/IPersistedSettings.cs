using System;

namespace Thinktecture.Relay.Server.Config
{
	public interface IPersistedSettings
	{
		Guid OriginId { get; }
	}
}
