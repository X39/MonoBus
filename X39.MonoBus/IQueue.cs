using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using X39.Util.Threading.Tasks;

namespace X39.MonoBus;

public interface IQueue : IAsyncDisposable
{
    void Enqueue<T>(T t) where T : notnull;
    Task<T> Dequeue<T>(CancellationToken cancellationToken) where T : notnull;
}