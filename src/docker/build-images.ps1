Push-Location
Set-Location -Path $PSScriptRoot/..

docker build . -t relayserver -f .\hosts\Thinktecture.Relay.Server.Docker\Dockerfile
docker build . -t relay_identityserver -f .\hosts\Thinktecture.Relay.IdentityServer.Docker\Dockerfile

Pop-Location
