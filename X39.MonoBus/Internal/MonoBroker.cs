using X39.Util.Threading;

namespace X39.MonoBus.Internal;

internal sealed class MonoBroker<T> : IBroker where T : notnull
{
    private readonly ReaderWriterLockSlim _readerWriterLock = new();
    private readonly List<IQueue> _queues = new();

    public ValueTask DisposeAsync()
    {
        _readerWriterLock.Dispose();
        return ValueTask.CompletedTask;
    }

    private void Enqueue(T t)
    {
        _readerWriterLock.WriteLocked(() =>
        {
            foreach (var queue in _queues)
            {
                queue.Enqueue(t);
            }
        });
    }

    void IBroker.Enqueue(object obj)
    {
        if (obj is not T t)
            throw new ArgumentException($"Expected object to be of type {typeof(T).FullName}", nameof(obj));
        Enqueue(t);
    }

    public void Add(IQueue queue)
    {
        _readerWriterLock.WriteLocked(() => _queues.Add(queue));
    }

    public async ValueTask RemoveAsync(IQueue queue)
    {
        _readerWriterLock.WriteLocked(() => _queues.Remove(queue));

        await queue.DisposeAsync();
    }

    public int Count => _queues.Count;

    private IEnumerable<IQueue> GetAllQueues()
        => _readerWriterLock.WriteLocked(() => _queues.ToArray());

    public async ValueTask ClearAsync()
    {
        foreach (var queue in GetAllQueues())
        {
            await RemoveAsync(queue);
        }
    }

    public T1? GetQueue<T1>() where T1 : IQueue
        => _readerWriterLock.ReadLocked(() => _queues.OfType<T1>().FirstOrDefault());
}