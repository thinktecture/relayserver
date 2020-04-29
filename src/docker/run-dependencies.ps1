# Helper script that runs a test environment for the RelayServer docker image

docker volume create relayserver_database_postgresql_persistence

docker rm -f relayserver_postgresql
docker run `
  --name relayserver_postgresql `
  -e POSTGRES_USER=relayserver `
  -e POSTGRES_PASSWORD=<Strong!Passw0rd> `
  -v relayserver_database_postgresql_persistence:/var/lib/postgresql/data `
  -p 5432:5432 `
  -d `
  postgres
