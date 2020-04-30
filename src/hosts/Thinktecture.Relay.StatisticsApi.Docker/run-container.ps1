docker rm -f relay_statistics

docker run `
  --name relay_statistics `
  --link relay_persistence_postgresql:database `
  -p 5003:80 `
  -d `
  relay_statistics
