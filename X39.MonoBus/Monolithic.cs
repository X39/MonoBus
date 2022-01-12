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
}