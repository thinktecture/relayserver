# RelayServer Deployment

RelayServer v3 is designed for containerized deployments using Docker.

_Note:_ The containers in this repository are intended for **demo and example** purposes only. It is strongly encouraged
to create your own host applications tailored and configured to your specific needs and to build and use your own
container images.

While at least the server-side components are intended to be deployed in containers, it is of course still possible to
build a custom host executable for each service and run it as a Windows service or Linux daemon.

## Introduction

RelayServer deployments consist of multiple instances (pods) of different containers which handle specific tasks.

## Prerequisites

* RabbitMQ  
  The default server-to-server transport channels are based on the RabbitMQ message queue service. RelayServer needs at
  least one single functional RabbitMQ node to connect to. However, it is strongly suggested that you provide a
  multi-node RabbitMQ cluster so that the system can continue to work in case of any failure (i.e. hardware problem)
  that might cause a RabbitMQ node to be unavailable for a certain amount of time.

  **DO** set up and operate a reliable, highly available (HA) and performant RabbitMQ cluster.  
  **Do NOT** use the example / development RabbitMQ containers from this repository in production.

* Database  
  RelayServer by default supports both PostgreSQL and Microsoft SQL Server for configuration and statistics storage. As
  with the message queue, you need a reliable HA database cluster when the RelayServer setup should be resilient to
  failures.

  **DO** set up and operate a reliable, highly available (HA) and performant PostgreSQL or Microsoft SQL Server
  cluster.  
  **Do NOT** use the example / development database containers from this repository in production.

* Access Token Provider  
  RelayServer v3 requires an OIDC compliant identity provider instance to provide access tokens for the connectors. This
  instance should be set up and configured to provide the required tokens for the tenants.  
  **DO** set up and operate a reliable, highly available (HA) and performant OIDC server instance.  
  **Do NOT** use the example / development Keycloak container from this repository in production.

## RelayServer Components

* Thinktecture.Relay.Server
  This is the main component of the RelayServer. It provides all endpoints required for the connectors and it also
  provides the endpoints for any client to send a request to a target. The load of this component is highly dynamic. It
  increases and decreases based on how many clients send how many requests against the relayed targets.

  **DO** create your own project using the RelayServer v3 NuGet packages. Use the corresponding persistence packages for
  the database of your choice.  
  **DO** run at least 2 instances of this service on different machines in order to provide a reliable system.  
  Consider providing automatic scaling functionality of this component based on load.

  Configuration:
  * Provide a shared volume to all instances of Thinktecture.Relay.Server i.e. on /var/bodystore and add a
  configuration property for the storage path to this directory
  * Provide a authority url pointing to your OIDC compliant identity provider instance 
  * Provide a connection string to the RelayServer database
  * Provide a connection string to the RabbitMQ

* Thinktecture.Relay.ManagementApi  
  This component is used to manage entries in the RelayServer database, i.e. add a configuration for a new tenant / 
  connector. These actions usually are not performed too often and load is expected to be idle for most of the time.  
  If not absoluteley required we suggested not exposing this container to the public internet. Have it available only
  for your internal administrative usages. Unless you have higher load and reliability requirements and/or perform a
  lot of automatic provisioning of connectors, it should be fine to run only a single instance.

  **DO** create your own project using the RelayServer v3 NuGet packages. Use the corresponding persistence packages for
  the database of your choice.  
  **Do NOT** use the example / development project and docker file from this repository in production.

  Configuration:
  * Provide a connection string to the RelayServer database 

## Connector

* Thinktecture.Relay.Connector  
  This component is the connector part and needs to be installed at every on-premises location (_Tenant_). The
  connector _can_ be run as docker container and also be scaled up. Multiple concurrent connectors for the same tenant
  will automatically load-balance requests between them and also keep a tenant connected when one connector fails.
  You can, however, also install the connector as a Windows service or Linux daemon.

  **DO** create your own project using the RelayServer v3 NuGet packages. Use the corresponding .NET features for
  deployment as a Windows Service / Linux daemon or docker deployment depending on your use case.  
  **Do NOT** use the example / development project and docker file from this repository in production.  

  Configuration:
  * Provide the URL to the RelayServer
  * Provide tenant name and secret for authentication
