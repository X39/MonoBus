namespace X39.MonoBus.Internal;

internal class MonoMessage<T> : IMessage<T>
    where T : notnull
{
    private readonly IQueue _queue;

    public MonoMessage(IQueue queue, T t)
    {
        _payload = t;
        _queue = queue;
    }

    public ValueTask DisposeAsync()
    {
        if (!_wasProcessed && !AutoCommitOnDispose)
            _queue.Enqueue(_payload);
        return default;
    }

    public ref readonly T Payload => ref _payload;
    private readonly T _payload;
    private bool _wasProcessed;

    public bool AutoCommitOnDispose { get; set; }
    public ValueTask ConfirmAsync(CancellationToken cancellationToken)
    {
        _wasProcessed = true;
        return default;
    }

    public ValueTask DenyAsync(CancellationToken cancellationToken)
    {
        _queue.Enqueue(_payload);
        _wasProcessed = true;
        return default;
    }
}