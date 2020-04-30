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
