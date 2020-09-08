docker rm -f relay_connector

docker run `
  --name relay_connector `
  --link relay_identityserver:relay_identityserver `
  --link relay_server:relay_server `
  -d `
  relay_connector
