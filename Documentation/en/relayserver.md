# [What is Thinktecture RelayServer?](1-what-is-thinktecture-relayserver.md)
# [RelayServer Architecture](2-architecture.md)
# [RelayServer Installation](3-installation.md)
# [RelayServer Management Web](4-relayserver-management-web.md)
# [RelayServer Plugin Development](5-relayserver-interceptor-development.md)
# [Development Setup](6-development-setup.md)

# Sponsors
The goal of this list is to highlight companies who pay back to this open source project which is helping them to save time and money in their own projects.

## Sponsors from Germany
[<img width="120px" src="./assets/logo_sponsor_kwp.svg" />](https://www.kwpsoftware.de)

## Sponsors from Switzerland
[<img width="120px" src="./assets/logo_sponsor_cmi.svg" />](https://www.cmiag.ch/)
[<img width="120px" src="./assets/logo_sponsor_abraxas.png" />](https://www.abraxas.ch/)

# Version history

## Version 2.3.0

* RabbitMQ Improvements

  * Closed Rabbit connections will now be automatically unbound at the client.
  * The automatic recovery feature of the rabbit client will now be enabled by default.
  * When a channel gets closed, make sure all unacknowledged messages get re-queued.
  * To improve throughput, multiple channels (via RoutingKeys) will be used for requests, responses and acknowledgements.

* On-Premise Interceptors

  * It is now possible to add custom code into the On-Premise Connector that is able to intercept and modifiy requests and responses.

* Modify content streams

  * Interceptors now can read and modify the content streams of requests and responses.

* General improvements

  * The RelayServer now warns when the `SharedSecret` setting is missing and uses a random value to be able to work at all, if it is not configured for Multi-Server operation.
  * The EF model was extended with more accurate information and new indices.
  * If an On-Premises target sets an invalid value for the expires header, there won't be an error anymopre.
  * Logging of sensitive data is now configurable and enabled by default.
  * Now all information required for manual acknowledgment is provided for easier handling.
  * Request logging now also tracks the request id.
  * It is now possible to register the On-Premise Connector types with Autofac.
  * The On-Premise Connector service now initializes http connection properties.
  * Relative paths in configuration are now consistently evaluated against the exe directory of the RelayServer and not against the execution dir anymore.
  * The new default for our own responses now is JSON and not XML.
  * An own implementation (or inherited from the default one) of an IOnPremiseConnectionContext can be registered in the DI.
  * For an easier migration scenario of changed IOnPremiseTargetRequest implementations a property called "Properties" was introduced, which will be serialized onto the root JSON object to be deserialized as properties on the on-premise side.
  * A PING will be removed from the RabbitMQ queue directly by the server because of using AcknowledgeMode.Auto.
  * The Entity Framework Context (RelayContext) is now public.
  * It is possible to use a custom implementation of an `IOnPremiseConnectionOnReceivedHandler` by registering that in an Autofac module within a custom code assembly to handle incoming data on a SignalR connection.

* Bugfixes

  * Under certain circumstances the On-Premise Connector demo service wasn't able to load a framework assembly.
  * The On-Premise Connector is now able to recreate the `HttpClient` that is used to send responses to the RelayServer in case there are errors when posting.
  * HttpConfig needs to be explicitely initialized under certain circumstances.
  * A newly created HttpClient now also receives the authentication header values of its predecessor.
  * The request interceptor now also has access to the request body.
  * Accessing empty bodies on intercepted requests and responses does not throw a NullReferenceException anymore.
  * The acknowledgement was not done on the correct RabbitMQ model.
  * An already deactivated connection was repeatingly deactivated again.
  * The config message was accidentally sent to On-Premise Connectors not supporting it.
  * Malformed query strings prevented the relaying of the request.

## Version 2.2.0

* RelayServer Windows Docker Container support

  * It is now possible to override all RelayServer configuration values, including the `RelayContext` connection string, via environment variables. The name of the environment variable needs to be the name of the setting as in the application config file, prefixed with `RelayServer__`.

* Customizing

  * It is now possible to replace the `IRequestLogger` in the RelayServer with a custom implementation.
  * It is now possible to replace the `ITraceFileWriter` and the `ITraceFileReader` in the RelayServer with a custom implementation.

## Version 2.1.2

* Bugfixes

  * The automatic disconnect feature killed active connections before the `LinkSlidingConnectionLifetime` had elapsed.

## Version 2.1.1

* Bugfixes

  * The automatic disconnect feature was not correctly made available for custom implementations of the On-Premise Connector service.

## Version 2.1.0

* Server-side Link configuration

  * It is now possible to configure link-specific settings on the RelayServer itself.

* Automatic disconnect of On-Premises Connectors

  * If required, it is possible to have an On-Premises Connector auto-disconnect itself after a maximum absolute connection time and/or after a maximum idle time.

* General improvements

  * The On-Premises Connectors settings for reconnect timeouts (maximum and minimum) are now configurable, to be able to prevent accidental DDoS detections i.e. when the server restarts and all Connectors want to reconnect in the same 30 second window.
  * Interceptors now can read the local uri that the client requested, i.e. to set forwarded headers.
  * It is now possible to configure whether the On-Premises Connector automatically follows an http redirect response from a On-Premises target, or if the redirect will be relayed too.
  * It is now possible to use a custom implementation of an `IPasswordComplexityValidator` by registering that in an Autofac module within a custom code assembly.

* Bugfixes

  * When the query to be relayed contained an query argument named 'path', this lead to unexpected behaviour
  * Corrected the filtering of contents of error responses from on-premise side services when enabled
  * A better error message will be displayed if the config file is missing for the RelayServer

## Version 2.0.0

* Multi-Server operation

  * It is now possible to operate multiple RelayServers in parallel for better load distribution. All servers need to have access to a shared network folder in order to exchange binary files with request or response payloads.

* Improved connection stability with new On-Premises Connectors

  * The RelayServer is now capable of heart beating On-Premises Connectors of version 2.x or newer. A Connector that does not receive this heartbeat will automatically try to reconnect to the server.

* Implementation of custom code

  * It is now possible to extend the RelayServer with custom WebAPI controllers as well as as plugins to react upon or even modify incoming requests or outgoing responses. For details see [RelayServer Plugin Development](5-relayserver-interceptor-development.md).  

    We offer the following extension points:

    * Requests

      * Read or modify the HTTP verb
      * Read or modify the requested url
      * Read or modify the HTTP headers
      * Immidiate rejection of answering of the request
      * Modifying the TTL of the request in the message queue
      * Modification of the acknowledge mode to automatic or manual acknowledgement of the request in the message queue

    * Responses

      * Read or modify the HTTP status code
      * Read or modify the response HTTP headers
      * Complete replacement of the received response

* Improved logging

  * We switched the logging from NLog to Serilog and enriched the log entries with structured information. You can configure custom sinks and enrichers as you see the need. For configuration details see [Serilog AppSettings Konfiguration](https://github.com/serilog/serilog/wiki/AppSettings).

* Optimizations

  * Memory consumption of the RelayServer and On-Premises Connectors has been reduced. Additionally we optimized general performance to make the system more efficient.

* Security improvements

  * It is now possible to deactivate features of the RelayServer (relaying, on-premises connections, management web) specifically. Also, when enabled, it is possible to allow access globally or only from localhost.
  * All dashboard and info endpoints now require authorization
  * Error messages do not contain stack traces anymore
  * Changing a users password now requires the current password
  * Rules for better passwords have been introduced
  * Too many failed attempts to login will now temporary lock a users account
  * Strict Transport Security headers will be set
  * X-Frame-Options headers will be set
  * X-XSS-Protection headers will be set
  * It ist now possible to restrict the relay endpoint to authenticated requests only

* General improvements

  * Info and management endpoints now set correct cache headers
  * Dependencies have been updated to their corresponding latest versions

* Bugfixes

  * Some operations did not work reliable in the Management Web and have been fixed

## Version 1.0.4

Initial release.
