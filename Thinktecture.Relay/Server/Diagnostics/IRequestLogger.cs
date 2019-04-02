using System;
using System.Net;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Diagnostics
{
	/// <summary>
	/// Represents a logger that is used to log all requests for a specific link.
	/// </summary>
	public interface IRequestLogger
	{
		/// <summary>
		/// Logs a request.
		/// </summary>
		/// <param name="request">The request to log.</param>
		/// <param name="response">The response to log.</param>
		/// <param name="linkId">The id of the link that should handle the request..</param>
		/// <param name="originId">The id of the RelayServer that received the request.</param>
		/// <param name="relayPath">The path of the request.</param>
		/// <param name="statusCode">The status code of the response.</param>
		void LogRequest(IOnPremiseConnectorRequest request, IOnPremiseConnectorResponse response, Guid linkId, Guid originId, string relayPath, HttpStatusCode? statusCode);
	}
}
