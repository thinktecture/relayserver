Push-Location
Set-Location -Path $PSScriptRoot/..

docker build . -t relayserver -f .\hosts\Thinktecture.Relay.Server.Docker\Dockerfile

Pop-Location
