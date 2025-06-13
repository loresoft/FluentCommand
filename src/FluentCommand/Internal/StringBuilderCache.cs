using System.Text;

namespace FluentCommand.Internal;

/// <summary>
/// Provides a cached, reusable instance of <see cref="StringBuilder"/> per thread to reduce memory allocations.
/// </summary>
/// <remarks>
/// This class is intended to optimize performance by reusing <see cref="StringBuilder"/> instances for short-lived operations.
/// The cache is thread-static, so each thread has its own cached instance.
/// </remarks>
public static class StringBuilderCache
{
    /// <summary>
    /// The maximum size, in characters, for a <see cref="StringBuilder"/> instance to be cached.
    /// </summary>
    /// <remarks>
    /// The value 360 was chosen as a compromise between minimizing memory usage per thread and covering
    /// a large portion of short-lived <see cref="StringBuilder"/> creations, especially during application startup.
    /// </remarks>
    internal const int MaxBuilderSize = 360;

    private const int DefaultCapacity = 16; // == StringBuilder.DefaultCapacity

    [ThreadStatic]
    private static StringBuilder t_cachedInstance;

    /// <summary>
    /// Retrieves a <see cref="StringBuilder"/> instance with the specified capacity.
    /// </summary>
    /// <param name="capacity">
    /// The minimum capacity of the returned <see cref="StringBuilder"/>. Defaults to 16 if not specified.
    /// </param>
    /// <returns>
    /// A <see cref="StringBuilder"/> instance with at least the specified capacity. If a suitable cached instance is available,
    /// it is returned; otherwise, a new instance is created.
    /// </returns>
    /// <remarks>
    /// If the requested capacity exceeds <see cref="MaxBuilderSize"/>, a new <see cref="StringBuilder"/> is always created.
    /// </remarks>
    public static StringBuilder Acquire(int capacity = DefaultCapacity)
    {
        if (capacity > MaxBuilderSize)
            return new StringBuilder(capacity);

        var sb = t_cachedInstance;
        if (sb == null)
            return new StringBuilder(capacity);

        // Avoid StringBuilder block fragmentation by getting a new StringBuilder
        // when the requested size is larger than the current capacity
        if (capacity > sb.Capacity)
            return new StringBuilder(capacity);

        t_cachedInstance = null;
        sb.Clear();

        return sb;
    }

    /// <summary>
    /// Places the specified <see cref="StringBuilder"/> instance in the cache if its capacity does not exceed <see cref="MaxBuilderSize"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance to cache.</param>
    public static void Release(StringBuilder sb)
    {
        if (sb.Capacity <= MaxBuilderSize)
            t_cachedInstance = sb;
    }

    /// <summary>
    /// Converts the specified <see cref="StringBuilder"/> to a string and releases it to the cache.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance to convert and release.</param>
    /// <returns>The string representation of the <see cref="StringBuilder"/> content.</returns>
    public static string ToString(StringBuilder sb)
    {
        var result = sb.ToString();
        Release(sb);
        return result;
    }
}
