using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Server.Dto
{
	public class Link
	{
		public Guid Id { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string SymbolicName { get; set; }
		public bool IsDisabled { get; set; }
		public bool ForwardOnPremiseTargetErrorResponse { get; set; }
		public bool AllowLocalClientRequestsOnly { get; set; }
		public int MaximumLinks { get; set; }
		public DateTime CreationDate { get; set; }
        public bool IsConnected { get; set; }
	    public List<string> Connections { get; set; }
	}
}