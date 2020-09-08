docker rm -f relay_server

docker run `
  --name relay_server `
  --link relay_identityserver:relay_identityserver `
  --link relay_persistence_postgresql:database `
  --link relay_transport_rabbitmq:transport `
  -p 5000:80 `
  -d `
  relay_server
