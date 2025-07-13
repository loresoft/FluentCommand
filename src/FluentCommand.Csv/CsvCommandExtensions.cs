using System.Buffers;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;

using FluentCommand.Extensions;

using Microsoft.IO;

namespace FluentCommand;

/// <summary>
/// Provides extension methods for <see cref="IDataCommand"/> to export query results as CSV data.
/// </summary>
public static class CsvCommandExtensions
{
    private static readonly RecyclableMemoryStreamManager _memoryStreamManager = new();
    private static readonly SearchValues<char> SpecialChars = SearchValues.Create(",\"\n\r");

    /// <summary>
    /// Executes the query and returns a CSV string from the data set returned by the query.
    /// </summary>
    /// <param name="dataCommand">The data command to execute.</param>
    /// <returns>
    /// A CSV string representing the <see cref="IDataReader"/> result of the command.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataCommand"/> is <c>null</c>.</exception>
    public static string QueryCsv(this IDataCommand dataCommand)
    {
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));

        using var stream = _memoryStreamManager.GetStream();

        QueryCsv(dataCommand, stream);

        var bytes = stream.GetReadOnlySequence();

#if NET5_0_OR_GREATER
        return Encoding.UTF8.GetString(bytes);
#else
        return Encoding.UTF8.GetString(bytes.ToArray());
#endif
    }

    /// <summary>
    /// Executes the query and writes the CSV data to the specified <paramref name="stream"/>.
    /// </summary>
    /// <param name="dataCommand">The data command to execute.</param>
    /// <param name="stream">The stream to which the CSV data will be written.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="dataCommand"/> or <paramref name="stream"/> is <c>null</c>.
    /// </exception>
    public static void QueryCsv(this IDataCommand dataCommand, Stream stream)
    {
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        using var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true);

        dataCommand.Read(
            readAction: reader => WriteData(streamWriter, reader),
            commandBehavior: CommandBehavior.SequentialAccess | CommandBehavior.SingleResult);

        streamWriter.Flush();
    }

    /// <summary>
    /// Executes the query and returns a CSV string from the data set returned by the query asynchronously.
    /// </summary>
    /// <param name="dataCommand">The data command to execute.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains a CSV string representing the <see cref="IDataReader"/> result of the command.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataCommand"/> is <c>null</c>.</exception>
    public static async Task<string> QueryCsvAsync(this IDataCommand dataCommand, CancellationToken cancellationToken = default)
    {
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));

        using var stream = _memoryStreamManager.GetStream();

        await QueryCsvAsync(dataCommand, stream, cancellationToken);

        var bytes = stream.GetReadOnlySequence();

#if NET5_0_OR_GREATER
        return Encoding.UTF8.GetString(bytes);
#else
        return Encoding.UTF8.GetString(bytes.ToArray());
