# How to use this test environment

## What's in it

The Test-Environment consists of:
  * Docker-containers for
    * SQL Server 2017 (`db`) 
      * Exposed on port `1433`
      * Admin user `sa` with password `<Strong!Password>`
    * RabbitMQ (`queue`)
      * Management UI exposed on port `15672`
      * Admin user `guest` with password `guest`
    * Seq Server (`seq`)
      * Management UI & API exposed on port `5341`
      * Single-User mode
  * A DB Script which seeds initial RelayServer User and Link
    * Admin user `admin` with password `<Strong!Passw0rd>`
    * Initial link `test` with password used in OPC default config

## Prerequisites

Make sure you have docker and docker-compose installed and running.

## Initial setup of Test environment

1. Execute `docker-compose up`  
   This will first download, then create and launch the required containers.
2. Execute `docker-compose exec db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "<Strong!Passw0rd>" -i /var/scripts/create.sql`  
   This will create the initial database and seed it with test data.  
   If you delete the `db` container and/or its volumes, you might want to execute the db script again to re-create the database and have the initial ManagementWeb-User and Test link recreated.

## Subsequent starts

You only need to start the containers (`docker-compose up`).
