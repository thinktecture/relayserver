using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.IO;
using Newtonsoft.Json;

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
		[JsonIgnore]

		public Stream Stream { get; set; }

		private IDictionary<string, string> _httpHeaders;
		public IDictionary<string, string> HttpHeaders
		{
			get => _httpHeaders;
			set => _readonlyHeaders = new ReadOnlyDictionary<string, string>(_httpHeaders = value);
			}

		public IPAddress ClientIpAddress { get; set; }

		private IReadOnlyDictionary<string, string> _readonlyHeaders;
		IReadOnlyDictionary<string, string> IOnPremiseTargetRequest.HttpHeaders => _readonlyHeaders;
	}
}
