using Confluent.Kafka;

const string topicName = "int.streaming.plastic.pollution";
KafkaConsumer.Consume(topicName);

internal static class KafkaConsumer
{
    private static readonly ConsumerConfig ConsumerConfig = new()
    {
        BootstrapServers = "PLAINTEXT://localhost:19092,PLAINTEXT://localhost:29092,PLAINTEXT://localhost:39092",
        Acks = Acks.None,
        GroupId = "PlasticPollution-RealTime-EventsLog",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = true
    };

    private static readonly IConsumer<string, string> Consumer =
        new ConsumerBuilder<string, string>(ConsumerConfig).Build();

    public static void Consume(string topic)
    {
        Consumer.Subscribe(topic);

        Console.WriteLine($"🎯 Listening for messages on topic: {topic}...");

        try
        {
            while (true)
            {
                try
                {
                    var consumeResult = Consumer.Consume(CancellationToken.None);
                    var jsonMessage = consumeResult.Message.Value;

                    Console.WriteLine($"✅ Received Message: {jsonMessage}");
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"❌ Kafka Consume Error: {e.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("🛑 Consumer shutting down...");
        }
        finally
        {
            Consumer.Close();
        }
    }
}