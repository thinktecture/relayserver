# RelayServer concepts

## Body content handling

Since transports may have limits on message sizes (i.e. RabbitMQ can send only up to 64kB), it is not possible to send any body through any transport. Body contents need to be persisted temporarily for processing. This is managed by an implementation of `IBodyStore`.

The decisions when to delete a content are as follows:

### Request

When acknowledgment is enabled, the contents of the request body will be deleted when the request is acknowledged. This means either the connector has completely received the request and also requested the request body from a server (`AcknowledgeMode.ConnectorReceived`), or the connector already got the response from the target (`AcknowledgeMode.ConnectorFinished`).

When in manual mode (`AcknowledgeMode.Manual`), the acknowledgment must not be sent before the request body was loaded from the server.

When acknowledgement is disabled (`AcknowledgeMode.Disabled`), the connector to explicitly requests automatic file deletion when loading the request body from the server using the request endpoint.

### Response

The persisted response body will automatically be deleted when the response has been send to the waiting client.
