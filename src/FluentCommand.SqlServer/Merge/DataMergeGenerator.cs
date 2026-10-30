using System.Data;
using System.Text;
using System.Text.Json;

using FluentCommand.Extensions;
using FluentCommand.Internal;

namespace FluentCommand.Merge;

/// <summary>
/// Provides static methods to generate SQL statements for data merge operations, including table creation and SQL Server MERGE statements.
/// </summary>
public static class DataMergeGenerator
{
    /// <summary>
    /// The prefix used for original (pre-merge) column values in output.
    /// </summary>
    public const string OriginalPrefix = "Original";
    /// <summary>
    /// The prefix used for current (post-merge) column values in output.
    /// </summary>
    public const string CurrentPrefix = "Current";
    /// <summary>
    /// The number of spaces to use for indentation in generated SQL.
    /// </summary>
    public const int TabSize = 4;

    /// <summary>
    /// Formats a table or schema-qualified table name as a valid SQL identifier, quoting each part as needed.
    /// </summary>
    /// <param name="name">The table name, optionally schema-qualified (e.g., <c>dbo.TableName</c>).</param>
    /// <returns>The quoted table identifier (e.g., <c>[dbo].[TableName]</c>).</returns>
    public static string TableIdentifier(string name)
    {
        var parts = new List<string>();
        var current = StringBuilderCache.Acquire();
        bool inBracket = false;

        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (inBracket)
            {
                current.Append(c);
                if (c != ']')
                    continue;

                // escaped closing bracket
                if (i + 1 < name.Length && name[i + 1] == ']')
                {
                    current.Append(']');
                    i++;
                    continue;
                }

                inBracket = false;
            }
            else if (c == '[')
            {
                inBracket = true;
                current.Append(c);
            }
            else if (c == '.')
            {
                parts.Add(QuoteIdentifier(current.ToString()));
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        parts.Add(QuoteIdentifier(StringBuilderCache.ToString(current)));

        return string.Join(".", parts);
    }

    /// <summary>
    /// Quotes a SQL identifier with square brackets, escaping any closing brackets.
    /// </summary>
    /// <param name="name">The identifier to quote.</param>
    /// <returns>The quoted identifier.</returns>
    /// <remarks>
    /// An identifier that is already quoted is returned unchanged only when every closing bracket inside it is escaped as <c>]]</c>;
    /// otherwise the entire value is quoted and escaped.
    /// </remarks>
    public static string QuoteIdentifier(string name)
    {
        if (IsQuotedIdentifier(name))
            return name;

        return "[" + name.Replace("]", "]]") + "]";
    }

    /// <summary>
    /// Removes square bracket quoting from a SQL identifier, if present, and unescapes any escaped closing brackets.
    /// </summary>
    /// <param name="name">The quoted identifier.</param>
    /// <returns>The unquoted identifier.</returns>
    public static string ParseIdentifier(string name)
    {
        if (name.Length >= 2 && name[0] == '[' && name[name.Length - 1] == ']')
            return name.Substring(1, name.Length - 2).Replace("]]", "]");

        return name;
    }

    private static bool IsQuotedIdentifier(string name)
    {
        if (name.Length < 2 || name[0] != '[' || name[name.Length - 1] != ']')
            return false;

        var last = name.Length - 1;
        for (int i = 1; i < last; i++)
        {
            if (name[i] != ']')
                continue;

            // a closing bracket inside the identifier must be escaped as ]]
            if (i + 1 >= last || name[i + 1] != ']')
                return false;

            i++;
        }

        return true;
    }

    /// <summary>
    /// Builds the SQL statement for creating the temporary table used in the merge operation.
    /// </summary>
    /// <param name="mergeDefinition">The <see cref="DataMergeDefinition"/> describing the temporary table schema.</param>
    /// <returns>The SQL statement to create the temporary table.</returns>
    /// <exception cref="InvalidOperationException">The merge definition does not contain any columns that are not ignored.</exception>
    public static string BuildTable(DataMergeDefinition mergeDefinition)
    {
        var tableColumns = mergeDefinition.Columns
            .Where(c => !c.IsIgnored)
            .ToList();

        if (tableColumns.Count == 0)
            throw new InvalidOperationException("The merge definition requires at least one column that is not ignored.");

        var tableName = TableIdentifier(mergeDefinition.TemporaryTable);

        var builder = StringBuilderCache.Acquire();
        builder
            .Append("CREATE TABLE ")
            .Append(tableName)
            .AppendLine()
            .Append('(')
            .AppendLine();

        bool hasColumn = false;
        foreach (var mergeColumn in tableColumns)
        {
            if (hasColumn)
                builder.AppendLine(",");

            builder
                .Append(' ', TabSize)
                .Append(QuoteIdentifier(mergeColumn.SourceColumn))
                .Append(' ')
                .Append(mergeColumn.NativeType)
                .Append(" NULL");

            hasColumn = true;
        }

        builder
            .AppendLine()
            .Append(')')
            .AppendLine();

        return StringBuilderCache.ToString(builder);
    }

    /// <summary>
    /// Builds the SQL Server MERGE statement for the specified merge definition.
    /// </summary>
    /// <param name="mergeDefinition">The <see cref="DataMergeDefinition"/> describing the merge operation.</param>
    /// <returns>The SQL MERGE statement.</returns>
    public static string BuildMerge(DataMergeDefinition mergeDefinition)
    {
        return BuildMerge(mergeDefinition, null);
    }

    /// <summary>
    /// Builds the SQL Server MERGE statement for the specified merge definition, optionally using data from an <see cref="IDataReader"/>.
    /// </summary>
    /// <param name="mergeDefinition">The <see cref="DataMergeDefinition"/> describing the merge operation.</param>
    /// <param name="reader">An optional <see cref="IDataReader"/> to provide source data for the merge statement.</param>
    /// <returns>The SQL MERGE statement.</returns>
    /// <exception cref="InvalidOperationException">
    /// The merge definition has no key column, would produce no insert, update, or delete action,
    /// or the <paramref name="reader"/> is missing a required source column or returns no rows.
    /// </exception>
    public static string BuildMerge(DataMergeDefinition mergeDefinition, IDataReader? reader)
    {
        var mergeColumns = mergeDefinition.Columns
            .Where(c => !c.IsIgnored && (c.IsKey || c.CanInsert || c.CanUpdate))
            .ToList();

        if (!mergeColumns.Any(c => c.IsKey))
            throw new InvalidOperationException("The merge definition requires at least one key column.");

        var hasInsert = mergeDefinition.IncludeInsert && mergeColumns.Any(c => c.CanInsert);
        var hasUpdate = mergeDefinition.IncludeUpdate && mergeColumns.Any(c => c.CanUpdate);

        // a MERGE statement requires at least one WHEN clause
        if (!hasInsert && !hasUpdate && !mergeDefinition.IncludeDelete)
            throw new InvalidOperationException("The merge definition requires at least one insert, update, or delete action with eligible columns.");

        var identityInsert = mergeDefinition.IdentityInsert && hasInsert;
        var targetTable = TableIdentifier(mergeDefinition.TargetTable);

        var builder = StringBuilderCache.Acquire();

        if (identityInsert)
        {
            builder
                .Append("SET IDENTITY_INSERT ")
                .Append(targetTable)
                .AppendLine(" ON;")
                .AppendLine()
                .AppendLine("BEGIN TRY")
                .AppendLine();
        }

        builder
            .Append("MERGE INTO ")
            .Append(targetTable)
            .Append(" AS t")
            .AppendLine();

        if (reader == null)
            AppendUsingSelect(mergeDefinition, mergeColumns, builder);
        else
            AppendUsingData(mergeDefinition, mergeColumns, reader, builder);

        AppendJoin(mergeColumns, builder);

        // Insert
        AppendInsert(mergeDefinition, builder);

        // Update
        AppendUpdate(mergeDefinition, builder);

        // Delete
        AppendDelete(mergeDefinition, builder);

        // Output
        AppendOutput(mergeDefinition, builder);

        // merge must end with ;
        builder.Append(';');

        if (identityInsert)
        {
            // turn identity insert off when the merge fails so the setting does not leak to later commands on the connection
            builder
                .AppendLine()
                .AppendLine()
                .AppendLine("END TRY")
                .AppendLine("BEGIN CATCH")
                .Append(' ', TabSize)
                .Append("SET IDENTITY_INSERT ")
                .Append(targetTable)
                .AppendLine(" OFF;")
                .Append(' ', TabSize)
                .AppendLine("THROW;")
                .AppendLine("END CATCH;")
                .AppendLine()
                .Append("SET IDENTITY_INSERT ")
                .Append(targetTable)
                .AppendLine(" OFF;");
        }

        return StringBuilderCache.ToString(builder);
    }

    /// <summary>
    /// Appends a VALUES-based USING clause to the merge statement using data from an <see cref="IDataReader"/>.
    /// </summary>
    /// <param name="mergeDefinition">The merge definition.</param>
    /// <param name="mergeColumns">The columns to include in the merge.</param>
    /// <param name="reader">The data reader providing source data.</param>
    /// <param name="builder">The <see cref="StringBuilder"/> to append SQL to.</param>
    private static void AppendUsingData(DataMergeDefinition mergeDefinition, List<DataMergeColumn> mergeColumns, IDataReader reader, StringBuilder builder)
    {
        var columnLookup = new Dictionary<string, DataMergeColumn>(StringComparer.OrdinalIgnoreCase);
        foreach (var mergeColumn in mergeColumns)
        {
            var name = ParseIdentifier(mergeColumn.SourceColumn);
            if (!columnLookup.ContainsKey(name))
                columnLookup.Add(name, mergeColumn);
        }

        // source columns are named after the merge column so references to s.[column] also match on case-sensitive collations
        var fields = new List<string>();
        var matched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var included = new bool[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var fieldName = ParseIdentifier(reader.GetName(i));
            if (!columnLookup.TryGetValue(fieldName, out var mergeColumn) || !matched.Add(fieldName))
                continue;

            included[i] = true;
            fields.Add(mergeColumn.SourceColumn);
        }

        var missing = mergeColumns
            .Where(c => c.IsKey
                || (mergeDefinition.IncludeInsert && c.CanInsert)
                || (mergeDefinition.IncludeUpdate && c.CanUpdate))
            .Select(c => ParseIdentifier(c.SourceColumn))
            .Where(n => !matched.Contains(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (missing.Count > 0)
            throw new InvalidOperationException($"The data reader is missing the merge source column(s): {string.Join(", ", missing)}.");

        builder
            .AppendLine("USING")
            .AppendLine("(")
            .Append(' ', TabSize)
            .AppendLine("VALUES");

        bool wroteRow = false;
        while (reader.Read())
        {
            bool wrote = false;

            if (wroteRow)
                builder.AppendLine(", ");

            builder
                .Append(' ', TabSize)
                .Append('(');

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (!included[i])
                    continue;

                if (wrote)
                    builder.Append(", ");

                builder.Append(GetLiteral(reader.GetValue(i)));

                wrote = true;
            }
            builder.Append(')');

            wroteRow = true;
        }

        if (!wroteRow)
            throw new InvalidOperationException("The data reader did not return any rows to merge.");

        builder
            .AppendLine()
            .AppendLine(")")
            .AppendLine("AS s")
            .AppendLine("(")
            .Append(' ', TabSize);

        bool wroteColumn = false;
        foreach (var field in fields)
        {
            if (wroteColumn)
                builder.Append(", ");

            builder.Append(QuoteIdentifier(field));
            wroteColumn = true;
        }

        builder
            .AppendLine()
            .AppendLine(")");
    }

    /// <summary>
    /// Appends a SELECT-based USING clause to the merge statement using the temporary table.
    /// </summary>
    /// <param name="mergeDefinition">The merge definition.</param>
    /// <param name="mergeColumns">The columns to include in the merge.</param>
    /// <param name="builder">The <see cref="StringBuilder"/> to append SQL to.</param>
    private static void AppendUsingSelect(DataMergeDefinition mergeDefinition, List<DataMergeColumn> mergeColumns, StringBuilder builder)
    {
        builder
            .AppendLine("USING")
            .AppendLine("(")
            .Append(' ', TabSize)
            .AppendLine("SELECT");

        bool hasColumn = false;
        foreach (var mergeColumn in mergeColumns)
        {
            if (hasColumn)
                builder.AppendLine(",");

            builder
                .Append(' ', TabSize * 2)
                .Append(QuoteIdentifier(mergeColumn.SourceColumn));

            hasColumn = true;
        }

        builder
            .AppendLine()
            .Append(' ', TabSize)
            .Append("FROM ")
            .Append(TableIdentifier(mergeDefinition.TemporaryTable))
            .AppendLine()
            .AppendLine(")")
            .AppendLine("AS s");
    }

    /// <summary>
    /// Appends the JOIN condition for the MERGE statement based on key columns.
    /// </summary>
    /// <param name="mergeColumns">The columns to use for the join condition.</param>
    /// <param name="builder">The <see cref="StringBuilder"/> to append SQL to.</param>
    private static void AppendJoin(List<DataMergeColumn> mergeColumns, StringBuilder builder)
    {
        builder
            .AppendLine("ON")
            .AppendLine("(");

        bool hasColumn = false;
        foreach (var mergeColumn in mergeColumns.Where(c => c.IsKey))
        {
            if (hasColumn)
                builder.AppendLine(" AND ");

            builder
                .Append(' ', TabSize)
                .Append("t.")
                .Append(QuoteIdentifier(mergeColumn.TargetColumn))
                .Append(" = s.")
                .Append(QuoteIdentifier(mergeColumn.SourceColumn));

            hasColumn = true;
        }

        builder
            .AppendLine()
            .Append(')')
            .AppendLine();
    }

    /// <summary>
    /// Appends the OUTPUT clause to the MERGE statement if output is enabled in the merge definition.
    /// </summary>
    /// <param name="mergeDefinition">The merge definition.</param>
    /// <param name="builder">The <see cref="StringBuilder"/> to append SQL to.</param>
    private static void AppendOutput(DataMergeDefinition mergeDefinition, StringBuilder builder)
    {
        if (!mergeDefinition.IncludeOutput)
            return;

        var mergeColumns = mergeDefinition.Columns
            .Where(c => !c.IsIgnored)
            .ToList();

        builder
            .AppendLine("OUTPUT")
            .Append(' ', TabSize)
            .Append("$action as [Action]");

        foreach (var mergeColumn in mergeColumns)
        {
            builder
                .AppendLine(",")
                .Append(' ', TabSize)
                .Append("DELETED.")
                .Append(QuoteIdentifier(mergeColumn.TargetColumn))
                .Append(" as ")
                .Append(QuoteIdentifier(OriginalPrefix + ParseIdentifier(mergeColumn.SourceColumn)))
                .Append(',')
                .AppendLine();

            builder
                .Append(' ', TabSize)
                .Append("INSERTED.")
                .Append(QuoteIdentifier(mergeColumn.TargetColumn))
                .Append(" as ")
                .Append(QuoteIdentifier(CurrentPrefix + ParseIdentifier(mergeColumn.SourceColumn)));
        }
    }

    /// <summary>
    /// Appends the DELETE clause to the MERGE statement if delete is enabled in the merge definition.
    /// </summary>
    /// <param name="mergeDefinition">The merge definition.</param>
    /// <param name="builder">The <see cref="StringBuilder"/> to append SQL to.</param>
    private static void AppendDelete(DataMergeDefinition mergeDefinition, StringBuilder builder)
    {
        if (!mergeDefinition.IncludeDelete)
            return;

        if (mergeDefinition.SoftDeleteColumn.HasValue())
        {
            var column = QuoteIdentifier(mergeDefinition.SoftDeleteColumn!);
            var value = mergeDefinition.SoftDeleteValue;
            if (value == null || value == DBNull.Value)
                throw new InvalidOperationException("A soft delete value is required when a soft delete column is specified.");

            var literal = GetLiteral(value);

            builder
                .Append("WHEN NOT MATCHED BY SOURCE AND (t.")
                .Append(column)
                .Append(" IS NULL OR t.")
                .Append(column)
                .Append(" <> ")
                .Append(literal)
                .AppendLine(") THEN ")
                .Append(' ', TabSize)
                .Append("UPDATE SET t.")
                .Append(column)
                .Append(" = ")
                .AppendLine(literal);

            return;
        }

        builder
            .AppendLine("WHEN NOT MATCHED BY SOURCE THEN ")
            .Append(' ', TabSize)
            .AppendLine("DELETE");
    }

    /// <summary>
    /// Appends the UPDATE clause to the MERGE statement if update is enabled in the merge definition.
    /// </summary>
    /// <param name="mergeDefinition">The merge definition.</param>
    /// <param name="builder">The <see cref="StringBuilder"/> to append SQL to.</param>
    private static void AppendUpdate(DataMergeDefinition mergeDefinition, StringBuilder builder)
    {
        if (!mergeDefinition.IncludeUpdate)
            return;

        var mergeColumns = mergeDefinition.Columns
            .Where(c => !c.IsIgnored && c.CanUpdate)
            .ToList();

        if (mergeColumns.Count == 0)
            return;

        builder
            .AppendLine("WHEN MATCHED THEN ")
            .Append(' ', TabSize)
            .AppendLine("UPDATE SET");

        bool hasColumn = false;
        foreach (var mergeColumn in mergeColumns)
        {
            if (hasColumn)
                builder.AppendLine(",");

            builder
                .Append(' ', TabSize * 2)
                .Append("t.")
                .Append(QuoteIdentifier(mergeColumn.TargetColumn))
                .Append(" = s.")
                .Append(QuoteIdentifier(mergeColumn.SourceColumn));

            hasColumn = true;
        }
        builder.AppendLine();
    }

    /// <summary>
    /// Appends the INSERT clause to the MERGE statement if insert is enabled in the merge definition.
    /// </summary>
    /// <param name="mergeDefinition">The merge definition.</param>
    /// <param name="builder">The <see cref="StringBuilder"/> to append SQL to.</param>
    private static void AppendInsert(DataMergeDefinition mergeDefinition, StringBuilder builder)
    {
        if (!mergeDefinition.IncludeInsert)
            return;

        var mergeColumns = mergeDefinition.Columns
            .Where(c => !c.IsIgnored && c.CanInsert)
            .ToList();

        if (mergeColumns.Count == 0)
            return;

        builder
            .AppendLine("WHEN NOT MATCHED BY TARGET THEN ")
            .Append(' ', TabSize)
            .AppendLine("INSERT")
            .Append(' ', TabSize)
            .AppendLine("(");

        bool hasColumn = false;
        foreach (var mergeColumn in mergeColumns)
        {
            if (hasColumn)
                builder.AppendLine(",");

            builder
                .Append(' ', TabSize * 2)
                .Append(QuoteIdentifier(mergeColumn.TargetColumn));

            hasColumn = true;
        }
        builder.AppendLine();

        builder
            .Append(' ', TabSize)
            .AppendLine(")")
            .Append(' ', TabSize)
            .AppendLine("VALUES")
            .Append(' ', TabSize)
            .AppendLine("(");

        hasColumn = false;
        foreach (var mergeColumn in mergeColumns)
        {
            if (hasColumn)
                builder.AppendLine(",");

            builder
                .Append(' ', TabSize * 2)
                .Append("s.")
                .Append(QuoteIdentifier(mergeColumn.SourceColumn));

            hasColumn = true;
        }
        builder.AppendLine();

        builder
            .Append(' ', TabSize)
            .AppendLine(")");
    }

    /// <summary>
    /// Determines whether the specified type requires quoting in SQL statements.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type requires quoting; otherwise, <c>false</c>.</returns>
    private static bool NeedQuote(Type type)
    {
        var underType = type.GetUnderlyingType();

        if (underType == typeof(string))
            return true;
        if (underType == typeof(char))
            return true;
        if (underType == typeof(TimeSpan))
            return true;
        if (underType == typeof(DateTime))
            return true;
        if (underType == typeof(DateTimeOffset))
            return true;
        if (underType == typeof(Guid))
            return true;
        if (underType == typeof(JsonElement))
            return true;
#if NET6_0_OR_GREATER
        if (underType == typeof(DateOnly))
            return true;
        if (underType == typeof(TimeOnly))
            return true;
#endif

        return false;
    }

    /// <summary>
    /// Converts a value to a complete SQL literal, including quoting and any type cast required for the value.
    /// </summary>
    /// <param name="value">The value to convert. The runtime type of the value determines quoting, so values from columns declared as <see cref="object"/> are still escaped.</param>
    /// <returns>The SQL literal, such as <c>NULL</c>, <c>123</c>, <c>N'text'</c>, or <c>CAST('2024-01-01 00:00:00.0000000' AS datetime2)</c>.</returns>
    private static string GetLiteral(object? value)
    {
        if (value == null || value == DBNull.Value)
            return "NULL";

        if (value is JsonElement { ValueKind: JsonValueKind.Undefined })
            return "NULL";

        var literal = GetValue(value);
        if (!NeedQuote(value.GetType()))
            return literal;

        // text is written as a unicode literal so characters outside the database code page are preserved
        var prefix = value is string or char or JsonElement ? "N'" : "'";
        literal = prefix + literal.Replace("'", "''") + "'";

        // temporal strings are cast to their native type so SQL Server parses them with ISO 8601 rules,
        // independent of DATEFORMAT and language, and then converts the typed value to the target column.
        // an untyped string is converted directly into the target column type instead, which fails for
        // legacy datetime columns when the string has more than 3 fractional second digits.
        var castType = GetCastType(value);

        return castType == null
            ? literal
            : $"CAST({literal} AS {castType})";
    }

    /// <summary>
    /// Gets the SQL Server type a quoted literal for the specified value should be cast to.
    /// </summary>
    /// <param name="value">The value being converted to a literal.</param>
    /// <returns>The SQL Server type name, or <c>null</c> if the literal does not require a cast.</returns>
    private static string? GetCastType(object value)
    {
        return value switch
        {
            DateTime => "datetime2",
            DateTimeOffset => "datetimeoffset",
            TimeSpan => "time",
#if NET6_0_OR_GREATER
            DateOnly => "date",
            TimeOnly => "time",
#endif
            _ => null
        };
    }

    /// <summary>
    /// Converts a value to its SQL literal representation for use in generated statements.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The SQL literal as a string.</returns>
    private static string GetValue(object? value)
    {
        if (value == null || value == DBNull.Value)
            return "NULL";

        return value switch
        {
            string stringValue => stringValue,
            DateTime dateTimeValue => dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss.fffffff", System.Globalization.CultureInfo.InvariantCulture),
            TimeSpan timeSpanValue => FormatTime(timeSpanValue),
            Enum enumValue => Convert.ToString(Convert.ChangeType(enumValue, Enum.GetUnderlyingType(enumValue.GetType()), System.Globalization.CultureInfo.InvariantCulture), System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            double doubleValue => FormatDouble(doubleValue),
            float floatValue => FormatDouble(floatValue),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss.fffffffzzz", System.Globalization.CultureInfo.InvariantCulture),
            byte[] byteArray => ToHex(byteArray),
            bool boolValue => boolValue ? "1" : "0",
            JsonElement jsonElement => GetJsonValue(jsonElement) ?? "NULL",
#if NET6_0_OR_GREATER
            DateOnly dateValue => dateValue.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            TimeOnly timeValue => timeValue.ToString("HH:mm:ss.fffffff", System.Globalization.CultureInfo.InvariantCulture),
#endif
            IFormattable formattable => formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
        };
    }

    private static string FormatTime(TimeSpan value)
    {
        // TimeSpan literals are cast to SQL Server time, which cannot hold negative values or values of a day or more
        if (value < TimeSpan.Zero || value.Ticks >= TimeSpan.TicksPerDay)
            throw new InvalidOperationException($"The TimeSpan value '{value}' is outside the range of a SQL Server time value (00:00:00 to 23:59:59.9999999).");

        return value.ToString(@"hh\:mm\:ss\.fffffff", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string FormatDouble(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            throw new InvalidOperationException($"The value '{value}' cannot be represented as a SQL literal.");

        return value.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string FormatDouble(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new InvalidOperationException($"The value '{value}' cannot be represented as a SQL literal.");

        return value.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts a JSON element
    /// </summary>
    /// <param name="jsonElement">The JSON element to convert.</param>
    /// <returns>The string representation of the JSON element, or <c>null</c> if undefined.</returns>
    private static string? GetJsonValue(JsonElement jsonElement)
    {
        return jsonElement.ValueKind != JsonValueKind.Undefined
            ? jsonElement.GetRawText()
            : null;
    }

    /// <summary>
    /// Converts a byte array to its hexadecimal string representation for use in SQL statements.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>The hexadecimal string representation.</returns>
    private static string ToHex(byte[] bytes)
    {
#if NET5_0_OR_GREATER
        return "0x" + Convert.ToHexString(bytes);
#else
        var s = StringBuilderCache.Acquire();
        s.Append("0x");

        foreach (var b in bytes)
            s.Append(b.ToString("x2").ToUpperInvariant());

        return StringBuilderCache.ToString(s);
#endif
    }
}
