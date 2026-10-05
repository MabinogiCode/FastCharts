using System.Threading;

namespace FastCharts.Core.Tests.DataBinding;

/// <summary>
/// Synchronization context that counts posts and runs them inline (test double for a UI context).
/// </summary>
internal sealed class RecordingSynchronizationContext : SynchronizationContext
{
    private int _postCount;

    public int PostCount => Volatile.Read(ref _postCount);

    public override void Post(SendOrPostCallback d, object? state)
    {
        Interlocked.Increment(ref _postCount);
        d(state);
    }
}
