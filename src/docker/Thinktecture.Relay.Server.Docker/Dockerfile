#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src

# Copy only project files for restore
# Dependencies
COPY ["Thinktecture.Relay.Abstractions/Thinktecture.Relay.Abstractions.csproj", "Thinktecture.Relay.Abstractions/"]
COPY ["Thinktecture.Relay.Server/Thinktecture.Relay.Server.csproj", "Thinktecture.Relay.Server/"]
COPY ["Thinktecture.Relay.Server.Abstractions/Thinktecture.Relay.Server.Abstractions.csproj", "Thinktecture.Relay.Server.Abstractions/"]
COPY ["Thinktecture.Relay.Server.Persistence.EntityFrameworkCore/Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.csproj", "Thinktecture.Relay.Server.Persistence.EntityFrameworkCore/"]
COPY ["Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql/Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql.csproj", "Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql/"]
COPY ["Thinktecture.Relay.Server.Protocols.RabbitMq/Thinktecture.Relay.Server.Protocols.RabbitMq.csproj", "Thinktecture.Relay.Server.Protocols.RabbitMq/"]
COPY ["Thinktecture.Relay.Server.Protocols.SignalR/Thinktecture.Relay.Server.Protocols.SignalR.csproj", "Thinktecture.Relay.Server.Protocols.SignalR/"]
COPY ["/docker/Thinktecture.Relay.Docker/Thinktecture.Relay.Docker.csproj", "docker/Thinktecture.Relay.Docker/"]

# Project
COPY ["/docker/Thinktecture.Relay.Server.Docker/Thinktecture.Relay.Server.Docker.csproj", "docker/Thinktecture.Relay.Server.Docker/"]
RUN dotnet restore "docker/Thinktecture.Relay.Server.Docker/Thinktecture.Relay.Server.Docker.csproj"

# Copy all sources (complete src directory)
COPY . .

# Build
WORKDIR "/src/docker/Thinktecture.Relay.Server.Docker"
RUN dotnet build "Thinktecture.Relay.Server.Docker.csproj" -c Release -o /app/build -p:DISABLE_SOURCELINK=true

FROM build AS publish
RUN dotnet publish "Thinktecture.Relay.Server.Docker.csproj" -c Release -o /app/publish -p:DISABLE_SOURCELINK=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "Thinktecture.Relay.Server.Docker.dll"]
