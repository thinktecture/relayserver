docker rm -f relayserver

docker run `
  --name relayserver `
  --link relayserver_postgresql:database `
  -p 5001:443 `
  -p 5000:80 `
  -d `
  relayserver
