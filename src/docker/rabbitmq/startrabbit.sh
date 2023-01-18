#!/bin/bash

. ~/.bashrc

HOSTNAME=`env hostname`

if [ -z "$CLUSTERED" ]; then
    # If not clustered then start it normally as standalone server
    rabbitmq-server &
    rabbitmqctl wait /var/lib/rabbitmq/mnesia/rabbit\@$HOSTNAME.pid
    #tail -f /var/log/rabbitmq/rabbit\@$HOSTNAME.log
    tail -f /var/log/rabbitmq/*.log
else
    if [ -z "$CLUSTER_WITH" ]; then
        # If clustered, but cluster with is not specified then again start normally, could be the first server in the cluster
        rabbitmq-server &
        sleep 5
        rabbitmqctl wait /var/lib/rabbitmq/mnesia/rabbit\@$HOSTNAME.pid
        tail -f /var/log/rabbitmq/rabbit\@$HOSTNAME.log
    else
      rabbitmq-server -detached
      rabbitmqctl wait /var/lib/rabbitmq/mnesia/rabbit\@$HOSTNAME.pid
      rabbitmqctl stop_app
      if [ -z "$RAM_NODE" ]; then
          rabbitmqctl join_cluster rabbit@$CLUSTER_WITH
      else
          rabbitmqctl join_cluster --ram rabbit@$CLUSTER_WITH
      fi
      rabbitmqctl start_app

      #tail to keep foreground process active ...
      tail -f /var/log/rabbitmq/*.log
    fi
fi
