FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

# Copy only project files for restore
# Dependencies
COPY ./docker/Thinktecture.Relay.Docker/Thinktecture.Relay.Docker.csproj ./docker/Thinktecture.Relay.Docker/
COPY ./Thinktecture.Relay.Abstractions/Thinktecture.Relay.Abstractions.csproj ./Thinktecture.Relay.Abstractions/
COPY ./Thinktecture.Relay.Server.Abstractions/Thinktecture.Relay.Server.Abstractions.csproj ./Thinktecture.Relay.Server.Abstractions/
COPY ./Thinktecture.Relay.Server.Management/Thinktecture.Relay.Server.Management.csproj ./Thinktecture.Relay.Server.Management/
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore/Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.csproj ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore/
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql/Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql.csproj ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql/
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer/Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer.csproj ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer/
COPY ./docker/Thinktecture.Relay.ManagementApi.Docker/Thinktecture.Relay.ManagementApi.Docker.csproj ./docker/Thinktecture.Relay.ManagementApi.Docker/

# Restore packages
RUN dotnet restore /src/docker/Thinktecture.Relay.ManagementApi.Docker/Thinktecture.Relay.ManagementApi.Docker.csproj

# Once restored, copy everything else that's required for the build
COPY ./Directory.Build.props ./
COPY ./docker/Thinktecture.Relay.Docker ./docker/Thinktecture.Relay.Docker
COPY ./Thinktecture.Relay.Abstractions ./Thinktecture.Relay.Abstractions
COPY ./Thinktecture.Relay.Server.Abstractions ./Thinktecture.Relay.Server.Abstractions
COPY ./Thinktecture.Relay.Server.Management ./Thinktecture.Relay.Server.Management
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer
COPY ./docker/Thinktecture.Relay.ManagementApi.Docker ./docker/Thinktecture.Relay.ManagementApi.Docker

RUN dotnet publish /src/docker/Thinktecture.Relay.ManagementApi.Docker/Thinktecture.Relay.ManagementApi.Docker.csproj --no-restore -c Release -o /app

# Create final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim
WORKDIR /app
RUN adduser -u 5678 --disabled-password --gecos "" relay && chown -R relay /app

# Add .NET Diagnostic Counters
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists && \
  wget -O dotnet-counters https://aka.ms/dotnet-counters/linux-x64 && \
  chmod +x dotnet-counters && \
  apt purge wget --yes

# Address CIS-DI-0008
RUN chmod u-s /usr/bin/gpasswd /usr/bin/chsh /bin/umount /bin/mount /sbin/unix_chkpwd /usr/bin/expiry /usr/bin/newgrp /usr/bin/chage /usr/bin/chfn /usr/bin/wall /usr/bin/passwd /bin/su && \
    chmod g-s /usr/bin/gpasswd /usr/bin/chsh /bin/umount /bin/mount /sbin/unix_chkpwd /usr/bin/expiry /usr/bin/newgrp /usr/bin/chage /usr/bin/chfn /usr/bin/wall /usr/bin/passwd /bin/su

USER relay
COPY --chown=relay --from=build /app .

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV Serilog__Properties__System=Management
ENV Authentication__ApiKey__ApiKeys__read-key__managementapi=read
ENV Authentication__ApiKey__ApiKeys__write-key__managementapi=write
ENV Authentication__ApiKey__ApiKeys__readwrite-key__managementapi=readwrite

ENTRYPOINT ["dotnet", "Thinktecture.Relay.ManagementApi.Docker.dll"]
