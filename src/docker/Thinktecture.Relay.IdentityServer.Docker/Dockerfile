#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src

COPY ["Thinktecture.Relay.Abstractions/Thinktecture.Relay.Abstractions.csproj", "Thinktecture.Relay.Abstractions/"]
COPY ["Thinktecture.Relay.Server.Abstractions/Thinktecture.Relay.Server.Abstractions.csproj", "Thinktecture.Relay.Server.Abstractions/"]
COPY ["Thinktecture.Relay.Server.Persistence.EntityFrameworkCore/Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.csproj", "Thinktecture.Relay.Server.Persistence.EntityFrameworkCore/"]
COPY ["Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql/Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql.csproj", "Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql/"]
COPY ["Thinktecture.Relay.IdentityServer/Thinktecture.Relay.IdentityServer.csproj", "Thinktecture.Relay.IdentityServer/"]
COPY ["hosts/Thinktecture.Relay.IdentityServer.Docker/Thinktecture.Relay.IdentityServer.Docker.csproj", "hosts/Thinktecture.Relay.IdentityServer.Docker/"]
RUN dotnet restore "hosts/Thinktecture.Relay.IdentityServer.Docker/Thinktecture.Relay.IdentityServer.Docker.csproj"

COPY . .
WORKDIR "/src/hosts/Thinktecture.Relay.IdentityServer.Docker"
RUN dotnet build "Thinktecture.Relay.IdentityServer.Docker.csproj" -c Release -o /app/build -p:DISABLE_SOURCELINK=true

FROM build AS publish
RUN dotnet publish "Thinktecture.Relay.IdentityServer.Docker.csproj" -c Release -o /app/publish -p:DISABLE_SOURCELINK=true

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "Thinktecture.Relay.IdentityServer.Docker.dll"]
