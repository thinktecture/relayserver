using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

		private Dictionary<string, string> _httpHeaders;
		public Dictionary<string, string> HttpHeaders
		{
			get => _httpHeaders ?? (_httpHeaders = new Dictionary<string, string>());
			set
			{
				_httpHeaders = value;
				_readonlyHeaders = null;
			}
		}

		private IReadOnlyDictionary<string, string> _readonlyHeaders;
		IReadOnlyDictionary<string, string> IOnPremiseTargetRequest.HttpHeaders => _readonlyHeaders ?? (_readonlyHeaders = new ReadOnlyDictionary<string, string>(HttpHeaders));
	}
}
