using KafkaStreamsProcessor.Streams;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;

var config = new StreamConfig<StringSerDes, StringSerDes>
{
    ApplicationId = "k-streams.plastic-pollution",
    BootstrapServers = "PLAINTEXT://localhost:19092,PLAINTEXT://localhost:29092,PLAINTEXT://localhost:39092"
};

var kafkaStream = GetKafkaStream(config);
await kafkaStream.StartAsync();
return;

KafkaStream GetKafkaStream(StreamConfig<StringSerDes, StringSerDes> streamConfig)
{
    var topology = GetTopology();

    var kafkaStream1 = new KafkaStream(topology, streamConfig);
    return kafkaStream1;
}

Topology GetTopology()
{
    IStreamTopologyBuilder[] topologyProviders =
    [
        new Lab5ConsumerLagStreams()
    ];

    var topology1 = topologyProviders
        .Aggregate(
            new StreamBuilder(),
            (builder, provider) => provider.BuildTopology(builder))
        .Build();

    return topology1;
}