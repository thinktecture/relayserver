# RelayServer concepts

Concepts explained in this document are:

- [Request processing](#request-processing)
- [Transports](#transports)
- [Acknowledgment](#acknowledgment)
- [Body content handling](#body-content-handling)
- [Authentication](#authentication)

## Request Processing

A request is processed in several stages in RelayServer.

### Request on the Server

1. `RelayMiddleware`
   - The relay middleware receives an incoming request from a client. The incoming url needs to be in the format
     `https://relayserver.tld/relay/{TenantName}/{TargetName}/{Path}`.
   - The middleware searches for the tenant and, if found, it starts processing the request.
   - If the request has a body, and it is larger than the maximum size any [transport](#transports) allows, it will be
     persisted using the configured `IBodyStore` implementation.
   - The middleware hands the request over to the request coordinator.
1. `RequestCoordinator`
   - If shortcutting is allowed by the configuration, the request coordinator will check if a connector for this tenant
     is currently connected to the server that processes this request. If this is the case, the request will directly be
     sent to the connector, skipping the next two steps.
1. `TenantDispatcher`  
   The tenant dispatcher is part of the RabbitMQ [server-to-server transport](#server-to-server-transport) and sends
   messages via the queue to the corresponding tenant handler.
   - If shortcutting is disabled (default), or enabled but no active connection is found on this server, the request
     will be dispatched via the server-to-server transport to another instance of the RelayServer that holds an active
     connection for the tenant.
1. `TenantHandler`  
   The tenant handler is the consumer on the RabbitMQ [server-to-server transport](#server-to-server-transport) that
   is instantiated for each connector connection and waits for messages targeted at a tenant.
   - One tenant handler reads the request from the server-to-server transport.
   - It writes the request to the TenantConnectorAdapter.
1. `TenantConnectionAdapter`  
   The tenant connection adapter is part of the SignalR [server-to-connector transport](#server-to-connector-transport).
   It gets called by either the request coordinator directly (shortcut) or by the tenant handler (which belongs to the
   server-to-server transport).
   - The request is sent down to the connector through the configured server-to-connector transport.

### Request on the connector

1. `ConnectorConnection`  
   The connector connection is part of the SignalR [server-to-connector transport](#server-to-connector-transport).
   - The server (`TenantConnectionAdapter`) calls a method on the connector connection, passing the request along.
   - The connector connection hands the request over to the client request handler.
1. `ClientRequestHandler`
   - The client request handler first checks if the requested target is configured. If this is not the case, it answers
     the request with a _404 Not Found_ status code.
   - If the request body was too large for the transport, it is requested with a separate http call from the server.
   - The complete request is then passed to the registered `IRelayTarget` to handle the request.
1. `RelayWebTarget`  
   The relay web target is the default `IRelayTarget` implementation.
   - The relay web target sends the request to the configured url.
   - The received response is passed back to the client request handler.

### Response on the connector

1. `ClientRequestHandler`
   - After receiving the response from the local target, the client request handler checks if the response body is large
     enough for the server-to-connector transport. If this is not the case, the response body will be http POSTed to the
     server and removed from the response.
   - The response is then passed back to the connector connection.
1. `ConnectorConnection`
   - The connector connection gets the response returned from the previous call to the client request handler.
   - The response (without a too large body) is sent back through the SignalR transport to the server.

### Response on the server

1. `ConnectorHub`  
   The connector hub is part of the SignalR [server-to-connector transport](#server-to-connector-transport).
   - The hub receives the response and hands it over to the response coordinator.
1. `ResponseDispatcher`  
   The server dispatcher is part of the RabbitMQ [server-to-server transport](#server-to-server-transport) and sends
   messages to other servers.
   - If shortcutting is disabled (default), or the client waiting for the response is connected to a different server,
     the server dispatcher puts the response in a message queue targeted to the server where the client waits for the
     response via the `ServerTransport`.
   - Otherwise (shortcutting is enabled and is possible) the response is directly passed on to the
     `ResponseCoordinator`.
1. `ResponseCoordinator`  
   - If shortcutting is allowed by the configuration, the response coordinator will check if the client is waiting for
     the response on the server that received this response. If this is the case, the response will be processed
     directly, skipping the next two steps.
1. `ServerTransport`  
   The server transport is part of the RabbitMQ [server-to-server transport](#server-to-server-transport). It is
   responsible for both sending as well as receiving messages.
   - When the `ResponseDispatcher` hands a response to the server transport, it will put a message into the queue for
     the corresponding origin server. 
   - When a response message is received, an event is raised by RabbitMQ to handle this. This event is handled by
     `ServerTransport` and the response is passed along to the `ResponseCoordinator` for further processing.
1. `ResponseCoordinator`  
   We are now on the server where the client is waiting for its response.
   - If the response body was too large for the transport, the body content is loaded from the body store.
   - The response is passed back to the relay middleware.
2. `RelayMiddleware`  
   - The relay middleware sends the response to the client waiting for it.

## Transports

The RelayServer has three concepts of transports, or protocols.

### Server-to-Server transport

In a multi-server environment, the server that receives a request from a client can be a different server than the one
to which the tenants connector has an active connection to. So the request needs to be sent to the server with the
active connection to the connector first. This is done through the server-to-server transport. Typically this is a
message queue. RelayServer is built around RabbitMQ, but the server transport can be implemented using other components
as well.

### Server-to-Connector transport

To make relaying possible without configuring port forwarding in a router or opening additional ports on firewalls, the
connector is meant to use HTTP to connect to the server. To have a reliable, steady connection websockets are
preferable. RelayServer is built around ASP.NET Core SignalR, but the server transport can also be implemented using
other components.

The connector opens up a connection to a RelayServer and waits for requests to be sent through this transport. The
transport also has a back channel, which will be used for handshake and server-side connector configuration as well as
heartbeats and acknowledgments.

### HTTP fallback

Transports, depending on the actual used underlying technologies or systems, may introduce possibly different limits on
the size of messages sent through them. For example, Microsoft suggests not to send [messages larger than 32kb through
SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/security?view=aspnetcore-6.0#buffer-management). The
request and/or response body contents that should be relayed might be too large to fit in a message. In these cases the
body is separated from the actual request or response and transferred via normal HTTP GET (downloading of the request
body from the Server to the connector) or HTTP POST (uploading the response body from the connector to the server).

## Acknowledgment

The concept of acknowledgement comes from the underlying assumption that the server-to-server transport is based on some
type of message queue. As long as a message is not acknowledged, it will remain in the message queue. This results in
the possibility, that a request can be kept in the queue until we made sure it was processed by the target, even though
the client might have already aborted the originating request. _Note:_ This is a very rare use case for asynchronus
messaging through RelayServer, and not the intended usage.

The default (`AcknowledgeMode.ConnectorReceived`), is sent by the connector via the connector transport back channel
when the request was received by the connector and the body content was downloaded from the RelayServer if it is too big
for the connector transport.

With the second option (`AcknowledgeMode.ConnectorFinished`), the connector will wait until the Target responded to the
request before acknowledging the message through the connector transport back-channel.

The third option (`AcknowledgeMode.Manual`) shifts the responsibility of acknowledging to the consumer. The message will
not be taken out of the queue until an explicit acknowledge HTTP POST request is sent to the RelayServer by some custom
code. The url with all arguments required for acknowledging will be provided to the target within the request as an
additional HTTP header.

Acknowledgement can also be disabled (`AcknowledgeMode.Disabled`), meaning that any request will be acknowledged and
thus taken completely out of the message queue automatically by the RelayServer, even before the request is sent to the
connector.

## Body content handling

As mentioned above, transports may have limits on message sizes. So it is not possible to send arbitrary bodies through
any transport. Larger body contents need to be persisted temporarily for processing. This is managed by an
implementation of the `IBodyStore` interface.

The decisions when to delete content are as follows:

### Request body

When acknowledgment is enabled, the contents of the request body will be deleted when the request is acknowledged. This
means either the connector has completely received the request and also requested the request body from a server
(`AcknowledgeMode.ConnectorReceived`), or the connector already got the response from the target
(`AcknowledgeMode.ConnectorFinished`).

When in manual mode (`AcknowledgeMode.Manual`), the acknowledgment must not be sent before the request body was loaded
from the server.

When acknowledgement is disabled (`AcknowledgeMode.Disabled`), the connector to explicitly requests automatic file
deletion when loading the request body from the server using the request endpoint.

### Response body

The persisted response body will automatically be deleted when the response has been send to the waiting client.

## Authentication

A Connector needs to authenticate itself against the RelayServer. This is done by providing a valid access token. The
token will be retrieved from the `Authority` that the RelayServer is configured to use. The connector will retrieve the
url to the token provider from the `.well-known/relayserver-configuration` endpoint on the RelayServer.

The identity provider needs to be an OIDC compliant service, like Entra ID, Keycloak, Auth0, IdentityServer or others.

The token needs to provide at least the following values in its claims:
* `client_id`: the name of the tenant as it is configured in the RelayServer database
* `aud` (audience): `relayserver`, only when the RelayServer knows that this token is intended for it, it will accept it
* `scope`: `connector`, the endpoints on the RelayServer that the connector needs to call require this scope

The configuration of the tenant in the RelayServer database can be omitted if the server is configured for automatic
tenant creation (see `Automatic Tenant Creation` in the [Configuration](./configuration.md#automatic-tenant-creation)
documentation.
