using System.Text;
using System.Text.Json;
using Confluent.Kafka;

var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(10));

const string topicName = "int.streaming.plastic.pollution";

foreach (var ppi in FakePlasticDataGenerator.GenerateRecord(100_000_000))
    await KafkaProducer.Produce(topicName, ppi);

return;


internal static class KafkaProducer
{
    private const string ClientName = "PlasticPollutionProducer@v1";

    private static readonly ProducerConfig ProducerConfig = new()
    {
        BootstrapServers = "PLAINTEXT://localhost:19092,PLAINTEXT://localhost:29092,PLAINTEXT://localhost:39092",
        Acks = Acks.None,
        LingerMs = 0,
        CompressionType = CompressionType.Snappy
    };

    private static readonly IProducer<string, string> Producer =
        new ProducerBuilder<string, string>(ProducerConfig).Build();

    public static async Task Produce(string topic, PlasticPollutionInfo ppi)
    {
        try
        {
            var jsonMessage = JsonSerializer.Serialize(ppi);
            var key = $"{ppi.Country}-{ppi.Year}";

            var headers = new Headers
            {
                new Header("X-Message-Source", Encoding.UTF8.GetBytes(ClientName))
            };

            var message = new Message<string, string>
            {
                Key = key,
                Value = jsonMessage,
                Headers = headers
            };

            var deliveryResult = await Producer.ProduceAsync(topic, message);

            Console.WriteLine($"✅ Delivered to {deliveryResult.TopicPartitionOffset}: {jsonMessage}");
        }
        catch (ProduceException<string, string> e)
        {
            Console.WriteLine($"❌ Kafka error: {e.Error.Reason}");
        }
    }
}

internal static class FakePlasticDataGenerator
{
    private static readonly string[] Countries =
        { "Argentina", "Brazil", "USA", "India", "China", "Germany", "UK", "Australia", "Japan", "Canada" };

    private static readonly string[] Companies =
    {
        "The Coca-Cola Company", "PepsiCo", "Nestle", "Unilever", "Procter & Gamble", "Unbranded", "Grand Total",
        "Danone", "Mars Incorporated", "Mondelez"
    };

    public static IEnumerable<PlasticPollutionInfo> GenerateRecord(int recordCount)
    {
        for (var i = 0; i < recordCount; i++)
        {
            var country = Countries[Random.Shared.Next(Countries.Length)];
            var year = Random.Shared.Next(2015, 2024); // Years between 2015-2023
            var parentCompany = Companies[Random.Shared.Next(Companies.Length)];

            var empty = GetRandomPlasticCount(0, 100);
            var hdpe = GetRandomPlasticCount(50, 500);
            var ldpe = GetRandomPlasticCount(30, 400);
            var other = GetRandomPlasticCount(100, 1000);
            var pet = GetRandomPlasticCount(200, 1500);
            var pp = GetRandomPlasticCount(50, 600);
            var ps = GetRandomPlasticCount(20, 300);
            var pvc = GetRandomPlasticCount(10, 150, allowMissing: true);

            var grandTotal = empty + hdpe + ldpe + other + pet + pp + ps + pvc;

            var numEvents = Random.Shared.Next(1, 10);
            var volunteers = Random.Shared.Next(10, 500);

            yield return new PlasticPollutionInfo(
                country, year, parentCompany, empty,
                hdpe, ldpe, other, pet, pp, ps, pvc,
                grandTotal, numEvents, volunteers);
        }
    }

    private static int GetRandomPlasticCount(int min, int max, bool allowMissing = false)
    {
        if (allowMissing && Random.Shared.NextDouble() < 0.2)
            return -1;

        return Random.Shared.Next(min, max);
    }
}

/// <summary>
/// Represents detailed information about plastic pollution in a specific country and year.
/// </summary>
/// <param name="Country">The name of the country where plastic pollution data was recorded.</param>
/// <param name="Year">The year of data collection.</param>
/// <param name="ParentCompany">The name of the parent company responsible for the plastic waste.</param>
/// <param name="Empty">The amount of empty plastic containers.</param>
/// <param name="Hdpe">The amount of High-Density Polyethylene (HDPE) plastic waste.</param>
/// <param name="Idpe">The amount of Low-Density Polyethylene (LDPE) plastic waste.</param>
/// <param name="O">The amount of other types of plastic waste.</param>
/// <param name="Pet">The amount of Polyethylene Terephthalate (PET) plastic waste.</param>
/// <param name="Pp">The amount of Polypropylene (PP) plastic waste.</param>
/// <param name="Ps">The amount of Polystyrene (PS) plastic waste.</param>
/// <param name="Pvc">The amount of Polyvinyl Chloride (PVC) plastic waste.</param>
/// <param name="GrandTotal">The total count of all plastic waste types collected.</param>
/// <param name="NumEvents">The number of events where data was collected.</param>
/// <param name="Volunteers">The number of volunteers who participated in data collection.</param>
internal sealed record PlasticPollutionInfo(
    string Country,
    int Year,
    string ParentCompany,
    int Empty,
    int Hdpe,
    int Idpe,
    int O,
    int Pet,
    int Pp,
    int Ps,
    int Pvc,
    int GrandTotal,
    int NumEvents,
    int Volunteers
);