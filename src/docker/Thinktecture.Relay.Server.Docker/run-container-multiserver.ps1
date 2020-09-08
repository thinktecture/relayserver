docker volume create relay_persistence_bodystore

docker rm -f relay_server_a
docker run `
  --name relay_server_a `
  --link relay_identityserver:relay_identityserver `
  --link relay_persistence_postgresql:database `
  --link relay_transport_rabbitmq:transport `
  --link relay_logging_seq:logs `
  --volume relay_persistence_bodystore:/var/bodystore `
  -e Serilog__MinimumLevel__Default=Verbose `
  -e Serilog__MinimumLevel__Override__Microsoft=Information `
  -e Serilog__MinimumLevel__Override__Microsoft.Hosting.Lifetime=Information `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://logs `
  -e Serilog__Properties__System=RelayServerA `
  -p 5010:80 `
  -d `
  relay_server

docker rm -f relay_server_b
docker run `
  --name relay_server_b `
  --link relay_identityserver:relay_identityserver `
  --link relay_persistence_postgresql:database `
  --link relay_transport_rabbitmq:transport `
  --link relay_logging_seq:logs `
  --volume relay_persistence_bodystore:/var/bodystore `
  -e Serilog__MinimumLevel__Default=Verbose `
  -e Serilog__MinimumLevel__Override__Microsoft=Information `
  -e Serilog__MinimumLevel__Override__Microsoft.Hosting.Lifetime=Information `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://logs `
  -e Serilog__Properties__System=RelayServerB `
  -p 5011:80 `
  -d `
  relay_server
