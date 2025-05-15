using System.Text.Json;
using System.Threading.Channels;
using Confluent.Kafka;

var cts = new CancellationTokenSource();
var lagChannel = Channel.CreateUnbounded<ConsumerLagDescriptor>();
var lagExporter = new ConsumerLagExporter(lagChannel.Reader);
_ = Task.Run(() => lagExporter.StartAsync(cts.Token));

const string kafkaProducerTopic = "int.streaming.plastic.pollution";
// const string pgCdcTopic = "int.streaming.pg.public.plastics";

var task1 = Task.Run(() => new KafkaConsumer(kafkaProducerTopic, lagChannel.Writer).Consume());
// var task2 = Task.Run(() => new KafkaConsumer(pgCdcTopic, lagChannel.Writer).Consume());

await Task.WhenAll(task1);

internal sealed class KafkaConsumer
{
    private readonly string _topic;
    private readonly IConsumer<string, string> _consumer;
    private readonly ChannelWriter<ConsumerLagDescriptor> _lagChannelWriter;

    private readonly string _consumerGroup;
    
    public KafkaConsumer(string topic, ChannelWriter<ConsumerLagDescriptor> lagChannelWriter)
    {
        _topic = topic;
        _lagChannelWriter = lagChannelWriter;

        _consumerGroup = "PlasticPollution-RealTime-EventsLog-" + topic;
        var consumerConfig= new ConsumerConfig
        {
            BootstrapServers = "PLAINTEXT://localhost:19092,PLAINTEXT://localhost:29092,PLAINTEXT://localhost:39092",
            Acks = Acks.Leader,
            GroupId = _consumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
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

                    ExportConsumerLag(consumeResult);
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

    private void ExportConsumerLag(ConsumeResult<string, string> consumeResult)
    {
        var watermarkOffsets = _consumer.GetWatermarkOffsets(consumeResult.TopicPartition);
        var committedOffset = consumeResult.Offset;

        var lag = watermarkOffsets.High - committedOffset;
                    
        var lagDescriptor = new ConsumerLagDescriptor(
            _consumerGroup,
            consumeResult.Topic,
            lag
        );

        _lagChannelWriter.TryWrite(lagDescriptor);
    }
}

internal sealed record ConsumerLagDescriptor(string ConsumerGroup, string Topic, long Lag);

internal sealed class ConsumerLagExporter
{
    private readonly ChannelReader<ConsumerLagDescriptor> _channelReader;
    private readonly IProducer<string, string> _producer;
    
    private const int ExportIntervalMs = 100;
    private const string ExportTopic = "int.streaming.plastic.pollution.consumer-lag";
    
    public ConsumerLagExporter(ChannelReader<ConsumerLagDescriptor> channelReader)
    {
        _channelReader = channelReader;
        var producerConfig= new ProducerConfig
        {
            BootstrapServers = "PLAINTEXT://localhost:19092,PLAINTEXT://localhost:29092,PLAINTEXT://localhost:39092",
            Acks = Acks.Leader
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task StartAsync(CancellationToken ct)
    {
        while (await _channelReader.WaitToReadAsync(ct))
        {
            while (_channelReader.TryRead(out var lagDescriptor))
            {
                try
                {
                    var message = new Message<string, string>
                    {
                        Key = $"{lagDescriptor.ConsumerGroup.ToLowerInvariant()}-{lagDescriptor.Topic}",
                        Value = JsonSerializer.Serialize(lagDescriptor)
                    };

                    await _producer.ProduceAsync(ExportTopic, message, ct);
                }
                catch (ProduceException<string, int> e)
                {
                    Console.WriteLine($"❌ Kafka Produce Error: {e.Error.Reason}");
                }
                
                Console.WriteLine($"✅ Exported Lag: {lagDescriptor}");
                await Task.Delay(TimeSpan.FromMilliseconds(ExportIntervalMs), ct);
                Thread.Yield();
            }
        }
    }
}