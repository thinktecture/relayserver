docker rm -f relay_managementapi

docker run `
  --name relay_managementapi `
  --network relay_network `
  --hostname relay_managementapi `
  -p 5004:80 `
  -d `
  relay_management
