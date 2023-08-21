# RelayServer configuration

## Server

The `RelayServerOptions` type provides the main configuration for the server. These are
the available settings:

```
{
    "RelayServer": {
      // Enable connector transport shortcut, Boolean, defaults to false
      // When enabled, a request can be send directly to a connector
      "EnableConnectorTransportShortcut": false,
      
      // Enable server transport shortcut, Boolean, defaults to false
      // When enabled, a response can be send directly to the client
      "EnableServerTransportShortcut": false,
      
      // Enpoint timeout, Timespan, defaults to 2 minutes
      // A connector will wait this long for any endpoint on the relay server,
      // i.e. loading an outsourced body, sending an acknowledgment or the response.
      "EndpointTimeout": "00:02:00",
      
      // Request expiration, Timespan, defaults to 2 minutes
      // The expiration time of a request until a response must be received from the connector.
      "RequestExpiration": "00:02:00",
      
      // Reconnect minimum delay, Timespan, defaults to 30 seconds
      // When a connector gets disconnected, it will wait at least this time before
      // attempting to reconnect.
      "ReconnectMinimumDelay": "00:00:30",
      
      // Reconnect maximum delay, Timespan, defaults to 5 minutes
      // When a connector gets disconnected, it will wait at most this time before
      // attempting to reconnect.
      "ReconnectMaximumDelay": "00:05:00",
      
      // Handshake timeout, Timespan, defaults to 15 seconds
      // The time in which a handshake between the server and a connector needs to be
      // completed before rejecting the connection.
      "HandshakeTimeout": "00:00:15",
      
      // Keep alive interval, Timespan, defaults to 15 seconds
      // The interval in which a keep alive ping is expected between connector and
      // server. By default the SignalR keepalive is set to this value.
      "KeepAliveInterval": "00:00:15",
      
      // Request logger level, FLAGGED ENUM, defaults to "All"
      // The verbosity of the IRelayRequestLogger.
      // Values: 0 = None, 1 = Succeeded, 2 = Aborted, 4 = Failed, 8 = Expired,
      // 16 = Errored, All = Succeeded | Aborted | Failed | Expired | Errored
      "RequestLoggerLevel": "All",

      // Acknowledge mode, ENUM, defaults to "Disabled"
      // Defines the mode in which requests are acknowledged on the server transport.
      // Values: 0 = Disabled, 1 = ConnectorReceived, 2 = ConnectorFinished, 3 = Manual
      "AcknowledgeMode": "Disabled",

      // Tenant info cache duration, Timespan, defaults to 10 seconds
      // Defines how long the tenant info will be cached before the database is queried
      // again.
      "TenantInfoCacheDuration": "00:00:10",

      // Require active connection, boolean, defaults to false
      // When enabled, a request is immediately rejected with a 503 Service unavailable
      // when there is no connector connected for the requested tenant. Otherwise the
      // request will time out a bit later.
      "RequireActiveConnection": false,
  }
}
```

The `MaintenanceOptions` type provides the maintenance configuration for the server.
These are the available settings:

```
{
   "Maintenance": {
      // Run interval, TimeSpan, defaults to 15 minutes
      // The interval in which maintenance jobs will be run.
      "RunInterval": "00:15:00",
   }
}
```

The `StatisticsOptions` type provides the statistic data configuration for the server.
These are the available settings:

```
{
   "Statistics": {
      // Entry max age, TimeSpan, defaults to 15 minutes
      // The time span to keep stale or closed connection and origin entries in the
      // statistics store.
      "EntryMaxAge": "00:15:00",
      
      // Origin last seen update interval, TimeSpan, defaults to 5 minutes
      // The time interval in which the origin's last seen timestamp will be updated.
      "OriginLastSeenUpdateInterval": "00:05:00",
      
      // Enable connection cleanup, boolean, defaults to false
      // Indicates whether to clean up stale or closed connections or not.
      "EnableConnectionCleanup": false,
   }
}
```


### Corresponding features

#### Transport shortcuts

When a client request reaches a RelayServer instance and the requested tenant has a
connector that is currently connected to this very same RelayServer instance, it is
possible to send the request directly to that connector without going through the
message queue.

If a response is sent to a RelayServer instance from a connector, and the client waiting
for the response also is connected to the very same RelayServer instance, it is again
possible to use a shortcut and directly deliver the response without sending it through
the message queue.

#### Reconnect delay

The reconnect delay is a feature that helps to prevent accidental self-DDoS (distributed
denial of service) scenarios.

If, for example, RelayServer gets updated, then the old instances shut down and the new
instances spin up. This will disconnect _all_ connectors from your RelayServer at once.
If all these connectors would try to reconnect immediately, this would mean a lot of
different IPs requesting your service at once. Depending on your hosting infrastructure
or your hosting service provide, this might be recognized as the beginning of a DDoS
attack, leading to the IP addresses getting blocked from the provider.

The reconnect delay creates a larger time frame for the connectors to reconnect. Each
connector will determine a random point in time, within the minimum and maximum limits,
at which to start reconnecting. Randomizing the value should lead to a evenly
distributed reconnect load.

Suggestion: The more connectors there are in a given relay system, the larger the
reconnect time window should be to keep the reconnect attempts per second low.

### Require active connection

When a request is received from a client, the RelayServer checks if the requested tenant
exists in the database. When `RequireActiveConnection` is enabled, it will also check if
there are any active connections listed in the database. If this is not the case, the
request will be immediately rejected with a 503 - Service unavailable status code.

__Caution__: This feature relies on the connection info from the database as one
RelayServer instance does not directly know if there is a connection available on
another RelayServer instance. If a RelayServer instance with active connections to it
does not shut down gracefully, i.e. because of a hardware issue killing all containers
running on a kubernetes node, the connections to that instance will still be listed as
active in the database. They will remain in the database until the maintenance job
removes the the old origin and all of its stale connections.

A stale origin is defined by the `EntryMaxAge` statistics option, and the maintenance
job is executed as often as the `RunInterval` maintenance option defines. With the
default settings, unavailable connections will be kept for at least 15 up to 30 minutes.
This will lead to requests for these tenants not being immediately rejected for that
time.

We suggest that you run the maintenance job more often by reducing the `RunInterval`
maintenance option, and to not keep old origins / RelayServer instances with their
connections as long in the database by also reducing the `EntryMaxAge` statistics
option. The last activity of an origin should be updated at least twice within this
range, so you might also want to reduce the `OriginLastSeenUpdateInterval` statistics
option. 
