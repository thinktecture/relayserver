using System;
using System.Text.Json.Serialization;

namespace Thinktecture.Relay
{
	/// <summary>
	/// Object that holds information to be displayed as a discovery document.
	/// </summary>
	public class DiscoveryDocument
	{
		/// <summary>
		/// The well-known relative path to the <see cref="DiscoveryDocument"/> endpoint.
		/// </summary>
		public const string WellKnownPath = ".well-known/relayserver-configuration";

		/// <summary>
		/// The version of the RelayServer.
		/// </summary>
		public string ServerVersion { get; set; }

		/// <summary>
		/// The url of the authorization server.
		/// </summary>
		public string AuthorizationServer { get; set; }

		/// <summary>
		/// The endpoint where the connector needs to connect to.
		/// </summary>
		public string ConnectorEndpoint { get; set; }

		/// <summary>
		/// The endpoint where requests should be acknowledged.
		/// </summary>
		public string AcknowledgeEndpoint { get; set; }

		/// <summary>
		/// The endpoint the connector should fetch large request bodies from.
		/// </summary>
		public string RequestEndpoint { get; set; }

		/// <summary>
		/// The endpoint the connector should send responses to.
		/// </summary>
		public string ResponseEndpoint { get; set; }

		/// <summary>
		/// The connection timeout in seconds.
		/// </summary>
		public int ConnectionTimeout { get; set; }

		/// <summary>
		/// The minimum delay to wait for until a reconnect of a connector should be attempted again.
		/// </summary>
		/// <seealso cref="ReconnectMaximumDelay"/>
		[JsonConverter(typeof(TimeSpanJsonConverter))]
		public TimeSpan ReconnectMinimumDelay { get; set; }

		/// <summary>
		/// The maximum delay to wait for until a reconnect of a connector should be attempted again.
		/// </summary>
		/// <seealso cref="ReconnectMinimumDelay"/>
		[JsonConverter(typeof(TimeSpanJsonConverter))]
		public TimeSpan ReconnectMaximumDelay { get; set; }
	}
}
