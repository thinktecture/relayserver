FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

# Copy only project files for restore
COPY ./docker/Thinktecture.Relay.Docker/Thinktecture.Relay.Docker.csproj ./docker/Thinktecture.Relay.Docker/
COPY ./Thinktecture.Relay.Abstractions/Thinktecture.Relay.Abstractions.csproj ./Thinktecture.Relay.Abstractions/
COPY ./Thinktecture.Relay.Connector/Thinktecture.Relay.Connector.csproj ./Thinktecture.Relay.Connector/
COPY ./Thinktecture.Relay.Connector.Abstractions/Thinktecture.Relay.Connector.Abstractions.csproj ./Thinktecture.Relay.Connector.Abstractions/
COPY ./Thinktecture.Relay.Connector.Protocols.SignalR/Thinktecture.Relay.Connector.Protocols.SignalR.csproj ./Thinktecture.Relay.Connector.Protocols.SignalR/
COPY ./docker/Thinktecture.Relay.Connector.Docker/Thinktecture.Relay.Connector.Docker.csproj ./docker/Thinktecture.Relay.Connector.Docker/

# Restore the entrypoint project
RUN dotnet restore /src/docker/Thinktecture.Relay.Connector.Docker/Thinktecture.Relay.Connector.Docker.csproj

# Once restored, copy everything else that's required for the build
COPY ./Directory.Build.props ./
COPY ./docker/Thinktecture.Relay.Docker ./docker/Thinktecture.Relay.Docker
COPY ./Thinktecture.Relay.Abstractions ./Thinktecture.Relay.Abstractions
COPY ./Thinktecture.Relay.Connector ./Thinktecture.Relay.Connector
COPY ./Thinktecture.Relay.Connector.Abstractions ./Thinktecture.Relay.Connector.Abstractions
COPY ./Thinktecture.Relay.Connector.Protocols.SignalR ./Thinktecture.Relay.Connector.Protocols.SignalR
COPY ./docker/Thinktecture.Relay.Connector.Docker ./docker/Thinktecture.Relay.Connector.Docker

RUN dotnet publish /src/docker/Thinktecture.Relay.Connector.Docker/Thinktecture.Relay.Connector.Docker.csproj --no-restore -c Release -o /app -p:DisableSourceLink=true

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

ENV DOTNET_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "Thinktecture.Relay.Connector.Docker.dll"]
