using JetBrains.Annotations;

namespace X39.MonoBus;

[PublicAPI]
public interface IProducer<in T> : IAsyncDisposable where T : notnull
{
    ValueTask ProduceAsync(T t, CancellationToken cancellationToken);
}

[PublicAPI]
public static class Producer
{
    public static ValueTask ProduceAsync<T>(this IProducer<T> producer, T t) where T : notnull
        => producer.ProduceAsync(t, CancellationToken.None);
}