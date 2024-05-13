FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

# Copy only project files for restore
# Dependencies
COPY ./docker/Thinktecture.Relay.Docker/Thinktecture.Relay.Docker.csproj ./docker/Thinktecture.Relay.Docker/
COPY ./Thinktecture.Relay.Abstractions/Thinktecture.Relay.Abstractions.csproj ./Thinktecture.Relay.Abstractions/
COPY ./Thinktecture.Relay.Server.Interceptors/Thinktecture.Relay.Server.Interceptors.csproj ./Thinktecture.Relay.Server.Interceptors/
COPY ./Thinktecture.Relay.Server.Abstractions/Thinktecture.Relay.Server.Abstractions.csproj ./Thinktecture.Relay.Server.Abstractions/
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore/Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.csproj ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore/
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql/Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql.csproj ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql/
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer/Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer.csproj ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer/
COPY ./Thinktecture.Relay.Server/Thinktecture.Relay.Server.csproj ./Thinktecture.Relay.Server/
COPY ./Thinktecture.Relay.Server.Protocols.RabbitMq/Thinktecture.Relay.Server.Protocols.RabbitMq.csproj ./Thinktecture.Relay.Server.Protocols.RabbitMq/
COPY ./Thinktecture.Relay.Server.Protocols.SignalR/Thinktecture.Relay.Server.Protocols.SignalR.csproj ./Thinktecture.Relay.Server.Protocols.SignalR/
COPY ./docker/Thinktecture.Relay.Server.Docker/Thinktecture.Relay.Server.Docker.csproj ./docker/Thinktecture.Relay.Server.Docker/

# Restore the entrypoint project
RUN dotnet restore /src/docker/Thinktecture.Relay.Server.Docker/Thinktecture.Relay.Server.Docker.csproj

# Once restored, copy everything else that's required for the build
COPY ./Directory.Build.props ./
COPY ./docker/Thinktecture.Relay.Docker ./docker/Thinktecture.Relay.Docker
COPY ./Thinktecture.Relay.Abstractions ./Thinktecture.Relay.Abstractions
COPY ./Thinktecture.Relay.Server.Interceptors ./Thinktecture.Relay.Server.Interceptors
COPY ./Thinktecture.Relay.Server.Abstractions ./Thinktecture.Relay.Server.Abstractions
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql
COPY ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer ./Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer
COPY ./Thinktecture.Relay.Server ./Thinktecture.Relay.Server
COPY ./Thinktecture.Relay.Server.Protocols.RabbitMq ./Thinktecture.Relay.Server.Protocols.RabbitMq
COPY ./Thinktecture.Relay.Server.Protocols.SignalR ./Thinktecture.Relay.Server.Protocols.SignalR
COPY ./docker/Thinktecture.Relay.Server.Docker ./docker/Thinktecture.Relay.Server.Docker

RUN dotnet publish /src/docker/Thinktecture.Relay.Server.Docker/Thinktecture.Relay.Server.Docker.csproj --no-restore -c Release -o /app

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

RUN mkdir -p /var/bodystore
RUN chown -R relay /var/bodystore

USER relay
COPY --chown=relay --from=build /app .

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV RabbitMq__ClusterHosts=relay_transport_rabbitmq1,relay_transport_rabbitmq2
ENV BodyStore__StoragePath=/var/bodystore
ENV Authentication__Authority=http://relay_identityserver:5000

ENTRYPOINT ["dotnet", "Thinktecture.Relay.Server.Docker.dll"]
