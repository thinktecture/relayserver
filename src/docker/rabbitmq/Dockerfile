FROM rabbitmq:management-alpine

# add erlang cookie file so that all nodes use the same secrets
COPY --chown=rabbitmq:rabbitmq .erlang.cookie /var/lib/rabbitmq/.erlang.cookie
RUN chmod 400 /var/lib/rabbitmq/.erlang.cookie

# add start script for clustering
COPY --chown=rabbitmq:rabbitmq startrabbit.sh /opt/rabbit/startrabbit.sh
RUN chmod a+x /opt/rabbit/startrabbit.sh

USER rabbitmq

EXPOSE 4369
EXPOSE 5672
EXPOSE 15672
EXPOSE 25672

CMD /opt/rabbit/startrabbit.sh
