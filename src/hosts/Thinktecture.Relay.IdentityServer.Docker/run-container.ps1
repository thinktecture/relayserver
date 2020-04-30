docker rm -f relay_identityserver

docker run `
  --name relay_identityserver `
  --link relay_persistence_postgresql:database `
  -p 5001:80 `
  -d `
  relay_identityserver
