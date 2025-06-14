using System.Data;
using System.Text;

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
        var parts = name.Split('.');
        for (int i = 0; i < parts.Length; i++)
            parts[i] = QuoteIdentifier(parts[i]);

        return string.Join(".", parts);
    }

    /// <summary>
    /// Quotes a SQL identifier with square brackets, escaping any closing brackets.
    /// </summary>
    /// <param name="name">The identifier to quote.</param>
    /// <returns>The quoted identifier.</returns>
    public static string QuoteIdentifier(string name)
    {
        if (name.StartsWith("[") && name.EndsWith("]"))
            return name;

        return "[" + name.Replace("]", "]]") + "]";
    }

    /// <summary>
    /// Removes square bracket quoting from a SQL identifier, if present.
    /// </summary>
    /// <param name="name">The quoted identifier.</param>
    /// <returns>The unquoted identifier.</returns>
    public static string ParseIdentifier(string name)
    {
        if (name.StartsWith("[") && name.EndsWith("]"))
            return name.Substring(1, name.Length - 2);

        return name;
    }

    /// <summary>
    /// Builds the SQL statement for creating the temporary table used in the merge operation.
    /// </summary>
    /// <param name="mergeDefinition">The <see cref="DataMergeDefinition"/> describing the temporary table schema.</param>
    /// <returns>The SQL statement to create the temporary table.</returns>
    public static string BuildTable(DataMergeDefinition mergeDefinition)
    {
        var builder = StringBuilderCache.Acquire();
        builder
            .Append("CREATE TABLE ")
            .Append(TableIdentifier(mergeDefinition.TemporaryTable))
            .AppendLine()
            .Append("(")
            .AppendLine();

        bool hasColumn = false;
        foreach (var mergeColumn in mergeDefinition.Columns.Where(c => !c.IsIgnored))
        {
            bool writeComma = hasColumn;

            builder
                .AppendLineIf(",", v => writeComma)
                .Append(' ', TabSize)
                .Append(QuoteIdentifier(mergeColumn.SourceColumn))
                .Append(" ")
                .Append(mergeColumn.NativeType)
                .Append(" NULL");

            hasColumn = true;
        }

        builder
            .AppendLine()
            .Append(")")
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
    public static string BuildMerge(DataMergeDefinition mergeDefinition, IDataReader reader)
    {
        var mergeColumns = mergeDefinition.Columns
            .Where(c => !c.IsIgnored && (c.IsKey || c.CanInsert || c.CanUpdate))
            .ToList();

        var builder = StringBuilderCache.Acquire();

        if (mergeDefinition.IdentityInsert && mergeDefinition.IncludeInsert)
        {
            builder
                .Append("SET IDENTITY_INSERT ")
                .Append(TableIdentifier(mergeDefinition.TargetTable))
                .AppendLine(" ON;")
                .AppendLine();
        }

        builder
            .Append("MERGE INTO ")
            .Append(TableIdentifier(mergeDefinition.TargetTable))
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
        builder.Append(";");

        if (mergeDefinition.IdentityInsert && mergeDefinition.IncludeInsert)
        {
            builder
                .Append("SET IDENTITY_INSERT ")
                .Append(TableIdentifier(mergeDefinition.TargetTable))
                .AppendLine(" OFF;")
                .AppendLine();
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
        builder
            .AppendLine("USING")
            .AppendLine("(")
            .Append(' ', TabSize)
            .AppendLine("VALUES");

        var fields = new HashSet<string>();

        bool wroteRow = false;
        while (reader.Read())
        {
            bool wrote = false;

            builder
                .AppendLineIf(", ", s => wroteRow)
                .Append(' ', TabSize)
                .Append("(");

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);

                var isFound = mergeColumns.Any(c => c.SourceColumn == fieldName);
                if (!isFound)
                    continue;

                fields.Add(fieldName);

                builder.AppendIf(", ", v => wrote);

                var value = reader.GetValue(i);
                var stringValue = GetValue(value);
                var fieldType = reader.GetFieldType(i);

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (value != null && value != DBNull.Value && NeedQuote(fieldType))
                    builder.AppendFormat("'{0}'", stringValue.Replace("'", "''"));
                else
                    builder.Append(stringValue);

                wrote = true;
            }
            builder.Append(")");

            wroteRow = true;
        }

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
            bool writeComma = hasColumn;

            builder
                .AppendLineIf(",", v => writeComma)
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
        bool hasColumn;
        builder
            .AppendLine("ON")
            .AppendLine("(");

        hasColumn = false;
        foreach (var mergeColumn in mergeColumns.Where(c => c.IsKey))
        {
            bool writeComma = hasColumn;
            builder
                .AppendLineIf(" AND ", v => writeComma)
                .Append(' ', TabSize)
                .Append("t.")
                .Append(QuoteIdentifier(mergeColumn.TargetColumn))
                .Append(" = s.")
                .Append(QuoteIdentifier(mergeColumn.SourceColumn));

            hasColumn = true;
        }

        builder
            .AppendLine()
            .Append(")")
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
                .Append(QuoteIdentifier(mergeColumn.SourceColumn))
                .Append(" as [")
                .Append(OriginalPrefix)
                .Append(ParseIdentifier(mergeColumn.SourceColumn))
                .Append("],")
                .AppendLine();

            builder
                .Append(' ', TabSize)
                .Append("INSERTED.")
                .Append(QuoteIdentifier(mergeColumn.SourceColumn))
                .Append(" as [")
                .Append(CurrentPrefix)
                .Append(ParseIdentifier(mergeColumn.SourceColumn))
                .Append("]");
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

        builder
            .AppendLine("WHEN MATCHED THEN ")
            .Append(' ', TabSize)
            .AppendLine("UPDATE SET");

        bool hasColumn = false;
        foreach (var mergeColumn in mergeColumns)
        {
            bool writeComma = hasColumn;
            builder
                .AppendLineIf(",", v => writeComma)
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

        builder
            .AppendLine("WHEN NOT MATCHED BY TARGET THEN ")
            .Append(' ', TabSize)
            .AppendLine("INSERT")
            .Append(' ', TabSize)
            .AppendLine("(");

        bool hasColumn = false;
        foreach (var mergeColumn in mergeColumns)
        {
            bool writeComma = hasColumn;
            builder
                .AppendLineIf(",", v => writeComma)
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
            bool writeComma = hasColumn;
            builder
                .AppendLineIf(",", v => writeComma)
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
        if (underType == typeof(TimeSpan))
            return true;
        if (underType == typeof(DateTime))
            return true;
        if (underType == typeof(DateTimeOffset))
            return true;
        if (underType == typeof(Guid))
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
    /// Converts a value to its SQL literal representation for use in generated statements.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The SQL literal as a string.</returns>
    private static string GetValue(object value)
    {
        if (value == null || value == DBNull.Value)
            return "NULL";

        return value switch
        {
            string stringValue => stringValue,
            DateTime dateTimeValue => dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz"),
            byte[] byteArray => ToHex(byteArray),
            bool boolValue => boolValue ? "1" : "0",
#if NET6_0_OR_GREATER
            DateOnly dateValue => dateValue.ToString("yyyy-MM-dd"),
            TimeOnly timeValue => timeValue.ToString("hh:mm:ss.ffffff"),
#endif
            _ => Convert.ToString(value)
        };
    }

    /// <summary>
    /// Converts a byte array to its hexadecimal string representation for use in SQL statements.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>The hexadecimal string representation.</returns>
    private static string ToHex(byte[] bytes)
    {
#if NET5_0_OR_GREATER
        return Convert.ToHexString(bytes);
#else
        var s = StringBuilderCache.Acquire();
        s.Append("0x");

        foreach (var b in bytes)
            s.Append(b.ToString("x2").ToUpperInvariant());

        return StringBuilderCache.ToString(s);
#endif
    }
}
