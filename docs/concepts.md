# RelayServer concepts

## Transports

The RelayServer has three concepts of transports, or protocols.

### Server to server transport

In a multi-server environment, the server that receives a request from a Client can be a different server than the one to which the tenants Connector has an active connection to. So the request needs to be sent to the Server with the active connection to the Connector first. This is done through the server-to-server transport. Typically this is a message queue. RelayServer is built around RabbitMQ, but the server transport can be implemented using other components as well.

### Server to connector transport

To make relaying possible without configuring port forwardings in a router or opening additional ports on firewalls, the Connector is meant to use HTTP to connect to the server. To have a reliable, steady connection websockets are preferable. RelayServer is built around ASP.NET Core SignalR, but the server transport can also be implemented using other components.

The Connector opens up a connection to a RelayServer and waits for requests to be sent through this transport. The transport also has a backchannel which will be used for handshake and server-side Connector configuration as well as heartbeating and acknowledgment.

### HTTP fallback

Both transports, depending on the actual used underlying technologies or systems, may introduce possibly different limits on the size of messages sent through them. The request and/or response body contents that should be relayed might be too large to fit in a message. In these cases the body is seperated from the actual request or response and transferred via normal HTTP GET (downloading of the request body from the Server to the Connector) or HTTP POST (uploading the response body from the Connector to the Server).

## Acknowledgment

The concept of acknowledgement comes from the underlying assumption that the server-to-server transport is based on some type of message queue. As long as a message is not acknowledged, it will remain in the message queue. This results in the possibility, that a request can be kept in the queue until we made sure it was processed by the target, even though the Client might have already aborted the originating request.

The default (`AcknowledgeMode.ConnectorReceived`), is sent by the Connector via the Connector transport backchannel when the request was received by the Connector and the body content was downloaded from the RelayServer if it is too big for the Connector transport.

With the second option (`AcknowledgeMode.ConnectorFinished`), the connector will wait until the Target responded to the request before acknowledging the message through the Connector transport backchannel.

The third option (`AcknowledgeMode.Manual`) shifts the responsibility of acknowledging to the consumer. The message will not be taken out of the queue anexplicit acknowledge HTTP POST request is sent to the RelayServer by some custom code. The url with all arguments required for acknowledging will be provided to the Target within the request as additional HTTP headers.

Acknowledgement can also be disabled (`AcknowledgeMode.Disabled`), meaning that any request will be acknowledged and thus taken completely out of the message queue automatically by the RelayServer, even before the request is sent to the Connector.

## Body content handling

Since transports may have limits on message sizes (i.e. RabbitMQ can send only up to 64kB), it is not possible to send any body through any transport. Body contents need to be persisted temporarily for processing. This is managed by an implementation of `IBodyStore`.

The decisions when to delete a content are as follows:

### Request body

When acknowledgment is enabled, the contents of the request body will be deleted when the request is acknowledged. This means either the connector has completely received the request and also requested the request body from a server (`AcknowledgeMode.ConnectorReceived`), or the connector already got the response from the target (`AcknowledgeMode.ConnectorFinished`).

When in manual mode (`AcknowledgeMode.Manual`), the acknowledgment must not be sent before the request body was loaded from the server.

When acknowledgement is disabled (`AcknowledgeMode.Disabled`), the connector to explicitly requests automatic file deletion when loading the request body from the server using the request endpoint.

### Response body

The persisted response body will automatically be deleted when the response has been send to the waiting client.
