using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.State;
using Streamiz.Kafka.Net.Table;
using static Streamiz.Kafka.Net.Stream.TumblingWindowOptions;

namespace KafkaStreamsProcessor.Streams;

public class Lab5ConsumerLagStreams : IStreamTopologyBuilder
{
    private const string InputTopic = "int.streaming.plastic.pollution.consumer-lag";

    private static JsonSerializerSettings GetJsonSerializerSettings()
    {
        return new JsonSerializerSettings
        {
            Converters = { new NaIntConverter() },
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy(),
            }
        };
    }

    public StreamBuilder BuildTopology(StreamBuilder streamBuilder)
    {
        JsonConvert.DefaultSettings = GetJsonSerializerSettings;

        var records = streamBuilder.Stream(InputTopic, new StringSerDes(), new JsonSerDes<ConsumerLagDescriptor>())
            .Filter((_, v, _) => v is not null)
            .GroupByKey()
            .WindowedBy(Of(TimeSpan.FromSeconds(10)))
            .Aggregate<ConsumerSumCountDescriptor, ConsumerSumCountDescriptorSerDes>(
                () => new ConsumerSumCountDescriptor(),
                (_, v, agg) => new ConsumerSumCountDescriptor(
                    v.ConsumerGroup + v.Topic,
                    agg.Sum + v.Lag,
                    agg.Count + 1
                )
            )
            .MapValues((r, _) => new ConsumerAverageLagDescriptor(r.Consumer, Math.Floor(r.Sum * 1.0 / r.Count)))
            .Suppress(
                SuppressedBuilder.UntilWindowClose<Windowed<string>, ConsumerAverageLagDescriptor>(
                    TimeSpan.FromMinutes(1),
                    StrictBufferConfig.Unbounded()
                )
                .WithValueSerdes(new ConsumerAverageLagDescriptorSerDes())
            )
            .ToStream();

        records.To(
            "consumer-average-lag",
            new TimeWindowedSerDes<string>(new StringSerDes(), 10_000),
            new JsonSerDes<ConsumerAverageLagDescriptor>()
        );

        return streamBuilder;
    }
}

internal sealed record ConsumerSumCountDescriptor(string Consumer = "", long Sum = 0, int Count = 0);

internal sealed record ConsumerAverageLagDescriptor(string Consumer, double Lag);

internal sealed record ConsumerLagDescriptor(string ConsumerGroup, string Topic, long Lag);

internal sealed class ConsumerSumCountDescriptorSerDes : AbstractSerDes<ConsumerSumCountDescriptor>
{
    public override byte[] Serialize(ConsumerSumCountDescriptor data, SerializationContext context)
    {
        var json = JsonConvert.SerializeObject(data);
        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    public override ConsumerSumCountDescriptor Deserialize(byte[] data, SerializationContext context)
    {
        return JsonConvert.DeserializeObject<ConsumerSumCountDescriptor>(System.Text.Encoding.UTF8.GetString(data))!;
    }
}

internal sealed class ConsumerAverageLagDescriptorSerDes : AbstractSerDes<ConsumerAverageLagDescriptor>
{
    public override byte[] Serialize(ConsumerAverageLagDescriptor data, SerializationContext context)
    {
        var json = JsonConvert.SerializeObject(data);
        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    public override ConsumerAverageLagDescriptor Deserialize(byte[] data, SerializationContext context)
    {
        return JsonConvert.DeserializeObject<ConsumerAverageLagDescriptor>(System.Text.Encoding.UTF8.GetString(data))!;
    }
}