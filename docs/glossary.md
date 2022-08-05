# Glossary

When you are new to **RelayServer** or coming from an older version, there can be a lot of words to learn. This glossary
aims to give you a 25.000-feet overview of common terms and what they mean in the context of RelayServer.

[A](#a) | [B](#b) | [C](#c) | [I](#i) | [M](#m) | [R](#r) | [S](#s) | [T](#t)

## A

### Authentication

RelayServer v3 supports different types of authentication for different use cases. [Connectors](#connector),
by default, use OAuth 2 client credential authentication to be able to securely communicate with the
[RelayServer](#relayserver). However you can customize and change that.

When you want to use the [Management API](#management-api) or the [Statistics API](#statistics-api), we suggest
that you use your own existing OIDC-based Identity Provider and configure your clients and the APIs to use this.

## B

### Body Store

The _Body Store_ is a storage where the body contents of [Requests](#request) and [Responses](#response) are
stored while a request is being processed. By default a file-based storage is used. An in-memory store is available,
too. Bodies are only written to the _Body Store_ when they are too large to be sent inline through the configured
[Transport](#transport).

## C

### Client

A _Client_ is an external application or a service, which is sending a [Request](#request) to a [Target](#target), that
is made accessible through the [RelayServer](#relayserver).

### Connector

The _Connector_ is a piece of software that runs on a physical location where you want to access local services (aka
[Targets](#target)). The network where the _Connector_ is located in, is usually not publicly accessible from the
internet. The _Connector_ creates a connection to the [RelayServer](#relayserver), through which the server can send
a [Request](#request) to the connector. The connector then requests the internal [Target](#target) and relays
its [Response](#response) back to the server, which then relays it back to the requesting [Client](#client).

For reasons of higher availability and load balancing, the _Connector_ can be run multiple times at the same
location / network. All _Connectors_ on a specific physical location are logically referred to as a [Tenant](#tenant).

The _Connector_ was formerly called _OnPremisesConnector_ (short _OPC_) in RelayServer v2.

## I

### Interceptor

An _Interceptor_ is a piece of code that you can provide via dependency injection and that is able to intercept

- a [Request](#request), after it was received by the [RelayServer](#relayserver) and before it is passed along to
  the [Connector](#connector) or
- a [Response](#response), after it was received from the [Connector](#connector) and before it is passed back to
  the [Client](#client).

_Interceptors_ are a flexible way of extending the [RelayServer](#relayserver) functionality and can be used to modify
the corresponding [Requests](#request) or [Responses](#response) by changing the url, method, http headers or the body (
payload).

_Note:_ In _RelayServer v2_ there was also the concept of _Interceptors_ that were executed within the
_OnPremisesConnector_ (_OPC_), before and after a request was passed to the [Target](#target). In _RelayServer v3_ this
was changed and you can now implement a custom [RelayTarget](#relaytarget) class to provide the same functionality.

## M

### Management API

The _Management API_ is a separate service that you can host publicly alongside the [RelayServer](#relayserver) or
only on your private network. The _Management API_ allows the management (creation, configuration, removal) of
[Tenants](#tenant).

In RelayServer v2, similar endpoints were an integral part of the RelayServer host process.

## R

### RelayServer

The _RelayServer_ is a service that usually is publicly available on the internet. Its main purpose is to
receive [Requests](#request) from [Clients](#client), and pass them to a [Connector](#connector) that belongs to the
corresponding [Tenant](#tenant). It then waits for the [Response](#response) to be sent back, and passes it back to
the [Client](#client). This process is referred to as _Relaying_.

### RelayTarget

A _RelayTarget_ is a class that implements handling of a [Requests](#request) within a [Connector](#connector). The
default `RelayWebTarget` implementation simply executes the Http(s) [Requests](#request) against the [Target](#target)
and returns the [Response](#response). In order to provide the same functionaly as you could with RelayServer v2
Connector-Side _Interceptors_, you now can implement your own _RelayTarget_ and add your custom logic here.

### Request

The _Request_ represents an external Http(s) request from a [Client](#client). It can be intercepted and modified while
being processed by the [RelayServer](#relayserver). It will be passed on to a [Target](#target) via
the [Connector](#connector).

### Response

A _Response_ always corresponds to a [Request](#request). When the [Request](#request) was executed by
the [Target](#target), the [Connector](#connector) will receive the _Response_ and send it back to
the [RelayServer](#relayserver) to be passed back to the [Client](#client).

## S

### Statistics API

The _Statistics API_ is a service that you can host publicly alongside the [RelayServer](#relayserver) or only on
your private network. The _Statistics API_ will provide you with statistical data about the whole system, i.e.
how many [Requests](#request) have been handled in what time frame, for what [Tenant](#tenant) these were handled,
how many bytes were transferred, how many [Connectors](#connector) are connected and a lot of more data.

In RelayServer v2, similar endpoints were an integral part of the RelayServer host process.

## T

### Target

A _Target_ describes a service that is usually not directly exposed to the internet. Instead it is accessible via
a [Request](#request) sent to the [RelayServer](#relayserver). This [Request](#request) is then relayed through
a [Connector](#connector) into the [Tenants](#tenant) network and then executed. The [Response](#response) of the _
Target_ is then sent back to the [RelayServer](#relayserver), which will then relay it back to the [Client](#client).

### Tenant

The _Tenant_ describes a physical location (on-premises) where one or more [Connectors](#connector) are installed and
ready to relay requests to local [Targets](#target) that are provided by the _Tenant_.

The _Tenant_ was formerly called _Link_ in RelayServer v2.

### Transport

Different communication channels between [Connectors](#connector) and the [RelayServer](#relayserver) as well as
between multiple [RelayServer](#relayserver) instances are called _Transport_. By default,
[RelayServer](#relayserver) uses three different _Transports_: RabbitMQ for communication between
[RelayServer](#relayserver) instances, SignalR for communication from a [RelayServer](#relayserver) to a
[Connector](#connector) and HTTPS for communication from a [Connector](#connector) back to the
[RelayServer](#relayserver).
