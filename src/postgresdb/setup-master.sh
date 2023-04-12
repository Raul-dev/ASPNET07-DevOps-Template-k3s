#!/bin/bash
echo 'SETUP DATABASE'

echo $PGDATA

# Public IP access 
echo "host all all 0.0.0.0/0 trust" >> "$PGDATA/pg_hba.conf"

#cp /dbsetup/master_postgresql.conf $PGDATA/postgresql.conf
#https://www.cybertec-postgresql.com/en/json-logs-in-postgresql-15/
#echo "log_destination = 'jsonlog'" >> $PGDATA/postgresql.conf

echo "logging_collector = on" >> $PGDATA/postgresql.conf
echo "log_directory = '/logs'" >> $PGDATA/postgresql.conf
echo "log_filename = 'postgresql-%Y-%m-%d_%H%M%S.log'" >> $PGDATA/postgresql.conf
echo "log_file_mode=444" >> $PGDATA/postgresql.conf
#chown :999 /logs

#chmod g+s /logs
set -e

echo "CREATE DB "$DB_NAME_API
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" -c "CREATE DATABASE $DB_NAME_API;"
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" -d $DB_NAME_API < ./dbsetup/init_api.sql

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" -l

#Check user
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" -c "SELECT * FROM pg_user;"

echo "finished" > /tmp/finished.ok
ls /tmp

if (test -f /tmp/finished.ok); then echo "yes"; else echo "no"; fi