using System.Text.Json;
using System.Text.Json.Serialization;

namespace FluentCommand.Converters;

/// <summary>
/// Converts object values while preserving common CLR value types.
/// </summary>
public sealed class ObjectValueJsonConverter : JsonConverter<object?>
{
    /// <inheritdoc />
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadValue(ref reader, options);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

    private static object? ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Number:
                return ReadNumber(ref reader);
            case JsonTokenType.StartArray:
                return ReadArray(ref reader, options);
            case JsonTokenType.StartObject:
                return ReadObject(ref reader);
            default:
                throw new JsonException($"Unexpected token '{reader.TokenType}' while reading query filter value.");
        }
    }

    private static object ReadNumber(ref Utf8JsonReader reader)
    {
        if (reader.TryGetInt32(out var intValue))
            return intValue;

        if (reader.TryGetInt64(out var longValue))
            return longValue;

        if (reader.TryGetDecimal(out var decimalValue))
            return decimalValue;

        return reader.GetDouble();
    }

    private static List<object?> ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var values = new List<object?>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return values;

            var item = ReadValue(ref reader, options);
            values.Add(item);
        }

        throw new JsonException("Unexpected end of JSON while reading query filter value array.");
    }

    private static JsonElement ReadObject(ref Utf8JsonReader reader)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        return document.RootElement.Clone();
    }
}
