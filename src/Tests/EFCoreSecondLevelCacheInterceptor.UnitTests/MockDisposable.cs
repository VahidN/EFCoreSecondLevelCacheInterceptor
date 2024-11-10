namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class MockDisposable : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        Dispose(disposing: true);

        // tell the GC not to finalize
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Not in destructor, OK to reference other objects
            }

            // perform cleanup for this object
        }

        _disposed = true;
    }

    ~MockDisposable() => Dispose(disposing: false);
}