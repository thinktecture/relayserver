using System;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Diagnostics;

namespace Thinktecture.Relay.Server;

/// <summary>
/// Options for the server.
/// </summary>
public class RelayServerOptions
{
	/// <summary>
	/// The default request expiration.
	/// </summary>
	public static readonly TimeSpan DefaultRequestExpiration = TimeSpan.FromMinutes(2);

	/// <summary>
	/// The default endpoint timeout.
	/// </summary>
	public static readonly TimeSpan DefaultEndpointTimeout = TimeSpan.FromMinutes(2);

	/// <summary>
	///  The default request logger level.
	/// </summary>
	public static readonly RelayRequestLoggerLevel DefaultRequestLoggerLevel = RelayRequestLoggerLevel.Errored;

	/// <summary>
	///  The default acknowledge mode.
	/// </summary>
	public static readonly AcknowledgeMode DefaultAcknowledgeMode = AcknowledgeMode.Disabled;

	/// <summary>
	/// The default tenant info cache duration.
	/// </summary>
	public static readonly TimeSpan DefaultTenantInfoCacheDuration = TimeSpan.FromSeconds(10);

	/// <summary>
	/// Enables the shortcut processing for the connector transport (e.g. client request).
	/// </summary>
	public bool EnableConnectorTransportShortcut { get; set; }

	/// <summary>
	/// Enables the shortcut processing for the server transport (e.g. target response).
	/// </summary>
	public bool EnableServerTransportShortcut { get; set; }

	/// <summary>
	/// The timeout of a server endpoint accessed from a connector.
	/// </summary>
	/// <remarks>The default value is 2 minutes.</remarks>
	public TimeSpan EndpointTimeout { get; set; } = DefaultEndpointTimeout;

	/// <summary>
	/// The expiration time of a request until a response must be received.
	/// </summary>
	public TimeSpan? RequestExpiration { get; set; } = DefaultRequestExpiration;

	/// <summary>
	/// The minimum delay to wait for until a reconnect of a connector should be attempted again.
	/// </summary>
	/// <seealso cref="ReconnectMaximumDelay"/>
	public TimeSpan ReconnectMinimumDelay { get; set; } = DiscoveryDocument.DefaultReconnectMinimumDelay;

	/// <summary>
	/// The maximum delay to wait for until a reconnect of a connector should be attempted again.
	/// </summary>
	/// <seealso cref="ReconnectMinimumDelay"/>
	public TimeSpan ReconnectMaximumDelay { get; set; } = DiscoveryDocument.DefaultReconnectMaximumDelay;

	/// <summary>
	/// The number of seconds used to timeout the handshake between the server and a connector.
	/// </summary>
	/// <remarks>The concrete use is an implementation detail of the protocols.</remarks>
	public TimeSpan HandshakeTimeout { get; set; } = DiscoveryDocument.DefaultHandshakeTimeout;

	/// <summary>
	/// The interval used to send keep alive pings in seconds between the server and a connector.
	/// </summary>
	/// <remarks>The concrete use is an implementation detail of the protocols.</remarks>
	public TimeSpan KeepAliveInterval { get; set; } = DiscoveryDocument.DefaultKeepAliveInterval;

	/// <summary>
	/// The verbosity of the <see cref="IRelayRequestLogger{TRequest,TResponse}"/>.
	/// </summary>
	public RelayRequestLoggerLevel RequestLoggerLevel { get; set; } = DefaultRequestLoggerLevel;

	/// <summary>
	/// The <see cref="AcknowledgeMode"/> for requests.
	/// </summary>
	public AcknowledgeMode AcknowledgeMode { get; set; } = DefaultAcknowledgeMode;

	/// <summary>
	/// The duration how long the tenant info will be cached.
	/// </summary>
	public TimeSpan TenantInfoCacheDuration { get; set; } = DefaultTenantInfoCacheDuration;

	/// <summary>
	/// Immediately reject a request if there is no active connection available for the requested tenant.
	/// </summary>
	public bool RequireActiveConnection { get; set; }

	/// <summary>
	/// Enables automatic tenant creation on first connect.
	/// </summary>
	public bool EnableAutomaticTenantCreation { get; set; }
}
