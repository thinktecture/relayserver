Push-Location
Set-Location -Path $PSScriptRoot/..

docker build . -t relay_server -f .\hosts\Thinktecture.Relay.Server.Docker\Dockerfile
docker build . -t relay_identityserver -f .\hosts\Thinktecture.Relay.IdentityServer.Docker\Dockerfile
docker build . -t relay_management -f .\hosts\Thinktecture.Relay.ManagementApi.Docker\Dockerfile
docker build . -t relay_statistics -f .\hosts\Thinktecture.Relay.StatisticsApi.Docker\Dockerfile

Pop-Location
