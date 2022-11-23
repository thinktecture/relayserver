Push-Location $PSScriptRoot

./stop-dependencies.ps1

docker stop relay_connector_b2
docker stop relay_connector_b1
docker stop relay_connector_a2
docker stop relay_connector_a1
docker stop relay_statisticsapi
docker stop relay_managementapi
docker stop relay_server_b
docker stop relay_server_a
docker stop relay_identityserver_a
docker stop relay_identityserver_b

docker rm -f relay_persistence_postgresql
docker rm -f relay_transport_rabbitmq1
docker rm -f relay_transport_rabbitmq2
docker rm -f relay_logging_seq

docker rm -f relay_connector_a1
docker rm -f relay_connector_a2
docker rm -f relay_connector_b1
docker rm -f relay_connector_b2
docker rm -f relay_statisticsapi
docker rm -f relay_managementapi
docker rm -f relay_server_a
docker rm -f relay_server_b
docker rm -f relay_identityserver_a
docker rm -f relay_identityserver_b

docker volume rm relay_persistence_seq
docker volume rm relay_persistence_postgresql
docker volume rm relay_persistence_bodystore
docker volume rm relay_persistence_identityserver

docker network rm relay_network

Pop-Location
