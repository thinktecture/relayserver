docker rm -f relay_managementapi

docker run `
  --name relay_managementapi `
  --network relay_network `
  --hostname relay_managementapi `
  -e Serilog__MinimumLevel__Default=Verbose `
  -e Serilog__MinimumLevel__Override__Microsoft=Information `
  -e Serilog__MinimumLevel__Override__Microsoft.Hosting.Lifetime=Information `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://relay_logging_seq `
  -p 5004:5000 `
  -d `
  relay_management
