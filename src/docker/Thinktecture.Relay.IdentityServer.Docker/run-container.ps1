docker rm -f relay_identityserver

docker run `
  --name relay_identityserver `
  --link relay_persistence_postgresql:database `
  -p 5002:80 `
  -d `
  relayserver_identityserver
