using Streamiz.Kafka.Net;

namespace KafkaStreamsProcessor.Streams;

public interface IStreamTopologyBuilder
{
    public StreamBuilder BuildTopology(StreamBuilder streamBuilder);
}