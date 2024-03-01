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
      "RequestLoggerLevel": "Errored",

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

      // Enable automatic tenant creation, Boolean, defaults to false
      // When enabled, a tenant will be automatically created if a connector connects
      // for the first time and the tenant was not found, yet.
      "EnableAutomaticTenantCreation": false,
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

### Automatic tenant creation

When this feature is switched on, a tenant will be automatically added to the database
the first time it connects to the RelayServer. The name of the tenant is retrieved from
the claim `client_id`. Optionally the display name will be used from the claim
`client_name` and the description from `client_description`.

__Caution__: As long as the token is valid, the tenant will be created. This means, adding
a valid login to your IDP creates the tenant on the RelayServer. No cleanup mechanism is
available, thus automatically created tenants need to be manually deleted when they are
not needed/wanted anymore.

### Require authentication

If a tenant has `RequireAuthentication` enabled in the database, the RelayServer only relays
when the request contains an access token from its own issuer and audience (e.g., it comes
from a connector). In any other case it returns 401.

### Maximum concurrent connector requests

Setting `MaximumConcurrentConnectorRequests` to a positive value greater than 0 and less than
65.536 limits the amount of requests which will be send to a connector while other requests
are still pending.

__Caution__: If the limit is activated, all requests for that tenant need to be switched to an
acknowledge mode `ConnectorFinished` when they are not set to `Manual`. This means, compared
to the `Disabled` mode, the request will be re-queued if an error raises during processing by the
connector and the acknowledgment isn't send.

## Connector

The `RelayConnectorOptions` type provides the main configuration for the connector. These
are the available settings:

```
{
    "RelayConnector": {
      // The url pointing to where the RelayServer is deployed
      "RelayServerBaseUri": "",

      // The tenant name of the connector
      // Used as client_id for authentication with the Identity Provider.
      "TenantName": "",

      // The tenant secret of the connector
      // Used as client_secret for authentication with the Identity Provider.
      "TenantSecret": "",

      // The local targets that this connector can request
      "Targets": {},

      // Use the Http KeepAlive feature when communication with the RelayServer
      // This can be disabled when certain network conditions cause issues.
      "UseHttpKeepAlive": true
   }
}
```

### Targets

Targets are local web api's that can be accessed through the RelayServer and Connector.

Targets are configured with a name, an internal url and additional settings. The `Type` is a type name
of a class that implements the `IRelayTarget` interface. The `Url` is the internal url of the target.

You can implement other in-process targets that are not web api's, and configure them here by specifying
the type name. The Connector will then instantiate your type and call the `HandleAsync` method whenever
a request is received for that target.

Examples:
```JSON
"Targets": {

   "swapi": {
      // Allows access to the Star Wars API
      "Type": "RelayWebTarget",
      "Url": "https://swapi.dev/",
      "Options": "FollowRedirect"
   },

   "tt": {
      // Allows access to the Thinktecture website
      "Type": "RelayWebTarget",
      "Url": "https://thinktecture.com",
      "Options": "FollowRedirect"
   },

   "exampleInProcTarget": {
      // Allows handling requests by code
      "Type": "MyCustomInprocTarget"
   }

}
```
