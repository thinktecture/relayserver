docker rm -f relay_server

docker run `
  --name relay_server `
  --network relay_network `
  -p 5000:80 `
  -d `
  relay_server
