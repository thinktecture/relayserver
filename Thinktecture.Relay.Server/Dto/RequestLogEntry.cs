using System;
using System.Net;

namespace Thinktecture.Relay.Server.Dto
{
	public class RequestLogEntry
	{
		public Guid LinkId { get; set; }
		public Guid OriginId { get; set; }
		public HttpStatusCode HttpStatusCode { get; set; }
		public string OnPremiseTargetKey { get; set; }
		public string LocalUrl { get; set; }
		public long ContentBytesIn { get; set; }
		public long ContentBytesOut { get; set; }
		public DateTime OnPremiseConnectorInDate { get; set; }
		public DateTime OnPremiseConnectorOutDate { get; set; }
		public DateTime? OnPremiseTargetInDate { get; set; }
		public DateTime? OnPremiseTargetOutDate { get; set; } 
	}
}