using System.Text.Json;

namespace FluentCommand;

/// <summary>
/// Provides contextual data for generated data reader factories.
/// </summary>
public interface IDataReaderContext
{
    /// <summary>
    /// Gets the JSON serializer options used by generated JSON column readers.
    /// </summary>
    JsonSerializerOptions? JsonSerializerOptions { get; }
}
