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
		/// The default reconnect minimum delay.
		/// </summary>
		public static TimeSpan DefaultReconnectMinimumDelay = TimeSpan.FromSeconds(30);

		/// <summary>
		/// The default reconnect maximum delay.
		/// </summary>
		public static TimeSpan DefaultReconnectMaximumDelay = TimeSpan.FromMinutes(5);

		/// <summary>
		/// The default handshake timeout.
		/// </summary>
		public static TimeSpan DefaultHandshakeTimeout = TimeSpan.FromSeconds(15);

		/// <summary>
		/// The default keep-alive interval.
		/// </summary>
		public static TimeSpan DefaultKeepAliveInterval = TimeSpan.FromSeconds(15);

		/// <summary>
		/// The well-known relative path to the <see cref="DiscoveryDocument"/> endpoint.
		/// </summary>
		public const string WellKnownPath = ".well-known/relayserver-configuration";

		/// <summary>
		/// The version of the RelayServer.
		/// </summary>
		public string ServerVersion { get; set; } = default!;

		/// <summary>
		/// The url of the authorization server.
		/// </summary>
		public string AuthorizationServer { get; set; } = default!;

		/// <summary>
		/// The endpoint where the connector needs to connect to.
		/// </summary>
		public string ConnectorEndpoint { get; set; } = default!;

		/// <summary>
		/// The endpoint where requests should be acknowledged.
		/// </summary>
		public string AcknowledgeEndpoint { get; set; } = default!;

		/// <summary>
		/// The endpoint the connector should fetch large request bodies from.
		/// </summary>
		public string RequestEndpoint { get; set; } = default!;

		/// <summary>
		/// The endpoint the connector should send responses to.
		/// </summary>
		public string ResponseEndpoint { get; set; } = default!;

		/// <summary>
		/// The timeout of a server endpoint accessed from a connector.
		/// </summary>
		[JsonConverter(typeof(TimeSpanJsonConverter))]
		public TimeSpan EndpointTimeout { get; set; }

		/// <summary>
		/// The minimum delay to wait for until a reconnect of a connector should be attempted again.
		/// </summary>
		/// <seealso cref="ReconnectMaximumDelay"/>
		[JsonConverter(typeof(TimeSpanJsonConverter))]
		public TimeSpan ReconnectMinimumDelay { get; set; } = DefaultReconnectMinimumDelay;

		/// <summary>
		/// The maximum delay to wait for until a reconnect of a connector should be attempted again.
		/// </summary>
		/// <seealso cref="ReconnectMinimumDelay"/>
		[JsonConverter(typeof(TimeSpanJsonConverter))]
		public TimeSpan ReconnectMaximumDelay { get; set; } = DefaultReconnectMaximumDelay;

		/// <summary>
		/// The timeout of the handshake between the server and a connector.
		/// </summary>
		/// <remarks>The concrete use is an implementation detail of the protocols.</remarks>
		[JsonConverter(typeof(TimeSpanJsonConverter))]
		public TimeSpan HandshakeTimeout { get; set; } = DefaultHandshakeTimeout;

		/// <summary>
		/// The interval used to send keep alive pings between the server and a connector.
		/// </summary>
		/// <remarks>The concrete use is an implementation detail of the protocols.</remarks>
		[JsonConverter(typeof(TimeSpanJsonConverter))]
		public TimeSpan KeepAliveInterval { get; set; } = DefaultKeepAliveInterval;
	}
}
