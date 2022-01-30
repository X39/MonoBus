using JetBrains.Annotations;
using X39.MonoBus.Internal;

namespace X39.MonoBus;

[PublicAPI]
public static class Monolithic
{
    public static IBusHub Hub()
    {
        return new MonoBusHub();
    }
    public static ValueTask<IProducer<T>> CreateMonolithicProducerAsync<T>(
        this IBusHub busHub)
        where T : notnull
        => busHub.CreateProducerAsync<T, MonoProducerConfiguration>((_) => { });
    public static ValueTask<IConsumer<T>> CreateMonolithicConsumerAsync<T>(
        this IBusHub busHub,
        Action<MonoConsumerConfiguration> configure)
        where T : notnull
        => busHub.CreateConsumerAsync<T, MonoConsumerConfiguration>(configure);
}