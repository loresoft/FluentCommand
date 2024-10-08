using System;
using System.Buffers;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Text.Json;

using FluentCommand.Extensions;

using Microsoft.IO;

namespace FluentCommand;

/// <summary>
/// Extension methods for <see cref="IDataCommand"/>
/// </summary>
public static class JsonCommandExtensions
{
    private static readonly RecyclableMemoryStreamManager _memoryStreamManager = new();

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
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));

        using var stream = _memoryStreamManager.GetStream();

        QueryJson(dataCommand, stream, options);

        var bytes = stream.GetReadOnlySequence();

#if NET5_0_OR_GREATER
        return Encoding.UTF8.GetString(bytes);
#else
        return Encoding.UTF8.GetString(bytes.ToArray());
#endif
    }

    /// <summary>
    /// Executes the query and returns a JSON string from data set returned by the query.
    /// </summary>
    /// <param name="dataCommand">The data command.</param>
    /// <param name="stream">The destination for writing JSON text.</param>
    /// <param name="options">The <see cref="JsonWriterOptions" /> options.</param>
    /// <returns>
    /// A JSON string representing the <see cref="IDataReader" /> result of the command.
    /// </returns>
    public static void QueryJson(this IDataCommand dataCommand, Stream stream, JsonWriterOptions options = default)
    {
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        var writer = new Utf8JsonWriter(stream, options);

        writer.WriteStartArray();

        dataCommand.Read(reader => WriteData(reader, writer), CommandBehavior.SequentialAccess | CommandBehavior.SingleResult);

        writer.WriteEndArray();

        writer.Flush();
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
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));

        using var stream = _memoryStreamManager.GetStream();

        await QueryJsonAsync(dataCommand, stream, options, cancellationToken);

        var bytes = stream.GetReadOnlySequence();

#if NET5_0_OR_GREATER
        return Encoding.UTF8.GetString(bytes);
#else
        return Encoding.UTF8.GetString(bytes.ToArray());
#endif

    }

    /// <summary>
    /// Executes the query and returns a JSON string from data set returned by the query asynchronously.
    /// </summary>
    /// <param name="dataCommand">The data command.</param>
    /// <param name="stream">The destination for writing JSON text.</param>
    /// <param name="options">The <see cref="JsonWriterOptions" /> options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A JSON string representing the <see cref="IDataReader" /> result of the command.
    /// </returns>
    public static async Task QueryJsonAsync(this IDataCommand dataCommand, Stream stream, JsonWriterOptions options = default, CancellationToken cancellationToken = default)
    {
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        var writer = new Utf8JsonWriter(stream, options);

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

#if NET6_0_OR_GREATER
        if (type == typeof(DateOnly))
        {
            var value = reader.GetValue<DateOnly>(index);
            var formatted = value.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture);

            writer.WriteStringValue(formatted);
            return;
        }

        if (type == typeof(TimeOnly))
        {
            var value = reader.GetValue<TimeOnly>(index);
            string formatted = value.Second == 0 && value.Millisecond == 0
                ? value.ToString("HH':'mm", CultureInfo.InvariantCulture)
                : value.ToString(CultureInfo.InvariantCulture);

            writer.WriteStringValue(formatted);
            return;
        }
#endif

        if (type == typeof(TimeSpan))
        {
            var value = reader.GetValue<TimeSpan>(index);
            string formatted = value.Seconds == 0 && value.Milliseconds == 0
                ? value.ToString(@"hh\:mm", CultureInfo.InvariantCulture)
                : value.ToString();

            writer.WriteStringValue(formatted);
            return;
        }

        if (type == typeof(DateTime))
        {
            var value = reader.GetDateTime(index);
            var dataType = reader.GetDataTypeName(index).ToLowerInvariant();

            if (string.Equals(dataType, "date", StringComparison.OrdinalIgnoreCase))
            {
                var formattedDate = value.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture);
                writer.WriteStringValue(formattedDate);
            }
            else
            {
                writer.WriteStringValue(value);
            }
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
