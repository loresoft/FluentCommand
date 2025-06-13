using System.Diagnostics;

namespace FluentCommand.Internal;

/// <summary>
/// Provides a thread-safe, lazily-initialized singleton instance for a specified reference type.
/// </summary>
/// <typeparam name="T">
/// The type of the singleton instance. Must be a reference type with a public parameterless constructor.
/// </typeparam>
/// <remarks>
/// This class uses <see cref="Lazy{T}"/> to ensure that the singleton instance is created in a thread-safe manner
/// and only when it is first accessed. The type <typeparamref name="T"/> must have a public parameterless constructor.
/// </remarks>
public static class Singleton<T>
    where T : class, new()
{
    private static readonly Lazy<T> _instance = new();

    /// <summary>
    /// Gets the singleton instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <value>
    /// The lazily-initialized singleton instance of <typeparamref name="T"/>.
    /// </value>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [DebuggerNonUserCode]
    public static T Current => _instance.Value;
}
