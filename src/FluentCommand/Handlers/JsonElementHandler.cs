using System.Data;
using System.Text.Json;

namespace FluentCommand.Handlers;

public class JsonElementHandler : IDataParameterHandler, IDataFieldConverter<JsonElement>
{
    /// <inheritdoc />
    public Type ValueType { get; } = typeof(JsonElement);

    /// <inheritdoc />
    public JsonElement ReadValue(IDataRecord dataRecord, int fieldIndex)
    {
        var value = dataRecord.GetValue(fieldIndex);

        return value switch
        {
            JsonElement jsonElement => jsonElement,
            string stringValue => Parse(stringValue),
            _ => default
        };
    }

    /// <inheritdoc />
    public object? ReadValue(IDbDataParameter parameter)
    {
        return parameter.Value switch
        {
            JsonElement jsonElement => jsonElement,
            string stringValue => Parse(stringValue),
            _ => default
        };
    }

    /// <inheritdoc />
    public void SetValue(IDbDataParameter parameter, object? value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value switch
        {
            JsonElement jsonElement => jsonElement.GetRawText(),
            null => DBNull.Value,
            _ => value.ToString()
        };
    }

    private static JsonElement Parse(string jsonString)
    {
        try
        {
#if NET10_0_OR_GREATER
            return JsonElement.Parse(jsonString);
#else
            using var document = JsonDocument.Parse(jsonString);
            return document.RootElement.Clone();
#endif
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
