# Getting started with RelayServer development

The RelayServer development environment can be run locally for debugging of individual components or in containers for
general testing.

_Note:_ The containers in this repository are intended for **demo and example** purposes only. It is strongly
encouraged to create your own host applications tailored and configured to your specific needs and to build and use
your own container images.

A docker compose file is provided to build all the docker images (`src/docker/docker-compose.yml`). You can build the
images with the `docker compose build` command.

## First time Development & Test-Setup

__Prerequisites__:  
In order to build and run the development environment, you need the following components on your system:

- [Docker](https://www.docker.com/products/docker-desktop/)
- [.NET SDK 6.0.100](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
  or newer features within .NET 6.0

The _development_ environment also comes with a [Seq](https://datalust.co/seq) logging server in a local docker
container (using the local, free single-user license). For production, the RelayServer components log to stdout and
stderr as this is default in docker environments, but you can also customize your host applications and your images
to use other logging targets or acquire a commercial Seq license.

## Components

The relay server development environment consists of several parts.

- Configuration database  
  The current implementation supports PostgreSQL and Microsoft SQL Server. The development environment is built for
  PostgreSQL as default. By setting an environment variable `RELAYSERVER_DATABASE_TYPE` to `SqlServer` you can switch to
  Microsoft SQL Server.

   - Only needs to be accessible from specifically listed components.
   - The PostgreSQL database server can be accessed through its default port (5432) on localhost.
   - The Microsoft SQL Server database server can be accessed through its default port (1433) on localhost.

- Message queue  
  Currently only RabbitMQ is supported. The development environment launches 2 Rabbit nodes in a cluster configuration.

   - Only needs to be accessible from specifically listed components.
   - Node 1 management UI can be accessed at http://localhost:15672/, login with guest/guest.
   - Node 2 management UI can be accessed at http://localhost:15673/, login with guest/guest.

- OIDC compliant identity provider  
  The identity provider provides an authentication service for the other components. In the development environment we
  use a preconfigured Keycloak instance.

   - Needs to be accessible from outside the system.
   - The development OIDC server can be accessed at http://localhost:5002/realms/relayserver as the authority.

- ManagementApi  
  The Management API is used to manage the data in the configuration database.

   - Needs access to the configuration database.
   - The example service uses API keys for authentication.
   - You should not expose this to the public.
   - Management API swagger UI can be accessed at http://localhost:5004/ to see the api docs.

- RelayServer  
  The main server component. Can be started as single node or multi-server.

   - Needs to be accessible from outside the system.
   - Needs access to the configuration database.
   - Needs access to the message queue.
   - Needs access to the identity provider.
   - If this component is started more than once, all instances need to share one storage volume (shared file storage).
   - RelayServer can be accessed at http://localhost:5000/ in single-node mode.
   - RelayServer node A can be accessed at http://localhost:5010/ in multi-server mode.
   - RelayServer node B can be accessed at http://localhost:5011/ in multi-server mode.

- Connector  
  This component will be installed on-premises at tenants.

   - Needs access to identity provider through public url.
   - Needs access to relay server through public url.

- Seq  
  The logging server for local development.

   - Do not make this accessible, as this is single user mode and everyone is admin without authentication.
   - The Seq server can be accessed at http://localhost:5341/ to analyse the logs.

## Plugability

All parts of RelayServer, the APIs and the Connector are normal ASP.NET Core middlewares, controllers, SignalR Hubs and
backing service classes. The way we structured the components listed above is based on the RelayServer v2 usages we saw
so far and does not mean that this is the only way to deploy and use RelayServer.

It might be the case that your specific use case demands or favours a different component layout, i.e. combining the
management API and RelayServer together into a single host project, or even putting all server components together.
