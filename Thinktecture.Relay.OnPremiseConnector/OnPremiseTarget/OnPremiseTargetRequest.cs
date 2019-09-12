using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseTargetRequest : IOnPremiseTargetRequestInternal
	{
		public string RequestId { get; set; }
		public Guid OriginId { get; set; }
		public string AcknowledgeId { get; set; }
		public string HttpMethod { get; set; }
		public string Url { get; set; }
		public IReadOnlyDictionary<string, string> HttpHeaders { get; set; }
		public byte[] Body { get; set; }
		public AcknowledgmentMode AcknowledgmentMode { get; set; }
		public Guid AcknowledgeOriginId { get; set; }
		public string ConnectionId { get; set; }

		[JsonIgnore]
		public Stream Stream { get; set; }
		[JsonIgnore]
		public bool IsHeartbeatRequest => HttpMethod == "HEARTBEAT";
		[JsonIgnore]
		public bool IsPingRequest => HttpMethod == "PING";
		[JsonIgnore]
		public bool IsHeartbeatOrPingRequest => IsHeartbeatRequest || IsPingRequest;
		[JsonIgnore]
		public bool IsConfigurationRequest => HttpMethod == "CONFIG";
	}
}
