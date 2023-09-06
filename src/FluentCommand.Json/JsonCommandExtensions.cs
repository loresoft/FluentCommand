using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.Json;

namespace FluentCommand;

/// <summary>
/// Extension methods for <see cref="IDataCommand"/>
/// </summary>
public static class JsonCommandExtensions
{
    /// <summary>
    /// Executes the query and returns a JSON string from data set returned by the query.
    /// </summary>
    /// <param name="dataCommand">The data command.</param>
    /// <param name="options">The <see cref="JsonWriterOptions" /> options.</param>
    /// <returns>
    /// A JSON string representing the <see cref="IDataReader" /> result of the command.
    /// </returns>
    public static string QueryJson(this IDataCommand dataCommand, JsonWriterOptions options = default)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, options);

        writer.WriteStartArray();

        dataCommand.Read(reader => WriteData(reader, writer), CommandBehavior.SequentialAccess);

        writer.WriteEndArray();

        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Executes the query and returns a JSON string from data set returned by the query asynchronously.
    /// </summary>
    /// <param name="dataCommand">The data command.</param>
    /// <param name="options">The <see cref="JsonWriterOptions" /> options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A JSON string representing the <see cref="IDataReader" /> result of the command.
    /// </returns>
    public static async Task<string> QueryJsonAsync(this IDataCommand dataCommand, JsonWriterOptions options = default, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await using var writer = new Utf8JsonWriter(stream, options);

        writer.WriteStartArray();

        await dataCommand.ReadAsync(async (reader, token) =>
        {
            if (reader is DbDataReader dataReader)
                await WriteDataAsync(dataReader, writer, token);
            else
                WriteData(reader, writer);

        }, CommandBehavior.SequentialAccess | CommandBehavior.SingleResult, cancellationToken);

        writer.WriteEndArray();

        await writer.FlushAsync(cancellationToken);

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteData(IDataReader reader, Utf8JsonWriter writer)
    {
        while (reader.Read())
        {
            WriteObject(reader, writer);
        }
    }

    private static async Task WriteDataAsync(DbDataReader reader, Utf8JsonWriter writer, CancellationToken cancellationToken = default)
    {
        while (await reader.ReadAsync(cancellationToken))
        {
            WriteObject(reader, writer);
        }
    }

    private static void WriteObject(IDataReader reader, Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        for (int index = 0; index < reader.FieldCount; index++)
        {
            var name = reader.GetName(index);
            writer.WritePropertyName(name);

            WriteValue(reader, writer, index);
        }

        writer.WriteEndObject();
    }

    private static void WriteValue(IDataReader reader, Utf8JsonWriter writer, int index)
    {
        if (reader.IsDBNull(index))
        {
            writer.WriteNullValue();
            return;
        }

        var type = reader.GetFieldType(index);

        if (type == typeof(string))
        {
            var value = reader.GetString(index);
            writer.WriteStringValue(value);
            return;
        }

        if (type == typeof(bool))
        {
            var value = reader.GetBoolean(index);
            writer.WriteBooleanValue(value);
            return;
        }

        if (type == typeof(byte))
        {
            var value = reader.GetByte(index);
            writer.WriteNumberValue(value);
            return;
        }

        if (type == typeof(short))
        {
            var value = reader.GetInt16(index);
            writer.WriteNumberValue(value);
            return;
        }

        if (type == typeof(int))
        {
            var value = reader.GetInt32(index);
            writer.WriteNumberValue(value);
            return;
        }

        if (type == typeof(long))
        {
            var value = reader.GetInt64(index);
            writer.WriteNumberValue(value);
            return;
        }

        if (type == typeof(float))
        {
            var value = reader.GetFloat(index);
            writer.WriteNumberValue(value);
            return;
        }

        if (type == typeof(double))
        {
            var value = reader.GetDouble(index);
            writer.WriteNumberValue(value);
            return;
        }

        if (type == typeof(decimal))
        {
            var value = reader.GetDecimal(index);
            writer.WriteNumberValue(value);
            return;
        }

        if (type == typeof(TimeSpan))
        {
            var value = reader.GetDateTime(index);
            writer.WriteStringValue(value);
            return;
        }

        if (type == typeof(DateTime))
        {
            var value = reader.GetDateTime(index);
            writer.WriteStringValue(value);
            return;
        }

        if (type == typeof(DateTimeOffset))
        {
            var value = reader.GetValue(index);
            if (value is DateTimeOffset offset)
            {
                writer.WriteStringValue(offset);
                return;
            }

            var date = reader.GetDateTime(index);
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            offset = new DateTimeOffset(date, TimeSpan.Zero);

            writer.WriteStringValue(offset);
            return;
        }

        if (type == typeof(Guid))
        {
            var value = reader.GetGuid(index);
            writer.WriteStringValue(value);
            return;
        }

        // fallback
        var v = reader.GetValue(index);
        writer.WriteStringValue(v.ToString());
    }
}
