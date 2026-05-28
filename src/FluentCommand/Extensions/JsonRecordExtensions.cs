using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace FluentCommand.Extensions;

/// <summary>
/// Extension methods for deserializing JSON columns from an <see cref="IDataRecord"/>.
/// </summary>
public static class JsonRecordExtensions
{
    /// <summary>Deserializes the JSON value of the specified column to <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The type to deserialize the JSON value into.</typeparam>
    /// <param name="dataRecord">The data record.</param>
    /// <param name="ordinal">The zero-based column ordinal.</param>
    /// <param name="options">Options to control the behavior during parsing.</param>
    /// <returns>The deserialized value, or <see langword="default"/> if the column is <see langword="null"/>.</returns>
    public static T? GetFromJson<T>(this IDataRecord dataRecord, int ordinal, JsonSerializerOptions? options = null)
    {
        if (dataRecord.IsDBNull(ordinal))
            return default;

        var json = dataRecord.GetString(ordinal);
        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// <summary>Deserializes the JSON value of the specified column to <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The type to deserialize the JSON value into.</typeparam>
    /// <param name="dataRecord">The data record.</param>
    /// <param name="ordinal">The zero-based column ordinal.</param>
    /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
    /// <returns>The deserialized value, or <see langword="default"/> if the column is <see langword="null"/>.</returns>
    public static T? GetFromJson<T>(this IDataRecord dataRecord, int ordinal, JsonTypeInfo<T> jsonTypeInfo)
    {
        if (dataRecord.IsDBNull(ordinal))
            return default;

        var json = dataRecord.GetString(ordinal);
        return JsonSerializer.Deserialize(json, jsonTypeInfo);
    }

    /// <summary>Deserializes the JSON value of the specified column to <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The type to deserialize the JSON value into.</typeparam>
    /// <param name="dataRecord">The data record.</param>
    /// <param name="name">The <paramref name="name"/> of the field to find.</param>
    /// <param name="options">Options to control the behavior during parsing.</param>
    /// <returns>The deserialized value, or <see langword="default"/> if the column is <see langword="null"/>.</returns>
    public static T? GetFromJson<T>(this IDataRecord dataRecord, string name, JsonSerializerOptions? options = null)
    {
        int ordinal = dataRecord.GetOrdinal(name);
        return dataRecord.GetFromJson<T>(ordinal, options);
    }

    /// <summary>Deserializes the JSON value of the specified column to <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The type to deserialize the JSON value into.</typeparam>
    /// <param name="dataRecord">The data record.</param>
    /// <param name="name">The <paramref name="name"/> of the field to find.</param>
    /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
    /// <returns>The deserialized value, or <see langword="default"/> if the column is <see langword="null"/>.</returns>
    public static T? GetFromJson<T>(this IDataRecord dataRecord, string name, JsonTypeInfo<T> jsonTypeInfo)
    {
        int ordinal = dataRecord.GetOrdinal(name);
        return dataRecord.GetFromJson<T>(ordinal, jsonTypeInfo);
    }
}
