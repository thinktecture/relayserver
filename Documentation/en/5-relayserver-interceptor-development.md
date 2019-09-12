# RelayServer Interceptor Development

The RelayServer and the On-Premise connectors can be extended with interceptors.

## RelayServer

Two types of expansion points are currently available:

1. The RequestInterceptor can reject, manipulate, or answered requests immediately before being forwarded to the appropriate OnPremise Connector.
1. The ResponseInterceptor can manipulate or replace a response received from the OnPremise Connector before passing it from the RelayServer to the client.

### Create interceptor assembly

To provide interceptors for the RelayServer, it is sufficient to create an assembly that contains at least one class that implements at least one of the provided interceptor interfaces.

A .NET 4 library project is created that has a reference to the `Thinktecture.Relay` assembly. The interceptor interfaces are available in this assembly.

### Implement the interceptors

The possible expansion points are listed and explained below. It is sufficient to implement at least one of the listed interfaces.

_Important:_ It is _not_ allowed to provide an interface implementation in more than one class, unless the class itself is registered via its own module in the DI container (see below).

In this case, however, it is _not_ allowed to provide more than one DI module.

#### Loading the interceptors

The RelayServer first looks for a DI module (see below). If exactly one DI module is found, this is registered. This allows you to register your own dependencies in the Dependency Injection Container and to use them in your own interceptors.

If no DI module is found, the RelayServer will attempt to automatically determine the classes that implement at least one of the interceptor interfaces from the interceptor assembly. If exactly one class per interface is found, this class is automatically registered in the DI.

In this case, an interceptor class can only use the dependencies that are available as a standard, such as the `Serilog.Logger`.

#### Modify the request

In order to modify an incoming request, or to answer it directly before the relay process, a class can be provided which implements the interface `IOnPremiseRequestInterceptor`.

The interface specifies the `HttpResponseMessage OnRequestReceived(IInterceptedRequest request)` method.

- In order not to change the request, it is sufficient to return `null`.
- If a `HttpResponseMessage` is returned, this reponse is *immediately* sent to the client. If the property `AlwaysSendToOnPremiseConnector` is not explicitely set to `true`, the forwarding of the request to the on-premise connector is skipped.
- To modify the request, the corresponding values ​​can be changed directly on the transferred `IInterceptedRequest`.

The following values ​​can be changed:
- `HttpMethod`: The HTTP method (so-called verb) can be changed here.
- `Url`: The URL of the request can be modified.
- `HttpHeaders`: HTTP headers can be removed, new added or existing changed.
- `Content`: The content stream of the request can be read and changed here.  
*Caution:* When this property is accessed, a copy of the content will be created in memory, as the original request stream can only be read once. This will increase overall memory usage of the RelayServer.
- `AlwaysSendToOnPremiseConnector`: Setting this to true will cause the request to be relayed to the OnPremiseConnector even if the interceptor immediately answers it by returning an `HttpResponseMessage`.
- `Expiration`: The TTL of this request in the RabbitMQ can be changed here.
- `AcknowledgmentMode`: This determines whether the OnPremiseConnector will acknowledge a request (Default), the RelayServer will auto-acknowledge the request (Auto), of if custom code on the target needs to manually acknowledge the request (Manual). For details see section Acknowledgment below.

If no `HttpResonseMessage` is returned, the modified request is forwarded to the actual destination via an OnPremiseConnector.

#### Modify the response

A response sent back from the On-Premise target through an OnPremise Connector can be modified before it is sent back to the client. To do this, create a class that implements the interface `IOnPremiseResponseInterceptor`.

This interface specifies two methods to implement:

* `HttpResponseMessage OnResponseFailed(IReadOnlyInterceptedRequest request)`: Invoked when the On-Premise service has received *no* response. In this case, an answer can be generated here.
* `HttpResponseMessage OnResponseReceived(IReadOnlyInterceptedRequest request, IInterceptedResponse response)`: Invoked when a response was received. This can be use to modify the response or replace it with a completely new response message.

If there is no response from the OnPremise Connector, the first method is called.

- If `null` is returned, the default behavior of the RelayServer is used.
- If an `HttpResonseMessage` is returned, this reponse is *immediately* sent to the client.

If a response was received via the OnPremise Connector, the second method is called.

- If `null` is returned, the (probably modified) `IInterceptedResponse` is forwarded to the client.
- If an `HttpResonseMessage` is returned, this reponse is *immediately* sent to the client and the `IInterceptedResponse` is discarded.
- To modify the response, the corresponding values ​​can be changed directly on the provided `IInterceptedResponse`.

