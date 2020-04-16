using System;

namespace Thinktecture.Relay.Abstractions
{
	public interface IRelayResponse : IRelayTask
	{
		DateTime? TargetStart { get; set; }
		TimeSpan? TargetDuration { get; set; }
	}
}
