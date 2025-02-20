FROM postgres:16

# Copy the startup script into the initialization directory
COPY init_replication.sh /docker-entrypoint-initdb.d/

# Set executable permission on the script
RUN chmod +x /docker-entrypoint-initdb.d/init_replication.sh
