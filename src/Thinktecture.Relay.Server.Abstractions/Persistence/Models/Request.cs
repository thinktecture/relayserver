using System;
using System.Net;

namespace Thinktecture.Relay.Server.Persistence.Models
{
	/// <summary>
	/// Represents a fulfilled logged request.
	/// </summary>
	public class Request
	{
		/// <summary>
		/// The unique id of the tenant.
		/// </summary>
		public Guid TenantId { get; set; }

		/// <summary>
		/// The unique id of the request.
		/// </summary>
		public Guid RequestId { get; set; }

		/// <summary>
		/// The date and time of the request.
		/// </summary>
		public DateTimeOffset RequestDate { get; set; }

		/// <summary>
		/// The duration of the request in milliseconds.
		/// </summary>
		public long RequestDuration { get; set; }

		/// <summary>
		/// The URL relative to the <see cref="Target"/>.
		/// </summary>
		public string RequestUrl { get; set; } = default!;

		/// <summary>
		/// The name of the target.
		/// </summary>
		public string Target { get; set; } = default!;

		/// <summary>
		/// The HTTP method.
		/// </summary>
		public string HttpMethod { get; set; } = default!;

		/// <summary>
		/// The amount of bytes received by the requesting client.
		/// </summary>
		public long RequestBodySize { get; set; }

		/// <summary>
		/// The <see cref="HttpStatusCode"/> received from the target.
		/// </summary>
		/// <remarks>This is only available when the request succeeded.</remarks>
		public HttpStatusCode? HttpStatusCode { get; set; }

		/// <summary>
		/// The amount of bytes sent to the requesting client.
		/// </summary>
		/// <remarks>This is only available when the request succeeded.</remarks>
		public long? ResponseBodySize { get; set; }

		/// <summary>
		/// Indicates if the client canceled the request.
		/// </summary>
		/// <remarks>This means the client aborted the request.</remarks>
		public bool Aborted { get; set; }

		/// <summary>
		/// Indicates if the request failed on the connector side.
		/// </summary>
		/// <remarks>This means there was an exception during the request transport.</remarks>
		public bool Failed { get; set; }

		/// <summary>
		/// Indicates if the request timed out.
		/// </summary>
		/// <remarks>This means there was no response received until the timeout was reached.</remarks>
		public bool Expired { get; set; }

		/// <summary>
		/// Indicates if the request errored.
		/// </summary>
		/// <remarks>This means there was an exception during the request handling.</remarks>
		public bool Errored { get; set; }
	}
}
