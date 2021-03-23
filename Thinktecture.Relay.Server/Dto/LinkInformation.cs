using System;

namespace Thinktecture.Relay.Server.Dto
{
	public class LinkInformation
	{
		public Guid Id { get; set; }
		public bool IsDisabled { get; set; }
		public bool AllowLocalClientRequestsOnly { get; set; }
		public string SymbolicName { get; set; }
		public bool ForwardOnPremiseTargetErrorResponse { get; set; }
		public bool HasActiveConnection { get; set; }
		public Guid[] TraceConfigurationIds { get; set; }
	}

	internal class CachedLinkInformation
	{
		public LinkInformation LinkInformation { get; set; }
		public DateTime CacheExpiry { get; set; }
	}
}
