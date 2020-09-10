# RelayServer Deployment

RelayServer v3 is designed for containerized deployments using Docker.

Scripts to help building and running the system are provided as [PowerShell Core](https://github.com/powershell/powershell) scripts, to be able to run cross platform on Windows, macOS and Linux.

All scripts are located in the folder `src/docker`.

First, a script is provided to build the docker images (`build-images.ps1`) and to run a test environment with one server (`run-environment.ps1`) and in a multiserver environment (`run-environment-multiserver.ps1`).

The external components (database, message queue, details see below) can be started with the `run-dependencies.ps1` script.

**First time Development & Test-Setup**:

When you run the system locally for the first time, only start the dependencies and the management api. Then you can run the `seed-data.ps1` script, which will create the first initial configuration for the tenants, so that the connectors can connect with their default development credentials.

The *development* environment also comes with a [Seq](https://datalust.co/seq) logging server in a local docker container (using the local, free single-user license). For production the RelayServer components log to stdout and stderr as this is default in docker environments, but you can also slightly modify the images to use other logging targets or aquire a commercial Seq license.

## Components

The relay server environment consists of several parts.

* Configuration database  
  The current implementation supports PostgreSQL as a database. SQL Server support is on the roadmap.  
  * Only needs to be accessible from specifically listed components.

* Message queue  
  Currently only RabbitMQ is supported.
  * Only needs to be accessible from specifically listed components.

* IdentityServer  
  The IdentityServer provides an authentication servicer for the other components.
  * Needs to be accessible from outside the system.
  * Needs access to the configuration database.
  * Currently NOT YET capable of running more than one node.

* ManagementApi  
  The Management API is used to manage the data in the configuration database.
  * Needs access to the configuration database.
  * Currently NOT YET secured with the identity server. DO NOT YET expose this to the public.

* RelayServer  
  The main server component.
  * Needs to be accessible from outside the system.
  * Needs access to the configuration database.
  * Needs access to the message queue.
  * Needs access to the identity server.
  * If this container is started more than once, all relay server instances need to share one storage volume (shared file storage). See the `src/docker/Thinktecture.Relay.Server.Docker/run-container-multiserver.ps1` for required links and mounts.

* Connector  
  This component will be installed on-premises at tenants.
  * Needs access to identity server through public url.
  * Needs access to relay server through public url.

 