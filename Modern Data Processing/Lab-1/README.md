# Kafka Cluster Deployment Guide 🚀

This guide explains **step-by-step** how to run our Kafka cluster with Graylog, Prometheus, Grafana, Kafka Connect, and Kafka UI using Docker Compose. Follow along to deploy, monitor, and manage your Kafka environment in KRaft mode! 💥

---

## Table of Contents

- [Prerequisites](#prerequisites)
- [Setup & Running the Cluster](#setup--running-the-cluster)
- [Accessing the Services](#accessing-the-services)
- [Network Ports Table](#network-ports-table)
- [Kafka Broker Configurations](#kafka-broker-configurations)
- [Dockerfiles Overview](#dockerfiles-overview)
- [Troubleshooting Tips](#troubleshooting-tips)

---

## Prerequisites ✅

- **Docker** and **Docker Compose** installed on your machine.
- Basic knowledge of Docker networking and volume mounting.
- Clone this repository to your local machine.

---

## Setup & Running the Cluster 🏃‍♂️

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd <repository-directory>
   ```

2. **Verify your configuration files**:  
   Ensure that the following configuration files and directories exist:
    - `docker-compose.yml`
    - `config/` (includes Kafka broker configs, Prometheus, Kafka UI config, etc.)
    - `data_logs/` directories for Graylog, Kafka, Prometheus, and Grafana

3. **Start the cluster**:
   ```bash
   docker-compose up -d
   ```
   This command will pull required images, build custom images (for Kafka, Kafka Connect, and Kafka UI), and start all services in detached mode. 🐳

4. **Monitor Logs**:  
   Use `docker-compose logs -f` to follow logs in real time and ensure that all services are running smoothly.

---

## Accessing the Services 🔗

Below are the links and default credentials to access each service:

- **Graylog**
    - **URL:** [http://localhost:9000](http://localhost:9000)
    - **Username:** `admin`
    - **Password:** `admin`
    - *Tip:* Change the default password for production! 🔒

- **Kafka UI**
    - **URL:** [http://localhost:8080](http://localhost:8080)

- **Kafka Connect**
    - **URL:** [http://localhost:8083](http://localhost:8083)

- **Prometheus**
    - **URL:** [http://localhost:9090](http://localhost:9090)

- **Grafana**
    - **URL:** [http://localhost:3000](http://localhost:3000)
    - **Username:** `admin`
    - **Password:** `admin`

- **Schema Registry** (if deployed)
    - **URL:** [http://localhost:8081](http://localhost:8081)
    - *Test:* `curl http://localhost:8081/subjects`

- **Elasticsearch** (if deployed)
    - **URL:** [http://localhost:9200](http://localhost:9200)
    - *Test:* `curl http://localhost:9200`

---

## Network Ports Table 🌐

| **Service**         | **Port** | **Access URL**                                 | **Description**                             |
|---------------------|----------|------------------------------------------------|---------------------------------------------|
| **Graylog**         | 9000     | [http://localhost:9000](http://localhost:9000) | Web interface & REST API for log management |
| **Kafka UI**        | 8080     | [http://localhost:8080](http://localhost:8080) | Monitor your Kafka cluster visually         |
| **Kafka Connect**   | 8083     | [http://localhost:8083](http://localhost:8083) | REST API for connector management           |
| **Prometheus**      | 9090     | [http://localhost:9090](http://localhost:9090) | Metrics scraping and monitoring             |
| **Grafana**         | 3000     | [http://localhost:3000](http://localhost:3000) | Visualization dashboards for your metrics   |
| **Schema Registry** | 8081     | [http://localhost:8081](http://localhost:8081) | Manage your Avro schemas (if enabled)       |
| **Elasticsearch**   | 9200     | [http://localhost:9200](http://localhost:9200) | Search & analytics engine (if enabled)      |

> **Note:** Kafka brokers are running in KRaft mode and are accessible internally via their advertised listeners. External access is provided via mapped Docker ports (e.g., 19092, 29092, 39092) as configured in the broker properties.

---

## Kafka Broker Configurations 📜

Below are the sample configuration files for the three Kafka brokers running in KRaft mode. They are located under your `config/kafka{1,2,3}/` directories.

### **Broker 1 (node.id=1)**
```properties
# Licensed to the Apache Software Foundation (ASF)...
process.roles=broker,controller
node.id=1
controller.quorum.voters=1@kafka1:9093,2@kafka2:9093,3@kafka3:9093

listeners=PLAINTEXT://0.0.0.0:9192,CONTROLLER://0.0.0.0:9093,LISTENER_DOCKER_EXTERNAL://0.0.0.0:19092
inter.broker.listener.name=PLAINTEXT
advertised.listeners=PLAINTEXT://kafka1:9192,LISTENER_DOCKER_EXTERNAL://localhost:19092
listener.security.protocol.map=CONTROLLER:PLAINTEXT,PLAINTEXT:PLAINTEXT,LISTENER_DOCKER_EXTERNAL:PLAINTEXT

num.network.threads=3
num.io.threads=8
socket.send.buffer.bytes=102400
socket.receive.buffer.bytes=102400
socket.request.max.bytes=104857600

log.dirs=/data/kafka
num.partitions=3
num.recovery.threads.per.data.dir=1

offsets.topic.replication.factor=1
transaction.state.log.replication.factor=1
transaction.state.log.min.isr=1
leader.imbalance.check.interval.seconds=300

log.retention.hours=168
log.segment.bytes=1073741824
log.retention.check.interval.ms=300000

controlled.shutdown.enable=true
delete.topic.enable=true
```

### **Broker 2 (node.id=2)**
```properties
process.roles=broker,controller
node.id=2
controller.quorum.voters=1@kafka1:9093,2@kafka2:9093,3@kafka3:9093

listeners=PLAINTEXT://0.0.0.0:9192,CONTROLLER://0.0.0.0:9093,LISTENER_DOCKER_EXTERNAL://0.0.0.0:29092
inter.broker.listener.name=PLAINTEXT
advertised.listeners=PLAINTEXT://kafka2:9192,LISTENER_DOCKER_EXTERNAL://localhost:29092
controller.listener.names=CONTROLLER
listener.security.protocol.map=CONTROLLER:PLAINTEXT,PLAINTEXT:PLAINTEXT,LISTENER_DOCKER_EXTERNAL:PLAINTEXT

num.network.threads=3
num.io.threads=8
socket.send.buffer.bytes=102400
socket.receive.buffer.bytes=102400
socket.request.max.bytes=104857600

log.dirs=/data/kafka
num.partitions=3
num.recovery.threads.per.data.dir=1

offsets.topic.replication.factor=1
transaction.state.log.replication.factor=1
transaction.state.log.min.isr=1
leader.imbalance.check.interval.seconds=300

log.retention.hours=168
log.segment.bytes=1073741824
log.retention.check.interval.ms=300000

controlled.shutdown.enable=true
delete.topic.enable=true
```

### **Broker 3 (node.id=3)**
```properties
process.roles=broker,controller
node.id=3
controller.quorum.voters=1@kafka1:9093,2@kafka2:9093,3@kafka3:9093

listeners=PLAINTEXT://0.0.0.0:9192,CONTROLLER://0.0.0.0:9093,LISTENER_DOCKER_EXTERNAL://0.0.0.0:39092
inter.broker.listener.name=PLAINTEXT
advertised.listeners=PLAINTEXT://kafka3:9192,LISTENER_DOCKER_EXTERNAL://localhost:39092
controller.listener.names=CONTROLLER
listener.security.protocol.map=EXTERNAL:PLAINTEXT,CONTROLLER:PLAINTEXT,PLAINTEXT:PLAINTEXT,SSL:SSL,SASL_PLAINTEXT:SASL_PLAINTEXT,SASL_SSL:SASL_SSL,LISTENER_DOCKER_EXTERNAL:PLAINTEXT

num.network.threads=3
num.io.threads=8
socket.send.buffer.bytes=102400
socket.receive.buffer.bytes=102400
socket.request.max.bytes=104857600

log.dirs=/data/kafka
num.partitions=3
num.recovery.threads.per.data.dir=1

offsets.topic.replication.factor=1
transaction.state.log.replication.factor=1
transaction.state.log.min.isr=1
leader.imbalance.check.interval.seconds=300

log.retention.hours=168
log.segment.bytes=1073741824
log.retention.check.interval.ms=300000

controlled.shutdown.enable=true
delete.topic.enable=true
```

> **Tip:** Adjust these configurations as needed for your production environment.

---

## Dockerfiles Overview 🐳

Your build context includes custom Dockerfiles for key services:

### **Kafka Broker Dockerfile (`container_images/kafka/Dockerfile`)**
```dockerfile
FROM openjdk:17-slim-bullseye

RUN apt-get update && \
    apt-get install -y curl wget && \
    rm -rf /var/lib/apt/lists/*

RUN mkdir /opt/kafka && \
    mkdir -p /opt/jmx_exporter && \
    mkdir /kafka && \
    mkdir -p /data/kafka

RUN curl "https://archive.apache.org/dist/kafka/3.8.0/kafka_2.13-3.8.0.tgz" \
    -o /opt/kafka/kafka.tgz && \
    cd /kafka && \
    tar -xvzf /opt/kafka/kafka.tgz --strip 1 && \
    rm /opt/kafka/kafka.tgz

RUN wget https://repo1.maven.org/maven2/io/prometheus/jmx/jmx_prometheus_javaagent/0.19.0/jmx_prometheus_javaagent-0.19.0.jar \
    -O /opt/jmx_exporter/jmx_prometheus_javaagent.jar

COPY start-kafka.sh /usr/bin
RUN chmod +x /usr/bin/start-kafka.sh

WORKDIR /kafka/bin
CMD ["start-kafka.sh"]
```

### **Kafka Connect Dockerfile (`container_images/kafka-connect/Dockerfile`)**
```dockerfile
FROM confluentinc/cp-kafka-connect:latest

# Install the SpoolDir connector from Confluent Hub
RUN confluent-hub install --no-prompt jcustenborder/kafka-connect-spooldir:latest
```

### **Kafka UI (Provectus) Dockerfile (`container_images/provectus/Dockerfile`)**
```dockerfile
FROM openjdk:17-jdk-slim

RUN apt update && apt install -y wget
WORKDIR /mnt

RUN wget https://github.com/provectus/kafka-ui/releases/download/v0.7.2/kafka-ui-api-v0.7.2.jar
COPY start-kafka-ui.sh /mnt
RUN chmod +x /mnt/start-kafka-ui.sh 

CMD ["/mnt/start-kafka-ui.sh"]
```

---

## Troubleshooting Tips 🛠

- **Service not starting?**  
  Check the logs:
  ```bash
  docker-compose logs -f <service-name>
  ```
- **Port conflicts?**  
  Verify that no other services are using the required ports.
- **Data persistence issues?**  
  Ensure that the mapped volume directories (under `./data_logs/`) exist and have the proper permissions.

---

## Final Notes 🎉

- **Enjoy your Kafka cluster!** Use this setup to experiment, monitor, and scale your Kafka applications.
- **Remember to secure your deployments** for production by updating default credentials and refining configurations.

Happy clustering! 🚀
