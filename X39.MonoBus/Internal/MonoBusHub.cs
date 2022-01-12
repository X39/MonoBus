using X39.Util.Threading;

namespace X39.MonoBus.Internal;

internal class MonoBusHub : IBusHub
{
    private readonly ReaderWriterLockSlim _readerWriterLock = new();
    private readonly Dictionary<Type, IBroker> _brokers = new();

    public ValueTask DisposeAsync()
    {
        IsAlive = false;
        return default;
    }

    public IEnumerable<IBroker> Brokers => _readerWriterLock.ReadLocked(() => _brokers.Values.ToArray());

    public ValueTask<IProducer<T>> CreateProducerAsync<T>(CancellationToken cancellationToken) where T : notnull
        => ValueTask.FromResult<IProducer<T>>(new MonoProducer<T>(this));

    public ValueTask<IConsumer<T>> CreateConsumerAsync<T, TConfiguration>(
        Action<TConfiguration> configure,
        CancellationToken cancellationToken)
        where T : notnull
        where TConfiguration : notnull
    {
        var config = new MonoConsumerConfiguration();
        configure((TConfiguration) (object) config);
        var broker = GetBroker<T>();
        var (queue, ownsQueue) = config.QueueMode switch
        {
            EQueueMode.ListenToAll => (CreateQueueForBroker<MonoSingleQueue>(broker), true),
            EQueueMode.SharedQueue => (GetQueueForBroker<MonoRoundRobinQueue>(broker), false),
            EQueueMode.SharedBulkQueue => (GetQueueForBroker<MonoWakeAllQueue>(broker), false),
            _ => throw new ArgumentOutOfRangeException()
        };
        var monoConsumer = new MonoConsumer<T>(this, queue, ownsQueue);
        return ValueTask.FromResult<IConsumer<T>>(monoConsumer);
    }

    private static IQueue GetQueueForBroker<T>(IBroker broker)
        where T : IQueue, new()
    {
        if (broker.GetQueue<T>() is { } queue)
            return queue;
        return CreateQueueForBroker<T>(broker);
    }
    private static IQueue CreateQueueForBroker<T>(IBroker broker)
        where T : IQueue, new()
    {
        var queue = new T();
        broker.Add(queue);
        return queue;
    }

    public bool IsAlive { get; private set; } = true;

    internal IBroker GetBroker<T>() where T : notnull
    {
        return _readerWriterLock.UpgradeableReadLocked(() =>
        {
            if (_brokers.TryGetValue(typeof(T), out var broker))
            {
                return broker;
            }

            return _readerWriterLock.WriteLocked(() =>
            {
                _brokers[typeof(T)] = broker = new MonoBroker<T>();
                return broker;
            });
        });
    }
}