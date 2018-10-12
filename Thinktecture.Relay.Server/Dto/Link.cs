using System;

namespace Thinktecture.Relay.Server.Dto
{
	public class Link
	{
		public Guid Id { get; set; }
		public bool IsDisabled { get; set; }
		public string SymbolicName { get; set; }
		public bool AllowLocalClientRequestsOnly { get; set; }
		public bool ForwardOnPremiseTargetErrorResponse { get; set; }
	}
}
