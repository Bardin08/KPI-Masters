FROM confluentinc/cp-kafka-connect:latest

# Install the SpoolDir connector from Confluent Hub
RUN confluent-hub install --no-prompt jcustenborder/kafka-connect-spooldir:latest

# Install the Debezium Postgres Connector from Confluent Hub
RUN confluent-hub install --no-prompt debezium/debezium-connector-postgresql:2.5.4
