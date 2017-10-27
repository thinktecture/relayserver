using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Interceptors
{
	internal class InterceptedRequest : IInterceptedRequest
	{
		private readonly IWriteableOnPremiseTargetRequest _request;

		public byte[] Body { get => _request.Body; }

		public IDictionary<string, string> HttpHeaders { get => _request.HttpHeaders; }
		public string HttpMethod { get => _request.HttpMethod; set => _request.HttpMethod = value; }
		public string Url { get => _request.Url; set => _request.Url = value; }

		public string RequestId { get => _request.RequestId; }

		public Guid OriginId { get => _request.OriginId; }

		public string AcknowledgeId { get => _request.AcknowledgeId; }

		public IPAddress ClientIpAddress => _request.ClientIpAddress;

		IReadOnlyDictionary<string, string> IReadOnlyInterceptedRequest.HttpHeaders => new ReadOnlyDictionary<string, string>(_request.HttpHeaders);

		public InterceptedRequest(IWriteableOnPremiseTargetRequest request)
		{
			_request = request ?? throw new ArgumentNullException(nameof(request));
		}
	}
}
