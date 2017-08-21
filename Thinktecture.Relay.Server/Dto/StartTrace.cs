using System;

namespace Thinktecture.Relay.Server.Dto
{
	public class StartTrace
	{
		public Guid LinkId { get; set; }
		public int Minutes { get; set; }
	}
}
