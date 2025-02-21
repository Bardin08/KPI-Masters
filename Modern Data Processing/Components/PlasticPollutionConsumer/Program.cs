using Confluent.Kafka;

const string kafkaProducerTopic = "int.streaming.plastic.pollution";
const string pgCdcTopic = "int.streaming.pg.public.plastics";

var task1 = Task.Run(() => new KafkaConsumer(kafkaProducerTopic).Consume());
var task2 = Task.Run(() => new KafkaConsumer(pgCdcTopic).Consume());

await Task.WhenAll(task1);

internal sealed class KafkaConsumer
{
    private readonly string _topic;
    private readonly IConsumer<string, string> _consumer;

    public KafkaConsumer(string topic)
    {
        _topic = topic;

        var consumerConfig= new ConsumerConfig
        {
            BootstrapServers = "PLAINTEXT://localhost:19092,PLAINTEXT://localhost:29092,PLAINTEXT://localhost:39092",
            Acks = Acks.Leader,
            GroupId = "PlasticPollution-RealTime-EventsLog-" + topic,
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = true,
            AutoCommitIntervalMs = 1000,
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
    }

    public void Consume()
    {
        _consumer.Subscribe(_topic);

        Console.WriteLine($"🎯 Listening for messages on topic: {_topic}...");

        try
        {
            while (true)
            {
                try
                {
                    var consumeResult = _consumer.Consume(CancellationToken.None);
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
            _consumer.Close();
        }
    }
}