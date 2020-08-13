#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src

# Copy only project files for restore
# Dependencies
COPY ["Thinktecture.Relay.Abstractions/Thinktecture.Relay.Abstractions.csproj", "Thinktecture.Relay.Abstractions/"]
COPY ["Thinktecture.Relay.Connector/Thinktecture.Relay.Connector.csproj", "Thinktecture.Relay.Connector/"]
COPY ["Thinktecture.Relay.Connector.Abstractions/Thinktecture.Relay.Connector.Abstractions.csproj", "Thinktecture.Relay.Connector.Abstractions/"]

# Project
COPY ["/docker/Thinktecture.Relay.Connector.Docker/Thinktecture.Relay.Connector.Docker.csproj", "docker/Thinktecture.Relay.Connector.Docker/"]
RUN dotnet restore "docker/Thinktecture.Relay.Connector.Docker/Thinktecture.Relay.Connector.Docker.csproj"

# Copy all sources (complete src directory)
COPY . .

# Build
WORKDIR "/src/docker/Thinktecture.Relay.Connector.Docker"
RUN dotnet build "Thinktecture.Relay.Connector.Docker.csproj" -c Release -o /app/build -p:DISABLE_SOURCELINK=true

FROM build AS publish
RUN dotnet publish "Thinktecture.Relay.Connector.Docker.csproj" -c Release -o /app/publish -p:DISABLE_SOURCELINK=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "Thinktecture.Relay.Connector.Docker.dll"]
