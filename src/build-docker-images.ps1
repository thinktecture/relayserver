Push-Location $PSScriptRoot

docker build . --force-rm -t relay_server -f ./docker/Thinktecture.Relay.Server.Docker/Dockerfile
docker build . --force-rm -t relay_identityserver -f ./docker/Thinktecture.Relay.IdentityServer.Docker/Dockerfile
docker build . --force-rm -t relay_management -f ./docker/Thinktecture.Relay.ManagementApi.Docker/Dockerfile
docker build . --force-rm -t relay_statistics -f ./docker/Thinktecture.Relay.StatisticsApi.Docker/Dockerfile
docker build . --force-rm -t relay_connector -f ./docker/Thinktecture.Relay.Connector.Docker/Dockerfile

Pop-Location
