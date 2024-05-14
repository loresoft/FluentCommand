using System.Buffers;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

using FluentCommand.Extensions;

using Microsoft.IO;

namespace FluentCommand;

public static class CsvCommandExtensions
{
    private static readonly RecyclableMemoryStreamManager _memoryStreamManager = new();

    /// <summary>
    /// Executes the query and returns a CSV string from data set returned by the query.
    /// </summary>
    /// <param name="dataCommand">The data command.</param>
    /// <param name="csvConfiguration">The configuration used for the CSV writer</param>
    /// <returns>
    /// A CSV string representing the <see cref="IDataReader" /> result of the command.
    /// </returns>
    public static string QueryCsv(this IDataCommand dataCommand, CsvConfiguration csvConfiguration = default)
    {
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));

        using var stream = _memoryStreamManager.GetStream();

        QueryCsv(dataCommand, stream, csvConfiguration);

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
    /// <param name="dataCommand">The data command.</param>
    /// <param name="stream">The stream writer.</param>
    /// <param name="csvConfiguration">The configuration used for the CSV writer</param>
    public static void QueryCsv(this IDataCommand dataCommand, Stream stream, CsvConfiguration csvConfiguration = default)
    {
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        if (csvConfiguration == null)
            csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

        using var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true);
        using var csvWriter = new CsvWriter(streamWriter, csvConfiguration, true);

        dataCommand.Read(reader => WriteData(reader, csvWriter), CommandBehavior.SequentialAccess | CommandBehavior.SingleResult);

        csvWriter.Flush();
        streamWriter.Flush();
    }


    /// <summary>
    /// Executes the query and returns a CSV string from data set returned by the query asynchronously.
    /// </summary>
    /// <param name="dataCommand">The data command.</param>
    /// <param name="csvConfiguration">The configuration used for the CSV writer</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A CSV string representing the <see cref="IDataReader" /> result of the command.
    /// </returns>
    public static async Task<string> QueryCsvAsync(this IDataCommand dataCommand, CsvConfiguration csvConfiguration = default, CancellationToken cancellationToken = default)
    {
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));

        using var stream = _memoryStreamManager.GetStream();

        await QueryCsvAsync(dataCommand, stream, csvConfiguration, cancellationToken);

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
    /// <param name="dataCommand">The data command.</param>
    /// <param name="stream">The stream writer.</param>
    /// <param name="csvConfiguration">The configuration used for the CSV writer</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A CSV string representing the <see cref="IDataReader" /> result of the command.
    /// </returns>
    public static async Task QueryCsvAsync(this IDataCommand dataCommand, Stream stream, CsvConfiguration csvConfiguration = default, CancellationToken cancellationToken = default)
    {
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        if (csvConfiguration == null)
            csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

        using var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true);
        await using var csvWriter = new CsvWriter(streamWriter, csvConfiguration, true);

        await dataCommand.ReadAsync(async (reader, token) =>
        {
            if (reader is DbDataReader dataReader)
                await WriteDataAsync(dataReader, csvWriter, token);
            else
                WriteData(reader, csvWriter);

        }, CommandBehavior.SequentialAccess | CommandBehavior.SingleResult, cancellationToken);

        await csvWriter.FlushAsync();
        await streamWriter.FlushAsync();
    }


    private static void WriteData(IDataReader reader, CsvWriter writer)
    {
        // if config says to include header, default false
        var wroteHeader = !writer.Configuration.HasHeaderRecord;

        while (reader.Read())
        {
            if (!wroteHeader)
            {
                WriteHeader(reader, writer);
                wroteHeader = true;
            }

            WriteRow(reader, writer);
        }
    }

    private static async Task WriteDataAsync(DbDataReader reader, CsvWriter writer, CancellationToken cancellationToken = default)
    {
        // if config says to include header, default false
        var wroteHeader = !writer.Configuration.HasHeaderRecord;

        while (await reader.ReadAsync(cancellationToken))
        {
            if (!wroteHeader)
            {
                WriteHeader(reader, writer);
                wroteHeader = true;
            }

            WriteRow(reader, writer);
        }
    }

    private static void WriteHeader(IDataReader reader, CsvWriter writer)
    {
        for (int index = 0; index < reader.FieldCount; index++)
        {
            var name = reader.GetName(index);
            writer.WriteField(name);
        }
        writer.NextRecord();
    }

    private static void WriteRow(IDataReader reader, CsvWriter writer)
    {
        for (int index = 0; index < reader.FieldCount; index++)
        {
            WriteValue(reader, writer, index);
        }
        writer.NextRecord();
    }

    private static void WriteValue(IDataReader reader, CsvWriter writer, int index)
    {
        if (reader.IsDBNull(index))
        {
            writer.WriteField(string.Empty);
            return;
        }

        var type = reader.GetFieldType(index);

        if (type == typeof(string))
        {
            var value = reader.GetString(index);
            writer.WriteField(value);
            return;
        }

        if (type == typeof(bool))
        {
            var value = reader.GetBoolean(index);
            writer.WriteField(value);
            return;
        }

        if (type == typeof(byte))
        {
            var value = reader.GetByte(index);
            writer.WriteField(value);
            return;
        }

        if (type == typeof(short))
        {
            var value = reader.GetInt16(index);
            writer.WriteField(value);
            return;
        }

        if (type == typeof(int))
        {
            var value = reader.GetInt32(index);
            writer.WriteField(value);
            return;
        }

        if (type == typeof(long))
        {
            var value = reader.GetInt64(index);
            writer.WriteField(value);
            return;
        }

        if (type == typeof(float))
        {
            var value = reader.GetFloat(index);
            writer.WriteField(value);
            return;
        }

        if (type == typeof(double))
        {
            var value = reader.GetDouble(index);
            writer.WriteField(value);
            return;
        }

        if (type == typeof(decimal))
        {
            var value = reader.GetDecimal(index);
            writer.WriteField(value);
            return;
        }

#if NET6_0_OR_GREATER
        if (type == typeof(DateOnly))
        {
            var value = reader.GetValue<DateOnly>(index);
            var formatted = value.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture);

            writer.WriteField(formatted);
            return;
        }

        if (type == typeof(TimeOnly))
        {
            var value = reader.GetValue<TimeOnly>(index);
            string formatted = value.Second == 0 && value.Millisecond == 0
                ? value.ToString("HH':'mm", CultureInfo.InvariantCulture)
                : value.ToString(CultureInfo.InvariantCulture);

            writer.WriteField(formatted);
            return;
        }
