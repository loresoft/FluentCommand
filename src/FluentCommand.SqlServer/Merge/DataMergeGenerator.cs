using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using FluentCommand.Extensions;
using FluentCommand.Internal;

namespace FluentCommand.Merge;

/// <summary>
/// Class used to build data merge SQL statements
/// </summary>
public static class DataMergeGenerator
{
    public const string OriginalPrefix = "Original";
    public const string CurrentPrefix = "Current";
    public const int TabSize = 4;

    /// <summary>
    /// Tables the identifier.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public static string TableIdentifier(string name)
    {
        var parts = name.Split('.');
        for (int i = 0; i < parts.Length; i++)
            parts[i] = QuoteIdentifier(parts[i]);

        return string.Join(".", parts);
    }

    public static string QuoteIdentifier(string name)
    {
        if (name.StartsWith("[") && name.EndsWith("]"))
            return name;

        return "[" + name.Replace("]", "]]") + "]";
    }

    public static string ParseIdentifier(string name)
    {
        if (name.StartsWith("[") && name.EndsWith("]"))
            return name.Substring(1, name.Length - 2);

        return name;
    }


    /// <summary>
    /// Builds the SQL for the temporary table used in the merge operation.
    /// </summary>
    /// <param name="mergeDefinition">The merge definition.</param>
    /// <returns></returns>
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
    /// Builds the SQL merge statement for the merge operation.
    /// </summary>
    /// <param name="mergeDefinition">The merge definition.</param>
    /// <returns>The merge sql statement</returns>
    public static string BuildMerge(DataMergeDefinition mergeDefinition)
    {
        return BuildMerge(mergeDefinition, null);
    }

    /// <summary>
    /// Builds the SQL merge statement for the merge operation.
    /// </summary>
    /// <param name="mergeDefinition">The merge definition.</param>
    /// <param name="table">The data table to generate merge statement with.</param>
    /// <returns>The merge sql statement</returns>
    public static string BuildMerge(DataMergeDefinition mergeDefinition, DataTable table)
    {
        var mergeColumns = mergeDefinition.Columns
            .Where(c => !c.IsIgnored)
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

        if (table == null)
            AppendUsingSelect(mergeDefinition, mergeColumns, builder);
        else
            AppendUsingData(mergeDefinition, mergeColumns, table, builder);

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


    private static void AppendUsingData(DataMergeDefinition mergeDefinition, List<DataMergeColumn> mergeColumns, DataTable table, StringBuilder builder)
    {
        builder
            .AppendLine("USING")
            .AppendLine("(")
            .Append(' ', TabSize)
            .AppendLine("VALUES");

        bool wroteRow = false;
        foreach (DataRow row in table.Rows)
        {
            bool wrote = false;

            builder
                .AppendLineIf(", ", s => wroteRow)
                .Append(' ', TabSize)
                .Append("(");

            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                var column = table.Columns[i];

                var isFound = mergeColumns.Any(c => c.SourceColumn == column.ColumnName);
                if (!isFound)
                    continue;

                builder.AppendIf(", ", v => wrote);

                object value = row[i];
                string stringValue = GetValue(value);

                if ((value != null && value != DBNull.Value) && NeedQuote(row.Table.Columns[i].DataType))
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

        for (int i = 0; i < table.Columns.Count; i++)
        {
            var column = table.Columns[i];

            var isFound = mergeColumns.Any(c => c.SourceColumn == column.ColumnName);
            if (!isFound)
                continue;

            if (wroteColumn)
                builder.Append(", ");

            builder.Append(QuoteIdentifier(column.ColumnName));
            wroteColumn = true;
        }

        builder
            .AppendLine()
            .AppendLine(")");
    }

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

    private static void AppendDelete(DataMergeDefinition mergeDefinition, StringBuilder builder)
    {
        if (!mergeDefinition.IncludeDelete)
            return;

        builder
            .AppendLine("WHEN NOT MATCHED BY SOURCE THEN ")
            .Append(' ', TabSize)
            .AppendLine("DELETE");
    }

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


    private static bool NeedQuote(Type type)
    {
        if (type == typeof(string))
            return true;
        if (type == typeof(TimeSpan))
            return true;
        if (type == typeof(DateTime))
            return true;
        if (type == typeof(DateTimeOffset))
            return true;
        if (type == typeof(Guid))
            return true;

        return false;
    }

    private static string GetValue(object value)
    {
        if (value == null || value == DBNull.Value)
            return "NULL";

        Type type = value.GetType();
        if (type == typeof(string))
            return (string)value;
        if (type == typeof(DateTime))
            return ((DateTime)value).ToString("u");
        if (type == typeof(DateTimeOffset))
            return ((DateTimeOffset)value).ToString("u");
        if (type == typeof(byte[]))
            return ToHex((byte[])value);
        if (type == typeof(bool))
            return Convert.ToString(Convert.ToInt32((bool)value));

        return Convert.ToString(value);
    }

    private static string ToHex(byte[] bytes)
    {
        var s = StringBuilderCache.Acquire();
        s.Append("0x");

        foreach (var b in bytes)
            s.Append(b.ToString("x2").ToUpperInvariant());

        return StringBuilderCache.ToString(s);
    }
}
