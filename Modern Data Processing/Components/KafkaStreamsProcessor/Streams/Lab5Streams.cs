using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.State;
using Streamiz.Kafka.Net.Stream;

namespace KafkaStreamsProcessor.Streams;

public class Lab5Streams : IStreamTopologyBuilder
{
    private const string InputTopic = "int.k-connect.csv.plastic-pollution";

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

        var records = streamBuilder.Stream(InputTopic, new StringSerDes(), new JsonSerDes<PlasticPollutionInfo>())
            .Filter((_, v, _) => v is not null);

        BuildVolunteersCountTopology(records);
        BuildUkraineTotalCountTopology(records);

        return streamBuilder;
    }

    private static void BuildVolunteersCountTopology(IKStream<string, PlasticPollutionInfo> records)
    {
        const int minEventsThreshold = 10;
        var volunteersCountStream = records
            .Filter((_, v, _) => v.NumEvents < minEventsThreshold)
            .Map<string, int>((_, v, _) => KeyValuePair.Create("passed", v.Volunteers))
            .GroupByKey<StringSerDes, Int32SerDes>()
            .WindowedBy(TumblingWindowOptions.Of(TimeSpan.FromSeconds(10)))
            .Aggregate<int, Int32SerDes>(
                () => 0,
                (_, v, agg) => agg + v
            )
            .ToStream();

        volunteersCountStream.Print(Printed<Windowed<string>, int>.ToOut());
    }

    private static void BuildUkraineTotalCountTopology(IKStream<string, PlasticPollutionInfo> records)
    {
        const string ukraineEventsKey = "Ukraine";
        var totalCollectedInUkraine = records.MapValues<long>((_, v, _) => v.NumEvents)
            .Filter((k, _, _) => k is ukraineEventsKey)
            .GroupByKey()
            .WindowedBy(TumblingWindowOptions.Of(TimeSpan.FromSeconds(10)))
            .Aggregate<long, Int64SerDes>(
                () => 0,
                (_, v, agg) => agg + v
            )
            .ToStream();

        totalCollectedInUkraine.Print(Printed<Windowed<string>, long>.ToOut());
    }
}