The following values ​​can be changed:
- `StatusCode`: Here the status code of the response can be changed.
- `HttpHeaders`: HTTP headers can be removed, new ones added or existing changed.
- `Content`: The content stream of the response can be read and changed here.  
*Caution:* When this property is accessed, a copy of the content will be created in memory, as the original response stream can only be read once. This will increase overall memory usage of the RelayServer.

#### Optional: Registration via an AutofacModule

To register the interceptors and your own dependencies to the DI of the RelayServer, it is possible to provide a custom Autofac module. This will allow for more control over your management of the required dependencies.

```
using Autofac;
using Thinktecture.Relay.Server.Interceptors;

/// <summary>
/// A RelayServer interceptor assembly can provide a single AutoFac
/// Module that will register all interceptor types that are implemented
/// and should be used.
/// </summary>
public class PluginModule : Module
{
	/// <summary>
	/// Override the Load method of the AutoFac module to
	/// register the interceptor types.
	/// </summary>
	/// <param name="builder"></param>
	protected override void Load(ContainerBuilder builder)
	{
		// Each interceptor that should be added needs to be registered
		// with the container builder as its Interface type

		builder.RegisterType<RequestInterceptorDemoPlugin>()
			.As<IOnPremiseRequestInterceptor>();
		builder.RegisterType<ResponseInterceptorDemoPlugin>()
			.As<IOnPremiseResponseInterceptor>();

		base.Load(builder);
	}
}
```

### Configure the interceptor

In the `App.config` of the RelayServer, it is sufficient to assign the configuration value `Interceptor Assembly` to a path that points to the interceptor assembly. The path can be either absolute or relative.

## Acknowledgment

The RelayServer concept wants the On-Premise Connector to acknowledge a request that it received, before the RelayServer will mark the corresponding message in the message queue as completed. In the default setting, the On-Premise Connector sends the acknowledge message to the Relay Server when it receives a request. After that, the request will be forwarded to the On-Premise target.

A request interceptor can modify the `AcknowledgmentMode` of a request and set it to `Auto` or `Manual`.  
`Auto` will make the RelayServer remove the message immideately from the queue after reading it. It will not be guaranteed to reach the On-Premise Connector in this case.  
`Manual` will require you to manually acknowledge the request. This could be done in the On-Premise target to confirm that the request actually reached its destination.

To manually acknowledge a request, you need to send a HTTP GET request to the `/request/acknowledge` endpoint on the RelayServer and pass the query string parameters `aid` with the AcknowledgeId, `oid` with the OriginId and optionally `cid` with the ConnectionId. The On-Premise connector will provide these arguments in the `X-TTRELAY-ACKNOWLEDGE-ORIGIN-ID`, `X-TTRELAY-ACKNOWLEDGE-ID` and `X-TTRELAY-CONNECTION-ID` http headers with the request. This request also needs to provide a valid bearer token in the authorization header.  
An In-Process target can call the method `AcknowledgeRequestAsync` on the `RelayServerConnector` instead, which will then send the authenticated acknowledge request to the RelayServer.

## On-Premise Connector

Also here two types of expansion points are currently available:

1. The RequestInterceptor can manipulate requests immediately before being forwarded to the appropriate OnPremise target.
1. The ResponseInterceptor can manipulate a response received from the OnPremise Target before passing it to the RelayServer.

### Implementation

The interfaces to implement are available in the assembly `Thinktecture.Relay.OnPremiseConnector`.

In the namespace `Thinktecture.Relay.OnPremiseConnector.Interceptor` you'll find two interfaces you can implement:
* `IOnPremiseRequestInterceptor`
* `IOnPremiseResponseInterceptor`

Interceptors will be instanciated for every request and every response by the IoC container, and can receive any other registered dependencies.

#### Notes

* If you want to modify the content stream of a request or response, make sure to check the available features of the stream (`CanSeek`, `CanRead`, `CanWrite`). If in doubt, it's better to copy the contents and assign a new, readable stream to the request or response to ensure that it can be read again for passing it along.
* If you change the size of a request or response content stream, make sure to also adjust the `Content-Length` http header if its there.

### Register the interceptors

To register On-Premise interceptors, you register them with the IoC container (`Microsoft.Extensions.DependencyInjection`) and pass the resulting `IServiceProvider` to the constructor of the `RelayServerConnector`. A sample is given in the `Thinktecture.Relay.OnPremiseConnector.InterceptorSample` project.

It is not possible, to extend the example-`OnPremiseConnectorService` with interceptors.
