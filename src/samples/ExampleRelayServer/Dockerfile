FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["samples/ExampleRelayServer/ExampleRelayServer.csproj", "samples/ExampleRelayServer/"]
RUN dotnet restore "samples/ExampleRelayServer/ExampleRelayServer.csproj"
COPY . .
WORKDIR "/src/samples/ExampleRelayServer"
RUN dotnet build "ExampleRelayServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ExampleRelayServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExampleRelayServer.dll"]
