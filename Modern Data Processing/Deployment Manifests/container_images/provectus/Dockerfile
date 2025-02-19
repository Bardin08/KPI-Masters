FROM openjdk:17-jdk-slim

RUN apt update && apt install -y wget

WORKDIR /mnt

RUN wget https://github.com/provectus/kafka-ui/releases/download/v0.7.2/kafka-ui-api-v0.7.2.jar

COPY start-kafka-ui.sh /mnt

RUN chmod +x /mnt/start-kafka-ui.sh 

CMD ["/mnt/start-kafka-ui.sh"]