using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace X39.MonoBus.Tests;

public class MonoConsumerTests
{
    private MonoBus.IBusHub _busHub = null!;
    private CancellationTokenSource _cancellationTokenSource = null!;

    [SetUp]
    public async Task Setup()
    {
        _busHub = Monolithic.Hub();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    [TearDown]
    public async Task TearDown()
    {
        _cancellationTokenSource.Dispose();
        await _busHub.DisposeAsync();
    }

    [Test(Author = "X39")]
    public async Task QueueMode_ListenToAll()
    {
        var arrivedOnA = false;
        var arrivedOnB = false;
        var message = new MockData();
        await using var producer = await _busHub.CreateMonolithicProducerAsync<MockData>();
        await using var consumerA = await _busHub.CreateMonolithicConsumerAsync<MockData>(
            (c) => c.QueueMode = EQueueMode.ListenToAll);
        var consumeA = consumerA.ConsumeAsync(_cancellationTokenSource.Token).AsTask().ContinueWith((t) =>
        {
            Assert.False(t.IsCanceled, "Consumer A was cancelled.");
            Assert.False(t.IsFaulted,  "Consumer A has faulted.");
            Assert.True(t.IsCompleted, "Consumer A did not complete.");
            if (t.IsCompleted)
            {
                arrivedOnA = t.Result.Payload == message;
            }
        });
        await using var consumerB = await _busHub.CreateMonolithicConsumerAsync<MockData>(
            (c) => c.QueueMode = EQueueMode.ListenToAll);
        var consumeB = consumerB.ConsumeAsync(_cancellationTokenSource.Token).AsTask().ContinueWith((t) =>
        {
            Assert.False(t.IsCanceled, "Consumer B was cancelled.");
            Assert.False(t.IsFaulted,  "Consumer B has faulted.");
            Assert.True(t.IsCompleted, "Consumer B did not complete.");
            if (t.IsCompleted)
            {
                arrivedOnB = t.Result.Payload == message;
            }
        });
        await producer.ProduceAsync(message);
        _cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
        await Task.WhenAny(Task.WhenAll(consumeA, consumeB), Task.Delay(TimeSpan.FromSeconds(1)));
        Assert.True(arrivedOnA, "Message did not arrive on consumer A");
        Assert.True(arrivedOnB, "Message did not arrive on consumer A");
    }

    [Test(Author = "X39")]
    public async Task QueueMode_SharedQueue()
    {
        var messageCount = 0;
        var message = new MockData();
        await using var producer = await _busHub.CreateMonolithicProducerAsync<MockData>();
        await using var consumerA = await _busHub.CreateMonolithicConsumerAsync<MockData>(
            (c) => c.QueueMode = EQueueMode.SharedQueue);
        var consumeA = consumerA.ConsumeAsync(_cancellationTokenSource.Token).AsTask().ContinueWith((t) =>
        {
            Assert.False(t.IsCanceled, "Consumer A was cancelled.");
            Assert.False(t.IsFaulted,  "Consumer A has faulted.");
            Assert.True(t.IsCompleted, "Consumer A did not complete.");
            if (t.IsCompleted && t.Result.Payload == message)
            {
                Interlocked.Increment(ref messageCount);
            }
        });
        await using var consumerB = await _busHub.CreateMonolithicConsumerAsync<MockData>(
            (c) => c.QueueMode = EQueueMode.SharedQueue);
        var consumeB = consumerB.ConsumeAsync(_cancellationTokenSource.Token).AsTask().ContinueWith((t) =>
        {
            Assert.False(t.IsCanceled, "Consumer B was cancelled.");
            Assert.False(t.IsFaulted,  "Consumer B has faulted.");
            Assert.True(t.IsCompleted, "Consumer B did not complete.");
            if (t.IsCompleted && t.Result.Payload == message)
            {
                Interlocked.Increment(ref messageCount);
            }
        });
        await producer.ProduceAsync(message);
        await producer.ProduceAsync(message);
        _cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
        await Task.WhenAny(Task.WhenAll(consumeA, consumeB), Task.Delay(TimeSpan.FromSeconds(1)));
        Assert.AreEqual(2, messageCount, "Message count is not matching the sent number of messages.");
    }
}