namespace FluentCommand;

/// <summary>
/// Provides a base implementation of the <see cref="IDisposable"/> pattern, including support for asynchronous disposal on supported platforms.
/// </summary>
public abstract class DisposableBase
    : IDisposable
#if NETCOREAPP3_0_OR_GREATER
    , IAsyncDisposable
#endif
{
    private int _disposeState;

    /// <summary>
    /// Releases all resources used by the current instance of the <see cref="DisposableBase"/> class.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the object and, optionally, releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
    /// </param>
    protected void Dispose(bool disposing)
    {
        // set state to disposing
        if (Interlocked.CompareExchange(ref _disposeState, 1, 0) != 0)
            return;

        if (disposing)
            DisposeManagedResources();

        DisposeUnmanagedResources();

        // set state to disposed
        Interlocked.Exchange(ref _disposeState, 2);
    }

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if this instance has already been disposed.
    /// </summary>
    protected void AssertDisposed()
    {
        if (_disposeState != 0)
            throw new ObjectDisposedException(GetType().FullName);
    }

    /// <summary>
    /// Gets a value indicating whether this instance has been disposed.
    /// </summary>
    protected bool IsDisposed => _disposeState != 0;

    /// <summary>
    /// Releases managed resources. Override this method to dispose managed resources in derived classes.
    /// </summary>
    protected virtual void DisposeManagedResources()
    { }

    /// <summary>
    /// Releases unmanaged resources. Override this method to dispose unmanaged resources in derived classes.
    /// </summary>
    protected virtual void DisposeUnmanagedResources()
    { }

#if NETCOREAPP3_0_OR_GREATER
    /// <summary>
    /// Asynchronously releases all resources used by the current instance of the <see cref="DisposableBase"/> class.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        // set state to disposing
        if (Interlocked.CompareExchange(ref _disposeState, 1, 0) != 0)
            return;

        await DisposeResourcesAsync();

        // set state to disposed
        Interlocked.Exchange(ref _disposeState, 2);

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously releases managed resources. Override this method to asynchronously dispose managed resources in derived classes.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous dispose operation.</returns>
    protected virtual ValueTask DisposeResourcesAsync()
    {
        return ValueTask.CompletedTask;
    }
#endif

    /// <summary>
    /// Finalizes an instance of the <see cref="DisposableBase"/> class.
    /// Releases unmanaged resources and performs other cleanup operations before the object is reclaimed by garbage collection.
    /// </summary>
    ~DisposableBase()
    {
        Dispose(false);
    }
}
