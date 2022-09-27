# RelayServer Modularity

RelayServer 3 is built with extensibility and modularity in mind.

In general, all modules that can be exchanged and plugged in are defined in the `Thinktecture.Relay.Server.Abstractions`
assembly or, for the connector side, in the `Thinktecture.Relay.Connector.Abstractions`.

## Server modules

### Persistence

The persistence module is used by RelayServer and the corresponding APIs to store and access information for tenants as
well as statistics.

Currently, four interfaces are to be implemented for the persistence module:

* `ITenantService`  
  Used to access and modify tenant data in the store.
* `IStatisticsService`  
  Used to access and modify statistics data in the store.
* `IRequestService`  
  Used to access and modify request data in the store.
* `IConnectionService`  
  Used to access and modify connection data in the store.

RelayServer comes with a single implementation based on Entity Framework Core. In addition to the service class
implementations that use the `RelayDbContext`, we also provide two assemblies prepared with EF Core migrations for
Microsoft SQL Server as well as PostgreSQL.

### BodyStore

The body store module is used by RelayServer to persist the body data of large requests and responses on a shared
storage medium, to enable multi-server environments.

The `IBodyStore` interface is to be implemented for the body store module.

RelayServer comes with an in-memory implementation for single-server use and a file storage based implementation for use
in multi-server environments. It is intended that a shared file volume is mapped into all running RelayServer containers
to use the file-based body store.

### Protocols

RelayServer uses two different communication protocols. The first one is the server-to-server protocol, the other one is
the communication channel to the connector.

#### Server-to-Server protocol

The server-to-server protocol is required for a multi-server environment when a request or response needs to be routed
to a different instance of RelayServer where the corresponding connector or requesting client is connected to.

If enabled, the server-to-server protocol can be short-circuited under certain circumstances when the corresponding
connector or client is connected  (see feature short-circuiting).

The interfaces to implement are:

* `IServerDispatcher`  
  Used to send response and acknowledge messages from one server instance to another, that are meant for the server.
* `IServerHandler`  
  Used to receive these server messages.
* `ITenantDispatcher`  
  Used to send requests from one server instance to another, that are meant for a tenant connector connected to that
  server.
* `ITenantHandler`  
  Used to receive these tenant messages.

RelayServer comes with a single implementation based on the RabbitMQ message queue system.

#### Server-to-Connector protocol

The server-to-connector protocol is used to send requests to a connected tenant and to receive messages from a
connector. This section describes only the server part of this protocol.

The interfaces to implement are:

* `ITenantConnectorAdapter`  
  Used to send a request to a connected tenant connector.
* `ITenantConnectorAdapterFactory`  
  Used to get hold of an adapter for a specific connected tenant.
* `IConnectorTransport`  
  Used to receive responses and acknowledgement messages from a connected tenant.

RelayServer comes with a single implementation based on the SignalR Core protocol.

## Connector Modules

### Connector-to-Server protocol

The connector-to-server protocol is used on the connector to receive requests for local execution and to send small
responses as well as acknowledgment messages back to the server. This section describes only the connector part of this
protocol.

The interfaces to implement are:

* `IConnectorConnection`  
  Represents the connection to the server that can be started / connected and stopped / disconnected.
* `IConnectorTransport`  
  Used to send responses as well as acknowlegement messages back to the server.

When the connector part receives a request from the server it has to call the `IClientRequestHandler.HandleAsync` method
and send the response back to the server using the connector transport.

RelayServer comes with a single implementation based on the SignalR Core protocol.
