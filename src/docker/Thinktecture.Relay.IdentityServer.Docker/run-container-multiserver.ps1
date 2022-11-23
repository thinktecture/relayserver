docker volume create relay_persistence_identityserver

docker rm -f relay_identityserver_a
docker run `
  --name relay_identityserver_a `
  --network relay_network `
  --volume relay_persistence_identityserver:/var/signingkeys `
  -e Serilog__MinimumLevel__Default=Verbose `
  -e Serilog__MinimumLevel__Override__Microsoft=Information `
  -e Serilog__MinimumLevel__Override__Microsoft.Hosting.Lifetime=Information `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://relay_logging_seq `
  -e Serilog__Properties__System=IdentityServer_A `
  -p 5002:5000 `
  -d `
  relay_identityserver

docker rm -f relay_identityserver_b
docker run `
  --name relay_identityserver_b `
  --network relay_network `
  --volume relay_persistence_identityserver:/var/signingkeys `
  -e Serilog__MinimumLevel__Default=Verbose `
  -e Serilog__MinimumLevel__Override__Microsoft=Information `
  -e Serilog__MinimumLevel__Override__Microsoft.Hosting.Lifetime=Information `
  -e Serilog__WriteTo__0__Name=Seq `
  -e Serilog__WriteTo__0__Args__ServerUrl=http://relay_logging_seq `
  -e Serilog__Properties__System=IdentityServer_B `
  -p 5003:5000 `
  -d `
  relay_identityserver
