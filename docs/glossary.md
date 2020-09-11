# Glossary

When you are new to **RelayServer** or coming from an older version, there can be a lot of words to learn. This glossary aims to give you a
25.000-feet overview of common terms and what they mean in the context of RelayServer.

[B](#b) | [C](#c) | [I](#i) | [R](#r) | [T](#t)

## B

### Body Store

The body store is a storage where the body contents of requests and responses are stored while a request is being processed. By default a
file-based storage is used. An in-memory store is available, too.

## C

### Client

A _Client_ is an external application or a service, which is sending a [Request](#request) to a [Target](#target) which is made accessible
through the [RelayServer](#relayserver).

### Connector

The RelayServer _Connector_ is a piece of software that runs on a physical location where you want to access local services (aka
[Targets](#target)). The network the _Connector_ is located in is usually not accessible from the internet. The _Connector_ creates a
connection to the [RelayServer](#relayserver), through which the server can send a [Request](#request) to the connector. The connector then
requests the internal [Target](#target) and relays its [Response](#response) back to the server, which then relays it back to the requesting
[Client](#client).

For reasons of availability and load balacing the _Connector_ can be run multiple times at the same location / network. All _Connectors_ on
a specific physical location are logically referred to as a [Tenant](#tenant).

The _Connector_ was formerly called _OnPremisesConnector_ (short _OPC_) in RelayServer 2.

## R

### RelayServer

The _RelayServer_ is a service that usually is publicly available on the internet. Its main purpose is to receive [Requests](#request) from
[Clients](#client), and pass them to a [Connector](#connector) that belongs to the correct [Tenant](#tenant). It then waits for the
[Response](#response) to be sent back, and passes it back to the [Client](#client). This process is referred to as _Relaying_.

### Request

The _Request_ represents an external Http(s) request from a [Client](#client). It can be intercepted and modified while being processed by
the [RelayServer](#relayserver). It will be passed on to a [Target](#target) via the [Connector](#connector).

### Response

A _Response_ always corresponds to a [Request](#request). When the [Request](#request) was executed by the [Target](#target), the
[Connector](#connector) will read the _Response_ and send it back to the [RelayServer](#relayserver).

## I

### Interceptor

An _Interceptor_ is a piece of code that you can provide via dependency injection and that is able to intercept

- a [Requests](#request) after it was received by the [RelayServer](#relayserver) and before it is passed along to the
  [Connector](#connector) or
- a [Response](#response) after it was received from the [Connector](#connector) and before it is passed back to the [Clients](#client).

_Interceptors_ are a flexible way of extending the [RelayServer](#relayserver) functionality and can be used to modify the corresponding
[Requests](#request) or [Responses](#response) by changing url, method, http headers, the body (payload).

## T

### Target

A _Target_ describes a service that is usually not directly exposed to the internet. Instead it is accessible via a [Request](#request) sent
to the [RelayServer](#relayserver). This [Request](#request) is then relayed through a [Connector](#connector) into the [Tenants](#tenant)
network and then executed. The [Response](#response) of the _Target_ is then sent back to the [RelayServer](#relayserver) which will then
relay it back to the [Client](#client).

### Tenant

The _Tenant_ describes a physical location (on-premises) where one or more [Connectors](#connector) are installed and ready to relay
requests to local [Targets](#target) that are provided by the _Tenant_.

The _Tenant_ was formerly called _Link_ in RelayServer 2.
