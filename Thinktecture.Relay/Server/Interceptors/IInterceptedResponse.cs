using System;
using System.Collections.Generic;
using System.Net;

namespace Thinktecture.Relay.Server.Interceptors
{
	/// <summary>
	/// A response being provided to <see cref="IOnPremiseResponseInterceptor"/>.
	/// </summary>
	public interface IInterceptedResponse
	{
		string RequestId { get; }
		Guid OriginId { get; }
		IDictionary<string, string> HttpHeaders { get; }
		HttpStatusCode StatusCode { get; set; }
	}
}