#endif

        if (type == typeof(TimeSpan))
        {
            var value = reader.GetValue<TimeSpan>(index);
            string formatted = value.Seconds == 0 && value.Milliseconds == 0
                ? value.ToString(@"hh\:mm", CultureInfo.InvariantCulture)
                : value.ToString();

            writer.WriteField(formatted);
            return;
        }

        if (type == typeof(DateTime))
        {
            var value = reader.GetDateTime(index);
            var dataType = reader.GetDataTypeName(index).ToLowerInvariant();

            if (string.Equals(dataType, "date", StringComparison.OrdinalIgnoreCase))
            {
                var formattedDate = value.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture);
                writer.WriteField(formattedDate);
            }
            else
            {
                writer.WriteField(value);
            }
            return;
        }

        if (type == typeof(DateTimeOffset))
        {
            var value = reader.GetValue(index);
            if (value is DateTimeOffset offset)
            {
                writer.WriteField(offset);
                return;
            }

            var date = reader.GetDateTime(index);
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            offset = new DateTimeOffset(date, TimeSpan.Zero);

            writer.WriteField(offset);
            return;
        }

        if (type == typeof(Guid))
        {
            var value = reader.GetGuid(index);
            writer.WriteField(value);
            return;
        }

        // fallback
        var v = reader.GetValue(index);
        writer.WriteField(v.ToString());
    }

}
