namespace X39.MonoBus.Internal;

internal class MonoConsumer<T> : IConsumer<T>
    where T : notnull
{
    private readonly MonoBusHub _hub;
    private readonly IQueue _queue;
    private readonly bool _ownsQueue;

    public MonoConsumer(MonoBusHub busHub, IQueue queue, bool ownsQueue)
    {
        _hub = busHub;
        _queue = queue;
        _ownsQueue = ownsQueue;
    }

    public async ValueTask<IMessage<T>> ConsumeAsync(CancellationToken cancellationToken)
    {
        var t = await _queue.Dequeue<T>(cancellationToken);
        return new MonoMessage<T>(_queue, t);
    }

    public async ValueTask DisposeAsync()
    {
        if (_ownsQueue) await _queue.DisposeAsync();
    }
}