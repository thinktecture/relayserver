dotnet publish -o service

$exe = "$(Get-Location)/service/Thinktecture.Relay.Connector.Docker.exe"
New-Service -Name RelayServer -BinaryPathName $exe -Credential $identity -StartupType Manual
