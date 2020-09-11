docker volume create relay_persistence_bodystore

docker rm -f relay_server_a
docker run `
  --name relay_server_a `
  --network relay_network `
  --volume relay_persistence_bodystore:/var/bodystore `
  -e Serilog__MinimumLevel__Default=Verbose `
  -e Serilog__MinimumLevel__Override__Microsoft=Information `
  -e Serilog__MinimumLevel__Override__Microsoft.Hosting.Lifetime=Information `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://relay_logging_seq `
  -e Serilog__Properties__System=RelayServerA `
  -e RabbitMq__ClusterHosts=relay_transport_rabbitmq1,relay_transport_rabbitmq2 `
  -e BodyStore__StoragePath=/var/bodystore `
  -p 5010:80 `
  -d `
  relay_server

docker rm -f relay_server_b
docker run `
  --name relay_server_b `
  --network relay_network `
  --volume relay_persistence_bodystore:/var/bodystore `
  -e Serilog__MinimumLevel__Default=Verbose `
  -e Serilog__MinimumLevel__Override__Microsoft=Information `
  -e Serilog__MinimumLevel__Override__Microsoft.Hosting.Lifetime=Information `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://relay_logging_seq `
  -e Serilog__Properties__System=RelayServerB `
  -e RabbitMq__Uri=amqp://relayserver:<Strong!Passw0rd>@relay_transport_rabbitmq2 `
  -e RabbitMq__ClusterHosts=relay_transport_rabbitmq2,relay_transport_rabbitmq1 `
  -e BodyStore__StoragePath=/var/bodystore `
  -p 5011:80 `
  -d `
  relay_server
