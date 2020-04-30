docker rm -f relay_server

docker run `
  --name relay_server `
  --link relay_persistence_postgresql:database `
  -p 5000:80 `
  -d `
  relay_server
