docker rm -f relay_identityserver

docker run `
  --name relay_identityserver `
  --link relayserver_postgresql:database `
  -p 5003:443 `
  -p 5002:80 `
  -d `
  relay_identityserver
