using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseTargetRequest : IOnPremiseTargetRequest
	{
		public string RequestId { get; set; }
		public string HttpMethod { get; set; }
		public string Url { get; set; }
		public byte[] Body { get; set; }
		public Guid OriginId { get; set; }
		public string AcknowledgeId { get; set; }

		private IDictionary<string, string> _httpHeaders;
		public IDictionary<string, string> HttpHeaders
		{
			get => _httpHeaders ?? (_httpHeaders = new Dictionary<string, string>());
			set
			{
				_httpHeaders = value;
				_readonlyHeaders = null;
			}
		}

		public IPAddress ClientIpAddress { get; set; }

		private IReadOnlyDictionary<string, string> _readonlyHeaders;
		IReadOnlyDictionary<string, string> IOnPremiseTargetRequest.HttpHeaders => _readonlyHeaders ?? (_readonlyHeaders = new ReadOnlyDictionary<string, string>(HttpHeaders));
	}
}
