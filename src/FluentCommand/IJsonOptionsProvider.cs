using System.Text.Json;

namespace FluentCommand;

/// <summary>
/// Provides JSON serializer options for source-generated JSON column readers.
/// </summary>
public interface IJsonOptionsProvider
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// Gets the JSON serializer options.
    /// </summary>
    static abstract JsonSerializerOptions? Options { get; }
#endif
}
