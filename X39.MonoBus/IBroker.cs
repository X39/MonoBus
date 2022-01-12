namespace X39.MonoBus;

public interface IBroker : IAsyncDisposable
{
    void Enqueue(object obj);
    void Add(IQueue queue);
    ValueTask RemoveAsync(IQueue queue);
    public int Count { get; }
    ValueTask ClearAsync();
    T? GetQueue<T>() where T : IQueue;
}