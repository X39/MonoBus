using JetBrains.Annotations;

namespace X39.MonoBus;

[PublicAPI]
public interface IMessage<T> : IAsyncDisposable where T : notnull
{
    ref readonly T Payload { get; }
    bool AutoCommitOnDispose { get; set; }
    ValueTask ConfirmAsync(CancellationToken cancellationToken);
    ValueTask DenyAsync(CancellationToken cancellationToken);
}
[PublicAPI]
public static class Message
{
    public static ValueTask ConfirmAsync<T>(this IMessage<T> consumer) where T : notnull
        => consumer.ConfirmAsync(CancellationToken.None);

    public static ValueTask DenyAsync<T>(this IMessage<T> consumer) where T : notnull
        => consumer.DenyAsync(CancellationToken.None);
}