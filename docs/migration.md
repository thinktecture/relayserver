# Migration

This section explains how a migration path from RelayServer v2 to v3 might look like.

## Update to latest v2

As a first step, you should update to the very latest version of RelayServer v2. This is important for the server but
also for your connectors. You don't need to roll out all connectors on the latest version now, but you need to make sure
your connector's code will work with the latest v2 connector component.

If you have modified the server or connector sources, make sure you revert these changes and only use the intended
extension points (i.e. interceptors).

## Plan your v3 deployment scenario

This is probably the most important step. RelayServer v3 is much more modular than v2. While it still needs a RabbitMQ
installation to work, you now can choose to use either Microsoft SQL Server or PostgreSQL as a database or even provide
your own database connector for other database types.

In order to be able to deploy and run a reliable RelayServer v3 installation you need to plan out which components you
want to deploy in what configuration. While you can host the management api in the same process as the actual
RelayServer, you might want to distribute them into several different hosting processes.

Be aware that if you only use a single RabbitMQ host and this fails (i.e. because of a hardware issue), your whole
RelayServer will not be able to process any more requests.

If you need a reliable setup, we strongly suggest to operate your RabbitMQ and the database in a fault tolerant cluster
configuration on different physical machines. You also should operate at least two instances of the RelayServer main
server component and the token security service in order to provide fault tolerance and some sort of load balancing.
Depending on your use-case scenario it might be suitable to merge these two components into a single host project.

In regards to the management api, it is intended to be an internal tool for you to help you to manage your installation.
Depending on your needs you might get away with a single deployment of this service. If you want you can also only spin
up an instance of the management api when you have to change your settings and shut it down otherwise to save resources.
Alternatively, you can also use the management assemblies to directly gather information or manage your tenants from
your own internal tools and don't use the api at all.

If your are unsure if your deployment scenario is well suited for your specific use-case, you can of course contact
Thinktecture AG. We do offer review and consulting services so that you can be sure that your deployment and operation
scenario is safe and sound.

## Migrate all Connectors to intermediate "v2.9"

We will provide an intermediate connector "v2.9" that is able to work with the old RelayServer v2 but is also capable of
connecting to the new RelayServer v3. Since the SignalR transport had to be incompatible between v2 where we use SignalR
2 and v3 where we use ASP.NET Core SignalR, we also did introduce some changes in the wire protocol. This means that
your intermediate connector will need to provide both implementations for the old and the new version in a single
project. We suggest using different assemblies for your new code, so that you can easily get rid of the old v2 parts
after the migration is completed.

The v3 connector does not provide connector-side interceptor support in the way v2 did. Instead, you can chose to
implement an `IRelayTarget` or extend the `RelayWebTarget` type. We suggest to try extending the `RelayWebTarget` class
and overriding the `HandleAsync` method first.

The logic that was done on the request interceptor should be done before calling the base implementation of
`HandleAsync`, and the logic that was done in the response interceptor can work on the result of the base
implementation.

In most cases you should be able to re-use most of your existing logic and only have to adjust your code where it
interacts with the actual request and response objects, as they have a different interface in v3.

You then should roll out all connectors to the "v2.9" intermediate connector state.

## Migrate to RelayServer v3

Following your deployment plan, create the host projects for your RelayServer v3 and management API. You can use our
docker examples as a starting point. We suggest using only our NuGet packages to build your RelayServer system. Building
from source might lead to possible unwanted modifications and makes troubleshooting extremely hard.

Be aware that we do not provide a ready-to run access control solution for the management API. If you have an existing
OIDC Identity Provider we suggest using this and to configure your API host projects to use this.

Migrate your server-side interceptor to the new interfaces of v3. In most cases you should be able to re-use most of
your existing logic and only have to adjust your code where it interacts with the actual request and response objects,
as they have a different interface in v3.

Move the database entries for your connectors from the v2 database to a new v3 database. A migration tool is currently
in planning to help you with that.
