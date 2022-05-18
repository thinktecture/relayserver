# RelayServer Deployment

RelayServer v3 is designed for containerized deployments using Docker.

Scripts to help building and running the system are provided
as [PowerShell Core](https://github.com/powershell/powershell) scripts, to be able to run cross platform on Windows,
macOS and Linux.

First, a script is provided to build all the docker images (`src/build-docker-images.ps1`).

All other scripts are located in the folder `src/docker`:

* The external components (database, message queue, details see below) can be started with the `run-dependencies.ps1`
  script.
* Then the environment with one relay server (`run-environment.ps1`)
* or a multi-server environment (`run-environment-multiserver.ps1`) can be started.

## First time Development & Test-Setup

__Prerequisites__:  
In order to build and run the development environment, you need the following components on your system:

- Docker
- .NET Core SDK 6.0.100 or newer features within .NET 6.0
- PowerShell Core 6 or newer

After you start the system for the first time, you need to execute the `seed-data.ps1` script, which will create the
first initial configuration for the tenants, so that the connectors can connect with their default development
credentials. After that you can run the environment or the multi-server environment.

The _development_ environment also comes with a [Seq](https://datalust.co/seq) logging server in a local docker
container (using the local, free single-user license). For production the RelayServer components log to stdout and
stderr as this is default in docker environments, but you can also slightly modify the images to use other logging
targets or acquire a commercial Seq license.

## Components

The relay server environment consists of several parts.

- Configuration database  
  The current implementation supports PostgreSQL as a database. SQL Server support is on the roadmap.

   - Only needs to be accessible from specifically listed components.
   - The PostgreSql database server can be accessed through its default port (5432) on localhost.

- Message queue  
  Currently only RabbitMQ is supported. The dev environment launches 2 Rabbit nodes in a cluster configuration.

   - Only needs to be accessible from specifically listed components.
   - Node 1 management UI can be accessed at http://localhost:15672 login with guest/guest.
   - Node 2 management UI can be accessed at http://localhost:15673 login with guest/guest.

- IdentityServer  
  The IdentityServer provides an authentication service for the other components.

   - Needs to be accessible from outside the system.
   - Needs access to the configuration database.
   - Currently NOT YET capable of running more than one node.
   - The identity server can be accessed at http://localhost:5002 as authority.

- ManagementApi  
  The Management API is used to manage the data in the configuration database.

   - Needs access to the configuration database.
   - Currently NOT YET secured with the identity server. DO NOT YET expose this to the public.
   - Management API swagger UI can be accessed at http://localhost:5004 to see the api docs.

- RelayServer  
  The main server component. Can be started as single node or multi-server.

   - Needs to be accessible from outside the system.
   - Needs access to the configuration database.
   - Needs access to the message queue.
   - Needs access to the identity server.
   - If this container is started more than once, all relay server instances need to share one storage volume (shared
     file storage). See the `src/docker/Thinktecture.Relay.Server.Docker/run-container-multiserver.ps1` for required
     links and mounts.
   - RelayServer can be accessed at http://localhost:5000 in single-node mode.
   - RelayServer node A can be accessed at http://localhost:5010 in multi-server mode.
   - RelayServer node B can be accessed at http://localhost:5011 in multi-server mode.

- Connector  
  This component will be installed on-premises at tenants.
   - Needs access to identity server through public url.
   - Needs access to relay server through public url.

- Seq  
  The logging server for local development.

   - Do not make this accessible, as this is single user mode and everyone is admin without authentication.
   - The Seq server can be accessed at http://localhost:5341 to analyse the logs.
