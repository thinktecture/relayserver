docker rm -f relay_connector

docker run `
  --name relay_connector `
  --network relay_network `
  -d `
  relay_connector
