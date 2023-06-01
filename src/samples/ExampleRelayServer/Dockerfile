FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Coopy project file
COPY ["samples/ExampleRelayServer/ExampleRelayServer.csproj", "samples/ExampleRelayServer/"]

# Copy direct dependency project files for restore
COPY ["Thinktecture.Relay.IdentityServer/Thinktecture.Relay.IdentityServer.csproj", "Thinktecture.Relay.IdentityServer/"]
COPY ["Thinktecture.Relay.Interceptors/Thinktecture.Relay.Interceptors.csproj", "Thinktecture.Relay.Interceptors/"]
COPY ["Thinktecture.Relay.Server.Persistence.EntityFrameworkCore/Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.csproj", "Thinktecture.Relay.Server.Persistence.EntityFrameworkCore/"]
COPY ["Thinktecture.Relay.Server.Protocols.RabbitMq/Thinktecture.Relay.Server.Protocols.RabbitMq.csproj", "Thinktecture.Relay.Server.Protocols.RabbitMq/"]
COPY ["Thinktecture.Relay.Server.Protocols.SignalR/Thinktecture.Relay.Server.Protocols.SignalR.csproj", "Thinktecture.Relay.Server.Protocols.SignalR/"]
COPY ["Thinktecture.Relay.Server/Thinktecture.Relay.Server.csproj", "Thinktecture.Relay.Server/"]

# Copy indirect dependency project files for restore
COPY ["Thinktecture.Relay.Server.Abstractions/Thinktecture.Relay.Server.Abstractions.csproj", "Thinktecture.Relay.Server.Abstractions/"]
COPY ["Thinktecture.Relay.Abstractions/Thinktecture.Relay.Abstractions.csproj", "Thinktecture.Relay.Abstractions/"]

RUN dotnet restore "samples/ExampleRelayServer/ExampleRelayServer.csproj"

# Copy rest of source code from build context
COPY . .

WORKDIR "/src/samples/ExampleRelayServer"
RUN dotnet build "ExampleRelayServer.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "ExampleRelayServer.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExampleRelayServer.dll"]
