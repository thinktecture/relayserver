# Helper script that runs a test environment for the RelayServer docker image

Push-Location $PSScriptRoot

./run-dependencies.ps1

../hosts/Thinktecture.Relay.Server.Docker/run-container.ps1
../hosts/Thinktecture.Relay.IdentityServer.Docker/run-container.ps1

Pop-Location
