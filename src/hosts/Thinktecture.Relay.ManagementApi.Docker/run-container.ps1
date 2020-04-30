docker rm -f relay_management

docker run `
  --name relay_management `
  --link relay_persistence_postgresql:database `
  -p 5002:80 `
  -d `
  relay_management
