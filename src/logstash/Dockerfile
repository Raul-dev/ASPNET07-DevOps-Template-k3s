FROM logstash:8.5.3 

RUN cat /etc/os-release
USER root

#Logs setup
# ./configs/log4j2.properties /usr/share/logstash/config/log4j2.properties
COPY ./configs/logstash.yml /usr/share/logstash/config/logstash.yml
#COPY ./configs/pipelines.yml /usr/share/logstash/config/pipelines.yml
COPY ./configs/postgres.yml /usr/share/logstash/config/pipelines.yml
COPY ./pipelines/logstash.conf /usr/share/logstash/config/pipelines/service_stamped_json_logs.conf
COPY ./pipelines/postgres.conf /usr/share/logstash/config/pipelines/postgres_logs.conf

RUN chmod +rwx /usr/share/logstash/config/pipelines/service_stamped_json_logs.conf
RUN chown logstash /usr/share/logstash/config/pipelines/service_stamped_json_logs.conf
RUN chmod +rwx /usr/share/logstash/config/pipelines/postgres_logs.conf
RUN chown logstash /usr/share/logstash/config/pipelines/postgres_logs.conf

USER root
RUN mkdir /logs

USER logstash
