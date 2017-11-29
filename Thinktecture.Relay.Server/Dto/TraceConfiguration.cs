using System;

namespace Thinktecture.Relay.Server.Dto
{
	public class TraceConfiguration
	{
		public Guid Id { get; set; }
		public Guid LinkId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public DateTime CreationDate { get; set; }
	}
}
