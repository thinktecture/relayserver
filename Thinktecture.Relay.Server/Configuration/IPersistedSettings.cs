using System;

namespace Thinktecture.Relay.Server.Configuration
{
	public interface IPersistedSettings
	{
		Guid OriginId { get; }
	}
}
