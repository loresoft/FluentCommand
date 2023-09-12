using System.Diagnostics;

namespace FluentCommand.Internal;

internal readonly struct SharedStopwatch
{
    private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    private readonly TimeSpan _started;

    private SharedStopwatch(TimeSpan started)
    {
        _started = started;
    }

    public TimeSpan Elapsed => _stopwatch.Elapsed - _started;

    public static SharedStopwatch StartNew()
    {
        // This call to StartNewCore isn't required, but is included to avoid measurement errors
        // which can occur during periods of high allocation activity. In some cases, calls to Stopwatch
        // operations can block at their return point on the completion of a background GC operation. When
        // this occurs, the GC wait time ends up included in the measured time span. In the event the first
        // call to StartNewCore blocked on a GC operation, the second call will most likely occur when the
        // GC is no longer active. In practice, a substantial improvement to the consistency of analyzer
        // timing data was observed.
        //
        // Note that the call to SharedStopwatch.Elapsed is not affected, because the GC wait will occur
        // after the timer has already recorded its stop time.
        _ = StartNewCore();
        return StartNewCore();
    }

    private static SharedStopwatch StartNewCore() => new(_stopwatch.Elapsed);
}
