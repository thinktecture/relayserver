# RelayServer Interceptor Development

The RelayServer can be extended with interceptors.

Two types of expansion points are currently available:

1. The RequestInterceptor can reject, manipulate, or answered requests immediately before being forwarded to the appropriate OnPremise Connector.
1. The ResponseInterceptor can manipulate or replace a response received from the OnPremise Connector before passing it from the RelayServer to the client.

## Create interceptor assembly

To provide interceptors for the RelayServer, it is sufficient to create an assembly that contains at least one class that implements at least one of the provided interceptor interfaces.

A .NET 4 library project is created that has a reference to the `Thinktecture.Relay` assembly. The interceptor interfaces are available in this assembly.

## Implement the interceptors

The possible expansion points are listed and explained below. It is sufficient to implement at least one of the listed interfaces.

_Important:_ It is _not_ allowed to provide an interface implementation in more than one class, unless the class itself is registered via its own module in the DI container (see below).

In this case, however, it is _not_ allowed to provide more than one DI module.

### Loading the interceptors

The RelayServer first looks for a DI module (see below). If exactly one DI module is found, this is registered. This allows you to register your own dependencies in the Dependency Injection Container and to use them in your own interceptors.

If no DI module is found, the RelayServer will attempt to automatically determine the classes that implement at least one of the interceptor interfaces from the interceptor assembly. If exactly one class per interface is found, this class is automatically registered in the DI.

In this case, an interceptor class can only use the dependencies that are available as a standard, such as the `Serilog.Logger`.

### Modify the request

In order to modify an incoming request, or to answer it directly before the relay process, a class can be provided which implements the interface `IOnPremiseRequestInterceptor`.

The interface specifies the `HttpResponseMessage OnRequestReceived(IInterceptedRequest request)` method.

- In order not to change the request, it is sufficient to return `null`.
- If a `HttpResponseMessage` is returned, this reponse is *immediately* sent to the client. If the property `AlwaysSendToOnPremiseConnector` is not explicitely set to `true`, the forwarding of the request to the on premise connector is skipped.
- To modify the request, the corresponding values ​​can be changed directly on the transferred `IInterceptedRequest`.

The following values ​​can be changed:
- `HttpMethod`: The HTTP method (so-called verb) can be changed here.
- `Url`: The URL of the request can be modified.
- `HttpHeaders`: HTTP headers can be removed, new added or existing changed.

If no `HttpResonseMessage` is returned, the modified request is forwarded to the actual destination via an OnPremiseConnector.

### Modify the response

A response sent back from the On-Premise target through an OnPremise Connector can be modified before it is sent back to the client. To do this, create a class that implements the interface `IOnPremiseResponseInterceptor`.

This interface specifies two methods to implement:

* `HttpResponseMessage OnResponseReceived(IReadOnlyInterceptedRequest request)`: Invoked when the On-Premise service has received *no* response. In this case, an answer can be generated here.
* `HttpResponseMessage OnResponseReceived(IReadOnlyInterceptedRequest request, IInterceptedResponse response)`: Invoked when a response was received. This can be use to modify the response or replace it by a separate answer.

If there is no response from the OnPremise Connector, the first version is called.

- If `null` is returned, the default behavior of the relay server is used.
- If an `HttpResonseMessage` is returned, this reponse is *immediately* sent to the client.

If a response was received via the OnPremise Connector, the second overload is called.

- If `null` is returned, the` IInterceptedResponse` is forwarded to the client.
- If an `HttpResonseMessage` is returned, this reponse is *immediately* sent to the client and the` IInterceptedResponse` is discarded.
- To modify the response, the corresponding values ​​can be changed directly on the transferred `IInterceptedResponse`.

The following values ​​can be changed:
- `StatusCode`: Here the status code of the response can be changed.
- `HttpHeaders`: HTTP headers can be removed, new added or existing changed.


### Optional: Registstration via an AutofacModule

To register the interceptors and your own dependencies to the DI of the RelayServer, it is possible to provide a custom Autofac module. This will allow for more control over your management of the required dependencies.

```
using Autofac;
using Thinktecture.Relay.Server.Interceptors;

/// <summary>
/// A relay server interceptor assembly can provide a single AutoFac
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

## Configure the intecreptor

In the `App.config` of the RelayServer, it is sufficient to assign the configuration value `Interceptor Assembly` to a path that points to the interceptor assembly. The path can be either absolute or relative.
