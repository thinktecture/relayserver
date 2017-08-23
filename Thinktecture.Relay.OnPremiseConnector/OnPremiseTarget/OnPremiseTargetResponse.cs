using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseTargetResponse : IOnPremiseTargetResponse
	{
		public string RequestId { get; set; }
		public Guid OriginId { get; set; }

        public HttpStatusCode StatusCode { get; set; }
        public IDictionary<string, string> HttpHeaders { get; set; }
        IReadOnlyDictionary<string, string> IOnPremiseTargetResponse.HttpHeaders => HttpHeaders != null ? new ReadOnlyDictionary<string, string>(HttpHeaders) : null;
        public byte[] Body { get; set; }

		public DateTime RequestStarted { get; set; }
		public DateTime RequestFinished { get; set; }
	}
}
