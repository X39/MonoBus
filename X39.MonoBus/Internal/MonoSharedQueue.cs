using System.Diagnostics.CodeAnalysis;
using X39.Util.Threading.Tasks;

namespace X39.MonoBus.Internal;

internal class MonoRoundRobinQueue : IQueue
{
    private readonly ReaderWriterLockSlim _readerWriterLock = new();
    private readonly Queue<object> _messages = new();
    private readonly Queue<Promise> _promises = new();

    public void Enqueue<T>(T t) where T : notnull
    {
        _readerWriterLock.EnterWriteLock();
        try
        {
            if (_messages.Count == 0)
            {
                _messages.Enqueue(t);
            }
            else
            {
                _messages.Enqueue(t);
            }
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
        CompletePromiseIfAny();
    }

    private void CompletePromiseIfAny()
    {
        Promise promise;
        lock (_promises)
        {
            if (!_promises.Any())
            {
                return;
            }
            promise = _promises.Dequeue();
        }
        promise.CompleteAsync();
    }

    private bool TryGetNextMessage<T>([NotNullWhen(returnValue: true)] out T? value) where T : notnull
    {
        _readerWriterLock.EnterUpgradeableReadLock();
        try
        {
            if (_messages.Count == 0)
            {
                value = default;
                return false;
            }

            _readerWriterLock.EnterWriteLock();
            try
            {
                if (_messages.TryDequeue(out var tmp))
                {
                    value = (T) tmp;
                    return true;
                }
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }
        finally
        {
            _readerWriterLock.ExitUpgradeableReadLock();
        }

        value = default;
        return false;
    }

    public async Task<T> Dequeue<T>(CancellationToken cancellationToken) where T : notnull
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (promise, result) = CreateMessagePromise<T>();
            if (result is not null)
            {
                return result;
            }

            await promise!;
        }
    }

    private (Promise? promise, T? t) CreateMessagePromise<T>() where T : notnull
    {
        if (TryGetNextMessage<T>(out var t))
        {
            return (null, t);
        }
        lock (_promises)
        {
            var promise = new Promise();
            _promises.Enqueue(promise);

            return (promise, t);
        }
    }
    
    

    public async ValueTask DisposeAsync()
    {
        Promise[] promises;
        lock (_promises)
        {
            promises = _promises.ToArray();
        }

        Exception exception;
        try { throw new ObjectDisposedException(nameof(MonoRoundRobinQueue)); }
        catch (Exception ex) { exception = ex; }
        
        foreach (var promise in promises)
        {
            await promise.CompleteAsync(exception);
        }
        _readerWriterLock.Dispose();
    }
}