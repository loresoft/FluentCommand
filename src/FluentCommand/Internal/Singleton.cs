using System.Diagnostics;

namespace FluentCommand.Internal;

/// <summary>
/// A class representing a singleton pattern.
/// </summary>
/// <typeparam name="T">The type of the singleton</typeparam>
public static class Singleton<T>
    where T : class, new()
{
    private static readonly Lazy<T> _instance = new();

    /// <summary>
    /// Gets the current instance of the singleton.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [DebuggerNonUserCode]
    public static T Current => _instance.Value;
}
