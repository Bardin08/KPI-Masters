using KafkaStreamsProcessor;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;

const string inputTopic = "int.k-connect.csv.plastic-pollution";
const string outputTopicBase = "out.k-streams.plastic-pollution.";

JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
    Converters = { new NaIntConverter() },
    ContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new SnakeCaseNamingStrategy(),
    }
};

var config = new StreamConfig<StringSerDes, StringSerDes>
{
    ApplicationId = "k-streams.plastic-pollution",
    BootstrapServers = "PLAINTEXT://localhost:19092,PLAINTEXT://localhost:29092,PLAINTEXT://localhost:39092"
};

var builder = new StreamBuilder();
var branches = builder.Stream(inputTopic, new StringSerDes(), new JsonSerDes<PlasticPollutionInfo>())
    .Filter((_, v, _) => v is not null)
    .Branch(
        (_, v, _) => v!.Country is "Ukraine",
        (_, v, _) => v!.NumEvents < 10,
        (_, v, _) => v!.NumEvents is > 10 and < 40,
        (_, v, _) => v!.NumEvents > 50
    );

// get the total number of the collected garbage in Ukraine
var uaEventsStream = branches[0];
var lessThen10EventsStream = branches[1];
var from10To40EventsStream = branches[2];
var moreThan40EventsStream = branches[3];

uaEventsStream.Print(Printed<string, PlasticPollutionInfo>.ToOut()!);

lessThen10EventsStream.To(outputTopicBase + "up-to-10");
from10To40EventsStream.To(outputTopicBase + "10-to-40");
moreThan40EventsStream.To(outputTopicBase + "40-plus");

var streams = new KafkaStream(builder.Build(), config);
await streams.StartAsync();