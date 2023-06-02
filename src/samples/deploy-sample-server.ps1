
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string] $resourceGroupName,
    [Parameter(Mandatory = $true)]
    [string] $dnsNameLabel,
    [Parameter(Mandatory = $true)]
    [string] $email,
    [Parameter(Mandatory = $false)]
    [string] $location,
    [Parameter(Mandatory = $false)]
    [string] $acrName
)

function Wait-For-ContainerGroup {
  param(
    [string] $Url
  )

  $containerReady = $false
  $retries = 6; # about a minute wait time with a 5 sec timeout timeout

  while ($containerReady -eq $false -and $retries -gt 0) {
    $retries = $retries - 1
    Start-Sleep -Seconds 5

    try {
      $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 5
      if ($response.StatusCode -eq 200) {
        Write-Host "Container group answered successfully"
        $containerReady = $true
      }
    } catch {
      Write-Host "Error while checking container group status: $_"
    }
  }

  return $containerReady
}

## Fill all default values if not provided
if ($location -eq '') {
  $location = 'westeurope'
}
if ($acrName -eq '') {
  $acrName = 'relaydemoacr'
}

## First, try to build the server docker image
Write-Host "Building relay server docker image"
docker rmi -f examplerelayserver
docker build -f ./ExampleRelayServer/Dockerfile -t examplerelayserver:latest ..

if($LASTEXITCODE -ne 0) {
  Write-Error "Docker image build failed, please fix errors and try again"
  exit 1
}

docker tag examplerelayserver:latest "$acrName.azurecr.io/relayserver:v1"

## Then try to build the deployables
dotnet publish ./ExampleArticleApi --configuration Release --output ./dist/mac/api --self-contained --os osx
dotnet publish ./ExampleArticleClient --configuration Release --output ./dist/mac/client --self-contained --os osx
dotnet publish ./ExampleConnector --configuration Release --output ./dist/mac/connector --self-contained --os osx

dotnet publish ./ExampleArticleApi --configuration Release --output ./dist/win/api --self-contained --os win
dotnet publish ./ExampleArticleClient --configuration Release --output ./dist/win/client --self-contained --os win
dotnet publish ./ExampleConnector --configuration Release --output ./dist/win/connector --self-contained --os win

## Create a new resource group
Write-Host "Creating resource group $resourceGroupName in $location"
az group create --name $resourceGroupName --location $location

## Create an prepare a new Container Registry to publish our relayserver container image to
Write-Host "Creating and preparing ACR $acrName"
az acr create --resource-group $resourceGroupName --name $acrName --sku Premium
az acr update --name $acrName --anonymous-pull-enabled

## We ned to wait until our ACR is reachable, so we request the url https://relaydemoacr.azurecr.io/v2/
## until we get a 401 response as we are not authenticated. This however means the ACR is ready to accept
## our login and subsequent container push
Write-Host "Waiting for ACR to be ready"
$acrReady = $false
while ($acrReady -eq $false) {
  try {
    Invoke-WebRequest -Uri "https://$acrName.azurecr.io/v2/" -UseBasicParsing
  } catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
      $acrReady = $true
    }
  }

  Write-Host "ACR $acrName not ready yet, waiting 5 seconds"
  Start-Sleep -Seconds 5
}

Write-Host "Logging into ACR"
az acr login --name $acrName

## Push our pre-built docker image to the ACR
Write-Host "Pushing docker image to ACR $acrName"
docker push "$acrName.azurecr.io/relayserver:v1"

## Create a new container instance based on our yaml file
Write-Host "Creating container instance"

## Replace all placeholders in the yaml file and create a deploy file
(Get-Content container-group.yaml) `
  -Replace '%dnsNameLabel%', "$dnsNameLabel" `
  -Replace '%email%', "$email" `
  -Replace '%location%', "$location" `
  -Replace '%acrName%', "$acrName" `
  | Out-File deploy.yaml -Force

az container create `
  --resource-group $resourceGroupName `
  --file deploy.yaml `

Remove-Item deploy.yaml

Write-Host "Waiting for the container group to be ready..."
$ready = Wait-For-ContainerGroup "https://$dnsNameLabel.$location.azurecontainer.io/.well-known/relayserver-configuration"

if ($ready -eq $false) {
  Write-Host "Container group not ready yet, forcing restart"
  az container restart --resource-group $resourceGroupName --name relayserver-demo-group
  $ready = Wait-For-ContainerGroup "https://$dnsNameLabel.$location.azurecontainer.io/.well-known/relayserver-configuration"
}

if ($ready -eq $false) {
  Write-Error "Container group not ready after 2 minutes and restart, please check the logs"
  exit 1
}

Write-Host "RelayServer should be up and running now, please check 'https://$dnsNameLabel.$location.azurecontainer.io/.well-known/relayserver-configuration' "
Write-Host "Do not forget to run 'az group delete --name $resourceGroupName --yes' when your are finished, to keep the Azure bill lean"
