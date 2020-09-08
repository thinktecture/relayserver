# Helper script that runs a test environment for the RelayServer docker image

docker volume create relay_persistence_postgresql

docker rm -f relay_persistence_postgresql
docker run `
  --name relay_persistence_postgresql `
  -e POSTGRES_USER=relayserver `
  -e POSTGRES_PASSWORD=<Strong!Passw0rd> `
  -v relay_persistence_postgresql:/var/lib/postgresql/data `
  -p 5432:5432 `
  -d `
  postgres

docker volume create relay_transport_rabbitmq

docker rm -f relay_transport_rabbitmq
docker run `
  --name relay_transport_rabbitmq `
  --hostname relay-transport-rabbitmq `
  -e RABBITMQ_DEFAULT_USER=relayserver `
  -e RABBITMQ_DEFAULT_PASS=<Strong!Passw0rd> `
  -v relay_transport_rabbitmq:/var/lib/rabbitmq `
  -p 5672:5672 `
  -p 15672:15672 `
  -d `
  rabbitmq:management

docker volume create relay_persistence_seq
docker rm -f relay_logging_seq
docker run `
  --name relay_logging_seq `
  --hostname relay-logging-seq `
  -e ACCEPT_EULA=Y `
  -v relay_persistence_seq:/data `
  -p 5341:80 `
  -d `
  datalust/seq:latest
