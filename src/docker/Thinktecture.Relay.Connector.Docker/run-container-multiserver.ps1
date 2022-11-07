docker rm -f relay_connector_a1
docker run `
  --name relay_connector_a1 `
  --network relay_network `
  -e DOTNET_ENVIRONMENT=Development `
  -e ASPNETCORE_ENVIRONMENT=Development `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://relay_logging_seq `
  -e Serilog__Properties__System=Connector_A1 `
  -e RelayConnector__RelayServerBaseUri=http://relay_server_a:5000 `
  -d `
  relay_connector

docker rm -f relay_connector_a2
docker run `
  --name relay_connector_a2 `
  --network relay_network `
  -e DOTNET_ENVIRONMENT=Development `
  -e ASPNETCORE_ENVIRONMENT=Development `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://relay_logging_seq `
  -e Serilog__Properties__System=Connector_A2 `
  -e RelayConnector__RelayServerBaseUri=http://relay_server_a:5000 `
  -d `
  relay_connector


docker rm -f relay_connector_b1
docker run `
  --name relay_connector_b1 `
  --network relay_network `
  -e DOTNET_ENVIRONMENT=Development `
  -e ASPNETCORE_ENVIRONMENT=Development `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://relay_logging_seq `
  -e Serilog__Properties__System=Connector_B1 `
  -e RelayConnector__RelayServerBaseUri=http://relay_server_b:5000 `
  -e RelayConnector__TenantName=TestTenant2 `
  -d `
  relay_connector

docker rm -f relay_connector_b2
docker run `
  --name relay_connector_b2 `
  --network relay_network `
  -e DOTNET_ENVIRONMENT=Development `
  -e ASPNETCORE_ENVIRONMENT=Development `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://relay_logging_seq `
  -e Serilog__Properties__System=Connector_B2 `
  -e RelayConnector__RelayServerBaseUri=http://relay_server_b:5000 `
  -e RelayConnector__TenantName=TestTenant2 `
  -d `
  relay_connector
