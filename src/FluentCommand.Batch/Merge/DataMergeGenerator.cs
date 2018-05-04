using System;
using System.Linq;
using System.Text;
using FluentCommand.Extensions;

namespace FluentCommand.Merge
{
    /// <summary>
    /// Class used to build data merge SQL statements
    /// </summary>
    public static class DataMergeGenerator
    {
        public const string OriginalPrefix = "Original";
        public const string CurrentPrefix = "Current";
        public const int TabSize = 4;
        
        /// <summary>
        /// Builds the SQL for the temporary table used in the merge operation.
        /// </summary>
        /// <param name="mergeDefinition">The merge definition.</param>
        /// <returns></returns>
        public static string BuildTable(DataMergeDefinition mergeDefinition)
        {
            var builder = new StringBuilder();
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

            return builder.ToString();
        }

        /// <summary>
        /// Builds the SQL merge statement for the merge operation.
        /// </summary>
        /// <param name="mergeDefinition">The merge definition.</param>
        /// <returns></returns>
        public static string BuildMerge(DataMergeDefinition mergeDefinition)
        {
            var mergeColumns = mergeDefinition.Columns
                .Where(c => !c.IsIgnored)
                .ToList();

            var builder = new StringBuilder();

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
                .AppendLine()
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
                .AppendLine("AS s")
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

            return builder.ToString();
        }


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
                return name.Substring(1, name.Length-2);

            return name;
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
    }
}