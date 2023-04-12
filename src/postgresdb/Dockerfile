FROM postgres:15.0-alpine
ADD ./dbsetup /dbsetup
USER root
RUN mkdir /logs
RUN chmod +rw /logs
COPY init_api.sql ./dbsetup/init_api.sql
COPY ./setup-master.sh /docker-entrypoint-initdb.d/setup-master.sh