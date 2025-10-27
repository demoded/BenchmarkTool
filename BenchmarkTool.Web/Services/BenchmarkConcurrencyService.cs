namespace BenchmarkTool.Web.Services;

public interface IBenchmarkConcurrencyService
{
    /// <summary>
    /// Acquire the single-run lock. If another benchmark is running, this will wait and optionally
    /// report queue status via the provided progress reporter.
    /// Returns an IDisposable that must be disposed to release the lock.
    /// </summary>
    Task<IDisposable> AcquireAsync(IProgress<(string Message, int Percentage)>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Whether a benchmark is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Approximate number of waiters queued (not including the active one).
    /// </summary>
    int WaitingCount { get; }
}

internal sealed class BenchmarkConcurrencyService : IBenchmarkConcurrencyService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private int _waiting;
    private int _isRunning; //0/1

    public bool IsRunning => Volatile.Read(ref _isRunning) == 1;

    public int WaitingCount => Math.Max(0, Volatile.Read(ref _waiting));

    public async Task<IDisposable> AcquireAsync(IProgress<(string Message, int Percentage)>? progress = null, CancellationToken cancellationToken = default)
    {
        // Indicate this caller is entering the queue
        var newWaiting = Interlocked.Increment(ref _waiting);

        // Try fast-path: acquire immediately if available
        if (await _semaphore.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            // We acquired immediately; adjust waiting and mark running
            Interlocked.Decrement(ref _waiting);
            Interlocked.Exchange(ref _isRunning, 1);
            progress?.Report(("Starting benchmark...", 10));
            return new Releaser(this);
        }

        // Someone else is running; report queue status once
        var ahead = Math.Max(0, newWaiting - 1);
        progress?.Report(($"Another benchmark is running. Waiting in queue... ({ahead} ahead)", 5));

        // Wait until the semaphore becomes available
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        // Now we're active
        Interlocked.Decrement(ref _waiting);
        Interlocked.Exchange(ref _isRunning, 1);
        progress?.Report(("Acquired runner slot. Preparing to start...", 10));

        return new Releaser(this);
    }

    private sealed class Releaser : IDisposable
    {
        private BenchmarkConcurrencyService? _owner;

        public Releaser(BenchmarkConcurrencyService owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            var owner = Interlocked.Exchange(ref _owner, null);
            if (owner is null)
                return;

            Interlocked.Exchange(ref owner._isRunning, 0);
            owner._semaphore.Release();
        }
    }
}
