#!/bin/bash
set -e

# Wait until PostgreSQL is ready
until pg_isready -h postgres -p 5432; do
  echo "Waiting for PostgreSQL..."
  sleep 2
done

# Run the commands to configure replication settings and create publication/slot
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
  -- Create a logical replication slot (if not already created)
  DO \$\$
  BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_replication_slots WHERE slot_name = 'debezium_slot') THEN
      PERFORM pg_create_logical_replication_slot('debezium_slot', 'pgoutput');
    END IF;
  END
  \$\$;

  -- Create a publication for all tables (if not already exists)
  CREATE PUBLICATION IF NOT EXISTS debezium_pub FOR ALL TABLES;
EOSQL
