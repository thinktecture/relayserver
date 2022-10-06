#! /bin/sh

dotnet ef migrations add "$1" -v -o Migrations/ConfigurationDb -c RelayDbContext -p ../../Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer -s .
