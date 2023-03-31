FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["samples/ExampleArticleApi/ExampleArticleApi.csproj", "samples/ExampleArticleApi/"]
RUN dotnet restore "samples/ExampleArticleApi/ExampleArticleApi.csproj"
COPY . .
WORKDIR "/src/samples/ExampleArticleApi"
RUN dotnet build "ExampleArticleApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ExampleArticleApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExampleArticleApi.dll"]
