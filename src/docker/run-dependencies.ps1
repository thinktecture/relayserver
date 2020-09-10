# Helper script that runs a test environment for the RelayServer docker image
docker network create relay_network

docker volume create relay_persistence_postgresql

docker rm -f relay_persistence_postgresql
docker run `
  --name relay_persistence_postgresql `
  --network relay_network `
  --hostname relay_persistence_postgresql `
  -e POSTGRES_USER=relayserver `
  -e POSTGRES_PASSWORD=<Strong!Passw0rd> `
  -v relay_persistence_postgresql:/var/lib/postgresql/data `
  -p 5432:5432 `
  -d `
  postgres

docker volume create relay_transport_rabbitmq1

docker rm -f relay_transport_rabbitmq1
docker run `
  --name relay_transport_rabbitmq1 `
  --network relay_network `
  --hostname relay_transport_rabbitmq1 `
  -e RABBITMQ_DEFAULT_USER=relayserver `
  -e RABBITMQ_DEFAULT_PASS=<Strong!Passw0rd> `
  -e RABBITMQ_ERLANG_COOKIE=<ThisIsSecret> `
  -v relay_transport_rabbitmq1:/var/lib/rabbitmq `
  -p 5672:5672 `
  -p 15672:15672 `
  -d `
  rabbitmq:management

docker volume create relay_transport_rabbitmq2

docker rm -f relay_transport_rabbitmq2
docker run `
  --name relay_transport_rabbitmq2 `
  --network relay_network `
  --hostname relay_transport_rabbitmq2 `
  -e RABBITMQ_DEFAULT_USER=relayserver `
  -e RABBITMQ_DEFAULT_PASS=<Strong!Passw0rd> `
  -e RABBITMQ_ERLANG_COOKIE=<ThisIsSecret> `
  -v relay_transport_rabbitmq2:/var/lib/rabbitmq `
  -p 5673:5672 `
  -p 15673:15672 `
  -d `
  rabbitmq:management

# Wait for rabbits to start up
Start-Sleep 5

# Enable Rabbit clustering
docker exec --user rabbitmq relay_transport_rabbitmq2 rabbitmqctl stop_app
docker exec --user rabbitmq relay_transport_rabbitmq2 rabbitmqctl join_cluster rabbit@relay_transport_rabbitmq1
docker exec --user rabbitmq relay_transport_rabbitmq2 rabbitmqctl start_app

# Set all queues to mirror on all nodes
docker exec --user rabbitmq relay_transport_rabbitmq1 rabbitmqctl set_policy ha "." '{\"ha-mode\":\"all\"}'

docker volume create relay_persistence_seq
docker rm -f relay_logging_seq
docker run `
  --name relay_logging_seq `
  --network relay_network `
  --hostname relay_logging_seq `
  -e ACCEPT_EULA=Y `
  -v relay_persistence_seq:/data `
  -p 5341:80 `
  -d `
  datalust/seq:latest
