FROM mcr.microsoft.com/dotnet/sdk:6.0 AS dependencies
WORKDIR /src

# Copy only project files for restore
# Dependencies
COPY ./docker/Thinktecture.Relay.Docker/Thinktecture.Relay.Docker.csproj ./docker/Thinktecture.Relay.Docker/
COPY ./Thinktecture.Relay.Abstractions/Thinktecture.Relay.Abstractions.csproj ./Thinktecture.Relay.Abstractions/
COPY ./Thinktecture.Relay.Connector/Thinktecture.Relay.Connector.csproj ./Thinktecture.Relay.Connector/
COPY ./Thinktecture.Relay.Connector.Abstractions/Thinktecture.Relay.Connector.Abstractions.csproj ./Thinktecture.Relay.Connector.Abstractions/
COPY ./Thinktecture.Relay.Connector.Protocols.SignalR/Thinktecture.Relay.Connector.Protocols.SignalR.csproj ./Thinktecture.Relay.Connector.Protocols.SignalR/

# Project
FROM dependencies AS project
COPY ./docker/Thinktecture.Relay.Connector.Docker/Thinktecture.Relay.Connector.Docker.csproj ./docker/Thinktecture.Relay.Connector.Docker/

# Restore packages
FROM project AS restore
WORKDIR /src/docker/Thinktecture.Relay.Connector.Docker
RUN dotnet restore Thinktecture.Relay.Connector.Docker.csproj

# Copy sources
FROM restore AS source
WORKDIR /src

COPY ./Directory.Build.props ./
COPY ./docker/Thinktecture.Relay.Docker ./docker/Thinktecture.Relay.Docker
COPY ./Thinktecture.Relay.Abstractions ./Thinktecture.Relay.Abstractions
COPY ./Thinktecture.Relay.Connector ./Thinktecture.Relay.Connector
COPY ./Thinktecture.Relay.Connector.Abstractions ./Thinktecture.Relay.Connector.Abstractions
COPY ./Thinktecture.Relay.Connector.Protocols.SignalR ./Thinktecture.Relay.Connector.Protocols.SignalR

COPY ./docker/Thinktecture.Relay.Connector.Docker ./docker/Thinktecture.Relay.Connector.Docker

# Build and publish
FROM source AS publish
WORKDIR /src/docker/Thinktecture.Relay.Connector.Docker
RUN dotnet publish Thinktecture.Relay.Connector.Docker.csproj --no-restore -c Release -o /app -p:DisableSourceLink=true

# Create final image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

# Add tooling
RUN apt-get update
RUN apt-get install -y wget
RUN wget -O dotnet-counters https://aka.ms/dotnet-counters/linux-x64
RUN chmod +x dotnet-counters

COPY --from=publish /app .
EXPOSE 80

ENTRYPOINT ["dotnet", "Thinktecture.Relay.Connector.Docker.dll"]
