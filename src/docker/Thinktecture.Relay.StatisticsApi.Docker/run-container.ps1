docker rm -f relay_statisticsapi

docker run `
  --name relay_statisticsapi `
  --network relay_network `
  --hostname relay_statisticsapi `
  -p 5006:5000 `
  -d `
  relay_statistics
