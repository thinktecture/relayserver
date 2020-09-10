docker rm -f relay_identityserver

docker run `
  --name relay_identityserver `
  --network relay_network `
  -p 5002:80 `
  -d `
  relay_identityserver
