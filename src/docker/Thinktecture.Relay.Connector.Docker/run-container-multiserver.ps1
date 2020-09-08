docker rm -f relay_connector_a_1
docker run `
  --name relay_connector_a_1 `
  --link relay_identityserver:relay_identityserver `
  --link relay_server_a:relay_server `
  --link relay_logging_seq:logs `
  --link relay_identityserver:relay_identityserver `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://logs `
  -e Serilog__Properties__System=Connector_A_1 `
  -d `
  relay_connector

docker rm -f relay_connector_a_2
docker run `
  --name relay_connector_a_2 `
  --link relay_identityserver:relay_identityserver `
  --link relay_server_a:relay_server `
  --link relay_logging_seq:logs `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://logs `
  -e Serilog__Properties__System=Connector_A_2 `
  -e RelayConnector__TenantName=TestTenant2 `
  -d `
  relay_connector


docker rm -f relay_connector_b_1
docker run `
  --name relay_connector_b_1 `
  --link relay_identityserver:relay_identityserver `
  --link relay_server_b:relay_server `
  --link relay_logging_seq:logs `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://logs `
  -e Serilog__Properties__System=Connector_B_1 `
  -d `
  relay_connector

docker rm -f relay_connector_b_2
docker run `
  --name relay_connector_b_2 `
  --link relay_identityserver:relay_identityserver `
  --link relay_server_b:relay_server `
  --link relay_logging_seq:logs `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://logs `
  -e Serilog__Properties__System=Connector_B_2 `
  -e RelayConnector__TenantName=TestTenant2 `
  -d `
  relay_connector
