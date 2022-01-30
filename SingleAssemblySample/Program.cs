using X39.MonoBus;
using X39.Util.Console;

namespace SingleAssemblySample;

public static class Program
{
    public static async Task Main(string[] args)
    {
        await using var cancellationTokenSource = new ConsoleCancellationTokenSource();
        await using var busHub = Monolithic.Hub();

        var stuff = Enumerable.Empty<Task>()
            .Concat(MakeConsumers(busHub, "consumer-{0:00}-{1}", 20, cancellationTokenSource.Token))
            .Concat(MakeProducers(busHub, "producer-{0:00}", 10, cancellationTokenSource.Token))
            .ToArray();
        await Task.WhenAll(stuff);
        await cancellationTokenSource.CancellationPromise;
    }

    private static IEnumerable<Task> MakeConsumers(IBusHub busHub, string logAsFormat, int count, CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            yield return StartConsumingAsync(busHub, string.Format(logAsFormat, i, (EQueueMode) (i % 3)), (EQueueMode) (i % 3), cancellationToken);
        }
    }
    private static IEnumerable<Task> MakeProducers(IBusHub busHub, string logAsFormat, int count, CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            yield return StartProducingAsync(busHub, string.Format(logAsFormat, i), cancellationToken);
        }
    }

    private static async Task StartConsumingAsync(IBusHub busHub, string logAs, EQueueMode queueMode, CancellationToken cancellationToken)
    {
        await using var consumer = await busHub.CreateMonolithicConsumerAsync<SamplePayload>(
            (config) => config.QueueMode = queueMode);
        var count = 0;
        
        // While the bus-hub reports as alive (aka: Not disposed/closed)
        while (busHub.IsAlive)
        {
            // wait for a message
            await using var message = await consumer.ConsumeAsync();
            
            // Disable auto commit for demonstration purpose
            message.AutoCommitOnDispose = false;

            // Deny message all 25 executions
            if (++count % 3 == 0)
            {
                await message.DenyAsync();
                Console.WriteLine($"[{logAs}][ABORT]: {message.Payload.LogMessage}");
                continue;
            }

            // Do something with the message
            {
                Console.WriteLine(
                    $"[{logAs}]{(message.Payload.IsError ? "[ERROR]" : string.Empty)}: {message.Payload.LogMessage}");
                await Task.Delay(500, cancellationToken);
            }
            
            // Confirm message manually.
            // Required because we disabled auto commit on dispose earlier with `message.AutoCommitOnDispose = false`
            await message.ConfirmAsync();
        }
    }

    private static int ProducerMessageCounter;
    private static async Task StartProducingAsync(IBusHub busHub, string logAs, CancellationToken cancellationToken)
    {
        await using var producer = await busHub.CreateMonolithicProducerAsync<SamplePayload>();
        
        // While the bus-hub reports as alive (aka: Not disposed/closed)
        while (busHub.IsAlive)
        {
            var count = Interlocked.Increment(ref ProducerMessageCounter);
            await producer.ProduceAsync(new SamplePayload {IsError = count % 7 == 0, LogMessage = $"Log message {count} of {logAs}."});

            await Task.Delay(250, cancellationToken);
        }
    }
}

public record SamplePayload
{
    public bool IsError { get; init; }
    public string LogMessage { get; init; } = string.Empty;
}