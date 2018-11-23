# [What is Thinktecture RelayServer?](1-what-is-thinktecture-relayserver.md)
# [RelayServer Architecture](2-architecture.md)
# [RelayServer Installation](3-installation.md)
# [RelayServer Web Management](4-relayserver-web-management.md)
# [RelayServer Plugin Development](5-relayserver-interceptor-development.md)
# [Development Setup](6-development-setup.md)

# Sponsors
The goal of this list is to highlight companies who pay back to this open source project which is helping them to save time and money in their own projects.

## Sponsors from Germany
[<img width="120px" src="./assets/logo_sponsor_kwp.svg" />](https://www.kwpsoftware.de)

## Sponsors from Swiss
[<img width="120px" src="./assets/logo_sponsor_cmi.svg" />](https://www.cmiag.ch/)

# Version history

## Version 2.1.0

* Server-side Link configuration

  * It is now possible to configure link-specific settings on the RelayServer itself.

* Automatic disconnect of On-Premises connectors

  * If required, it is possible to have an On-Premises connector auto-disconnect itself after a maximum absolute connection time and/or after a maximum idle time.

* General improvements

  * The On-Premises connectors settings for reconnect timeouts (maximum and minimum) are now configurable, to be able to prevent accidental DDoS detections i.e. when the server restarts and all connectors want to reconnect in the same 30 second window.


## Version 2.0.1

- Bugfixes

  * When the query to be relayed contained an query argument named 'path', this lead to unexpected behaviour
  * Corrected the filtering of contents of error responses from on-premise side services when enabled

## Version 2.0.0

* Multi-Server operation

  * It is now possible to operate multiple RelayServers in parallel for better load distribution. All servers need to have access to a shared network folder in order to exchange binary files with request or response payloads.

* Improved connection stability with new On-Premises connectors

  * The RelayServer is now capable of heart beating On-Premises connectors of version 2.x or newer. A connector that does not receive this heartbeat will automatically try to reconnect to the server.

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

- Improved logging

  * We switched the logging from NLog to Serilog and enriched the log entries with structured information. You can configure custom sinks and enrichers as you see the need. For configuration details see [Serilog AppSettings Konfiguration](https://github.com/serilog/serilog/wiki/AppSettings).

- Optimizations

  * Memory consumption of the RelayServer and On-Premises connectors has been reduced. Additionally we optimized general performance to make the system more efficient.

- Security improvements

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

- General improvements

  * Info and management endpoints now set correct cache headers
  * Dependencies have been updated to their corresponding latest versions

- Bugfixes

  * Some operations did not work reliable in the Management Web and have been fixed

## Version 1.0.4

Initial release.
