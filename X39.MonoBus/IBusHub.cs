using System.ComponentModel;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace X39.MonoBus;

[PublicAPI]
public interface IBusHub : IAsyncDisposable
{
    public IEnumerable<IBroker> Brokers { get; }
    ValueTask<IProducer<T>> CreateProducerAsync<T, TConfiguration>(
        Action<TConfiguration> configure, CancellationToken cancellationToken)
        where T : notnull
        where TConfiguration : notnull;
    ValueTask<IConsumer<T>> CreateConsumerAsync<T, TConfiguration>(
        Action<TConfiguration> configure, CancellationToken cancellationToken)
        where T : notnull
        where TConfiguration : notnull;
    bool IsAlive { get; }
}

[PublicAPI]
public static class BusHub
{
    public static ValueTask<IProducer<T>> CreateProducerAsync<T, TConfiguration>(
        this IBusHub busHub, Action<TConfiguration> configure)
        where T : notnull
        where TConfiguration : notnull
        => busHub.CreateProducerAsync<T, TConfiguration>(configure, CancellationToken.None);

    public static ValueTask<IConsumer<T>> CreateConsumerAsync<T, TConfiguration>(
        this IBusHub busHub, Action<TConfiguration> configure)
        where T : notnull
        where TConfiguration : notnull
        => busHub.CreateConsumerAsync<T, TConfiguration>(configure, CancellationToken.None);
}