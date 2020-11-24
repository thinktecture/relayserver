# Helper script that runs a test environment for the RelayServer docker image

Push-Location $PSScriptRoot

./run-dependencies.ps1

# Wait for the DB and rabbit to start up
Start-Sleep 3

# First apply migrations
./Thinktecture.Relay.Server.Docker/run-container.ps1 migrate-only=true

./Thinktecture.Relay.IdentityServer.Docker/run-container.ps1
./Thinktecture.Relay.Server.Docker/run-container.ps1
./Thinktecture.Relay.ManagementApi.Docker/run-container.ps1
./Thinktecture.Relay.StatisticsApi.Docker/run-container.ps1
./Thinktecture.Relay.Connector.Docker/run-container.ps1

Pop-Location
