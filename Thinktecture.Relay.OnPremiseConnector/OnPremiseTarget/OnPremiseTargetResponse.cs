using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Thinktecture.Relay.Server.Plugins;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseTargetResponse : IOnPremiseTargetResponse, IInterceptedResponse
	{
		public string RequestId { get; set; }
		public Guid OriginId { get; set; }
		public byte[] Body { get; set; }
		public DateTime RequestStarted { get; set; }
		public DateTime RequestFinished { get; set; }
		public HttpStatusCode StatusCode { get; set; }

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

		private IReadOnlyDictionary<string, string> _readonlyHeaders;
		IReadOnlyDictionary<string, string> IOnPremiseTargetResponse.HttpHeaders => _readonlyHeaders ?? (_readonlyHeaders = new ReadOnlyDictionary<string, string>(HttpHeaders));
	}
}
