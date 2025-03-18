using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;

namespace KafkaStreamsProcessor.Streams;

public class Lab3Streams : IStreamTopologyBuilder
{
    private const string inputTopic = "int.k-connect.csv.plastic-pollution";
    private const string outputTopicBase = "out.k-streams.plastic-pollution.";

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

        var branches = streamBuilder.Stream(inputTopic, new StringSerDes(), new JsonSerDes<PlasticPollutionInfo>())
            .Filter((_, v, _) => v is not null)
            .Branch(
                (_, v, _) => v!.Country is "Ukraine",
                (_, v, _) => v!.NumEvents < 10,
                (_, v, _) => v!.NumEvents is > 10 and < 40,
                (_, v, _) => v!.NumEvents > 50
            );

        var uaEventsStream = branches[0];
        var lessThen10EventsStream = branches[1];
        var from10To40EventsStream = branches[2];
        var moreThan40EventsStream = branches[3];

        uaEventsStream.Print(Printed<string, PlasticPollutionInfo>.ToOut()!);

        lessThen10EventsStream.To(outputTopicBase + "up-to-10");
        from10To40EventsStream.To(outputTopicBase + "10-to-40");
        moreThan40EventsStream.To(outputTopicBase + "40-plus");

        return streamBuilder;
    }
}