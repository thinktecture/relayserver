$resourceGroupName = "tt-relaydemo"

az group create --name $resourceGroupName --location westeurope
Start-Sleep -seconds 10

az acr create --resource-group $resourceGroupName --name relaydemoacr --sku Premium
Start-Sleep -seconds 10

az acr update --name relaydemoacr --anonymous-pull-enabled
Start-Sleep -seconds 10

az acr login --name relaydemoacr

Start-Sleep -seconds 10
docker rmi -f examplerelayserver
docker build -f ./ExampleRelayServer/Dockerfile -t examplerelayserver:latest ..
docker tag examplerelayserver:latest relaydemoacr.azurecr.io/relayserver:v1
docker push relaydemoacr.azurecr.io/relayserver:v1

Start-Sleep -seconds 10

## Make sure to replace the domain name and email in the container-group.yaml file
az container create --resource-group $resourceGroupName --file container-group.yaml
