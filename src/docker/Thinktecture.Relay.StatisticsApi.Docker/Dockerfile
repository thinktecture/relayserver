FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS dependencies
WORKDIR /src

# Copy only project files for restore
# Dependencies
COPY ./docker/Thinktecture.Relay.Docker/Thinktecture.Relay.Docker.csproj ./docker/Thinktecture.Relay.Docker/
COPY ./Thinktecture.Relay.Abstractions/Thinktecture.Relay.Abstractions.csproj ./Thinktecture.Relay.Abstractions/
COPY ./Thinktecture.Relay.Server.Abstractions/Thinktecture.Relay.Server.Abstractions.csproj ./Thinktecture.Relay.Server.Abstractions/
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore/Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.csproj ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore/
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql/Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql.csproj ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql/

# Project
FROM dependencies AS project
COPY ./docker/Thinktecture.Relay.StatisticsApi.Docker/Thinktecture.Relay.StatisticsApi.Docker.csproj ./docker/Thinktecture.Relay.StatisticsApi.Docker/

# Restore packages
FROM project AS restore
WORKDIR /src/docker/Thinktecture.Relay.StatisticsApi.Docker
RUN dotnet restore Thinktecture.Relay.StatisticsApi.Docker.csproj

# Copy sources
FROM restore AS source
WORKDIR /src

COPY ./shared ./shared
COPY ./docker/Thinktecture.Relay.Docker ./docker/Thinktecture.Relay.Docker
COPY ./Thinktecture.Relay.Abstractions ./Thinktecture.Relay.Abstractions
COPY ./Thinktecture.Relay.Server.Abstractions ./Thinktecture.Relay.Server.Abstractions
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql

COPY ./docker/Thinktecture.Relay.StatisticsApi.Docker ./docker/Thinktecture.Relay.StatisticsApi.Docker

# Build and publish
FROM source AS publish
WORKDIR /src/docker/Thinktecture.Relay.StatisticsApi.Docker
RUN dotnet publish Thinktecture.Relay.StatisticsApi.Docker.csproj --no-restore -c Release -o /app -p:DisableSourceLink=true

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim
WORKDIR /app

COPY --from=publish /app .

EXPOSE 80

ENTRYPOINT ["dotnet", "Thinktecture.Relay.StatisticsApi.Docker.dll"]
