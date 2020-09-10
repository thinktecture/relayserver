docker rm -f relay_management

docker run `
  --name relay_management `
  --network relay_network `
  --hostname relay_managementapi `
  -p 5004:80 `
  -d `
  relay_management