#endif
    }

    /// <summary>
    /// Executes the query and writes the CSV data to the specified <paramref name="stream"/> asynchronously.
    /// </summary>
    /// <param name="dataCommand">The data command to execute.</param>
    /// <param name="stream">The stream to which the CSV data will be written.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="dataCommand"/> or <paramref name="stream"/> is <c>null</c>.
    /// </exception>
    public static async Task QueryCsvAsync(this IDataCommand dataCommand, Stream stream, CancellationToken cancellationToken = default)
    {
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        using var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true);

        await dataCommand.ReadAsync(
            readAction: async (reader, token) =>
            {
                if (reader is DbDataReader dataReader)
                    await WriteDataAsync(streamWriter, dataReader, token);
                else
                    WriteData(streamWriter, reader);
            },
            commandBehavior: CommandBehavior.SequentialAccess | CommandBehavior.SingleResult,
            cancellationToken: cancellationToken);

        await streamWriter.FlushAsync(cancellationToken);
    }


    private static void WriteData(TextWriter writer, IDataReader reader)
    {
        var wroteHeader = false;
        Type[] rowTypes = null;

        while (reader.Read())
        {
            if (!wroteHeader)
            {
                WriteHeader(writer, reader);
                wroteHeader = true;
            }

            if (rowTypes is null)
            {
                rowTypes = new Type[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                    rowTypes[i] = reader.GetFieldType(i);
            }

            WriteRow(writer, reader, rowTypes);
        }
    }

    private static async Task WriteDataAsync(TextWriter writer, DbDataReader reader, CancellationToken cancellationToken = default)
    {
        var wroteHeader = false;
        Type[] rowTypes = null;

        while (await reader.ReadAsync(cancellationToken))
        {
            if (!wroteHeader)
            {
                WriteHeader(writer, reader);
                wroteHeader = true;
            }

            if (rowTypes is null)
            {
                rowTypes = new Type[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                    rowTypes[i] = reader.GetFieldType(i);
            }

            WriteRow(writer, reader, rowTypes);
        }
    }

    private static void WriteHeader(TextWriter writer, IDataReader reader)
    {
        for (int index = 0; index < reader.FieldCount; index++)
        {
            if (index > 0)
                writer.Write(',');

            var name = reader.GetName(index);
            WriteValue(writer, name);
        }

        writer.WriteLine();
    }

    private static void WriteRow(TextWriter writer, IDataReader reader, Type[] rowTypes)
    {
        for (int index = 0; index < reader.FieldCount; index++)
        {
            if (index > 0)
                writer.Write(',');

            WriteValue(writer, reader, index, rowTypes);
        }
        writer.WriteLine();
    }

    private static void WriteValue(TextWriter writer, IDataReader reader, int index, Type[] rowTypes)
    {
        if (reader.IsDBNull(index))
        {
            writer.Write(string.Empty);
            return;
        }

        var type = rowTypes[index];
        if (type == typeof(string))
        {
            var value = reader.GetString(index);
            WriteValue(writer, value);
            return;
        }
        if (type == typeof(char))
        {
            var value = reader.GetChar(index);
            writer.Write(value);
            return;
        }

        if (type == typeof(char[]))
        {
            var value = reader.GetString(index);
            WriteValue(writer, value);
            return;
        }

        if (type == typeof(bool))
        {
            var value = reader.GetBoolean(index);
            writer.Write(value);
            return;
        }

        if (type == typeof(byte))
        {
            var value = reader.GetByte(index);
            writer.Write(value);
            return;
        }

        if (type == typeof(short))
        {
            var value = reader.GetInt16(index);
            writer.Write(value);
            return;
        }

        if (type == typeof(int))
        {
            var value = reader.GetInt32(index);
            writer.Write(value);
            return;
        }

        if (type == typeof(long))
        {
            var value = reader.GetInt64(index);
            writer.Write(value);
            return;
        }

        if (type == typeof(float))
        {
            var value = reader.GetFloat(index);
            writer.Write(value);
            return;
        }

        if (type == typeof(double))
        {
            var value = reader.GetDouble(index);
            writer.Write(value);
            return;
        }

        if (type == typeof(decimal))
        {
            var value = reader.GetDecimal(index);
            writer.Write(value);
            return;
        }

#if NET6_0_OR_GREATER
        if (type == typeof(DateOnly))
        {
            var value = reader.GetValue<DateOnly>(index);
            var formatted = value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            writer.Write(formatted);
            return;
        }

        if (type == typeof(TimeOnly))
        {
            var value = reader.GetValue<TimeOnly>(index);
            var formatted = value.Second == 0 && value.Millisecond == 0
                ? value.ToString("HH:mm", CultureInfo.InvariantCulture)
                : value.ToString("HH:mm:ss.fffffff", CultureInfo.InvariantCulture);

            writer.Write(formatted);
            return;
        }
#endif

        if (type == typeof(TimeSpan))
        {
            var value = reader.GetValue<TimeSpan>(index);
            var formatted = value.Seconds == 0 && value.Milliseconds == 0
                ? value.ToString(@"hh\:mm", CultureInfo.InvariantCulture)
                : value.ToString(@"hh\:mm\:ss\.fffffff", CultureInfo.InvariantCulture);

            writer.Write(formatted);
            return;
        }

        if (type == typeof(DateTime))
        {
            var value = reader.GetDateTime(index);
            var formatted = value.TimeOfDay == TimeSpan.Zero
                ? value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff", CultureInfo.InvariantCulture);

            writer.Write(formatted);
            return;
        }

        if (type == typeof(DateTimeOffset))
        {
            var value = reader.GetValue<DateTimeOffset>(index);
            var formatted = value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffffK", CultureInfo.InvariantCulture);

            writer.Write(formatted);
            return;
        }

        if (type == typeof(Guid))
        {
            var value = reader.GetGuid(index);
            var formatted = value.ToString("D", CultureInfo.InvariantCulture);

            writer.Write(formatted);
            return;
        }

        if (type == typeof(byte[]))
        {
            var value = reader.GetBytes(index);
            var hex = Convert.ToHexString(value);

            writer.Write(hex);
            return;
        }

        // fallback
        var fieldValue = reader.GetValue(index);
        var formattedValue = Convert.ToString(fieldValue, CultureInfo.InvariantCulture);
        WriteValue(writer, formattedValue);
    }

    private static void WriteValue(TextWriter writer, string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        var span = value.AsSpan();
        var needsQuotes = span.ContainsAny(SpecialChars);

        if (!needsQuotes)
        {
            // Fast path: no special chars, write directly
            writer.Write(value);
            return;
        }

        // write with quotes and escape any quotes within the value
        writer.Write('"');
        for (var i = 0; i < value.Length; i++)
        {
            var ch = value[i];

            if (ch == '"')
                writer.Write("\"\"");
            else
                writer.Write(ch);
        }
        writer.Write('"');
    }
}
