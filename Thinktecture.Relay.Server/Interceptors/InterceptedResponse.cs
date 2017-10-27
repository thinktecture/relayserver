using System;
using System.Collections.Generic;
using System.Net;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Interceptors
{
	public class InterceptedResponse : IInterceptedResponse
	{
		private readonly IOnPremiseConnectorResponse _response;

		public string RequestId => _response.RequestId;

		public Guid OriginId => _response.OriginId;

		public IDictionary<string, string> HttpHeaders { get { return _response.HttpHeaders; } }

		public HttpStatusCode StatusCode { get => _response.StatusCode; set => _response.StatusCode = value; }

		public InterceptedResponse(IOnPremiseConnectorResponse response)
		{
			_response = response ?? throw new ArgumentNullException(nameof(response));
		}
	}
}
