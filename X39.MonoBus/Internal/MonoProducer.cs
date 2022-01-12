namespace X39.MonoBus.Internal;

internal class MonoProducer<T> : IProducer<T> where T : notnull
{
    private readonly MonoBusHub _hub;

    public MonoProducer(MonoBusHub busHub)
    {
        _hub = busHub;
    }

    public ValueTask ProduceAsync(T t, CancellationToken cancellationToken)
    {
        _hub.GetBroker<T>().Enqueue(t);
        return default;
    }

    public ValueTask DisposeAsync() => default;
}