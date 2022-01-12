using JetBrains.Annotations;

namespace X39.MonoBus;

[PublicAPI]
public interface IConsumer<T> : IAsyncDisposable where T : notnull
{
    ValueTask<IMessage<T>> ConsumeAsync(CancellationToken cancellationToken);
}

[PublicAPI]
public static class Consumer
{
    public static ValueTask<IMessage<T>> ConsumeAsync<T>(this IConsumer<T> consumer) where T : notnull
        => consumer.ConsumeAsync(CancellationToken.None);
}