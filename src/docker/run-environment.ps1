# Helper script that runs a test environment for the RelayServer docker image

Push-Location $PSScriptRoot

./run-dependencies.ps1

# Wait for the DB to start up
Start-Sleep 3

./Thinktecture.Relay.Server.Docker/run-container.ps1
./Thinktecture.Relay.IdentityServer.Docker/run-container.ps1
./Thinktecture.Relay.ManagementApi.Docker/run-container.ps1
./Thinktecture.Relay.StatisticsApi.Docker/run-container.ps1

Pop-Location
