using System.Diagnostics;

namespace FluentCommand.Extensions;

public static class StopwatchExtensions
{
    private static readonly double _tickFrequency = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;

    public static TimeSpan GetElapsedTime(this long startingTimestamp)
    {
        var endingTimestamp = Stopwatch.GetTimestamp();

        return GetElapsedTime(startingTimestamp, endingTimestamp);
    }

    public static TimeSpan GetElapsedTime(this long startingTimestamp, long endingTimestamp)
    {
#if NET7_0_OR_GREATER
        var duration = Stopwatch.GetElapsedTime(startingTimestamp, endingTimestamp);
#else
        var duration = new TimeSpan((long)((endingTimestamp - startingTimestamp) * _tickFrequency));
#endif
        return duration;
    }
}
