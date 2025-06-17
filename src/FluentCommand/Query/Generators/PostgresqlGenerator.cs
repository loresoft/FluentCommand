using FluentCommand.Extensions;
using FluentCommand.Internal;

namespace FluentCommand.Query.Generators;

/// <summary>
/// Provides a SQL generator for PostgreSQL, implementing SQL statement and expression generation
/// with PostgreSQL-specific syntax and conventions.
/// </summary>
public class PostgreSqlGenerator : SqlServerGenerator
{
    /// <summary>
    /// Builds a SQL INSERT statement for PostgreSQL, including support for RETURNING and comments.
    /// </summary>
    /// <param name="insertStatement">The <see cref="InsertStatement"/> containing the INSERT statement configuration.</param>
    /// <returns>A SQL INSERT statement string for PostgreSQL.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="insertStatement"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if the table or values are not specified.</exception>
    public override string BuildInsert(InsertStatement insertStatement)
    {
        if (insertStatement is null)
            throw new ArgumentNullException(nameof(insertStatement));

        if (insertStatement.TableExpression == null)
            throw new ArgumentException("No table specified to insert into", nameof(insertStatement));

        if (insertStatement.ValueExpressions == null || insertStatement.ValueExpressions.Count == 0)
            throw new ArgumentException("No values specified for insert", nameof(insertStatement));

        var insertBuilder = StringBuilderCache.Acquire();

        if (insertStatement.CommentExpressions?.Count > 0)
        {
            insertBuilder
                .AppendJoin(Environment.NewLine, insertStatement.CommentExpressions)
                .AppendLine();
        }

        var table = TableExpression(insertStatement.TableExpression);
        insertBuilder
            .Append("INSERT INTO ")
            .Append(table);

        if (insertStatement.ColumnExpressions?.Count > 0)
        {
            insertBuilder
                .Append(" (")
                .AppendJoin(", ", insertStatement.ColumnExpressions.Select(ColumnExpression))
                .Append(")");
        }

        insertBuilder
            .AppendLine()
            .Append("VALUES ")
            .Append("(")
            .AppendJoin(", ", insertStatement.ValueExpressions)
            .Append(")");

        if (insertStatement.OutputExpressions?.Count > 0)
        {
            insertBuilder
                .AppendLine()
                .Append("RETURNING ")
                .AppendJoin(", ", insertStatement.OutputExpressions.Select(ColumnExpression));
        }

        insertBuilder.AppendLine(";");

        return StringBuilderCache.ToString(insertBuilder);
    }

    /// <summary>
    /// Builds a SQL UPDATE statement for PostgreSQL, including support for FROM, JOIN, WHERE, RETURNING, and comments.
    /// </summary>
    /// <param name="updateStatement">The <see cref="UpdateStatement"/> containing the UPDATE statement configuration.</param>
    /// <returns>A SQL UPDATE statement string for PostgreSQL.</returns>
    /// <exception cref="ArgumentException">Thrown if the table or update values are not specified.</exception>
    public override string BuildUpdate(UpdateStatement updateStatement)
    {
        if (updateStatement.TableExpression == null)
            throw new ArgumentException("No table specified to update", nameof(updateStatement));

        if (updateStatement.UpdateExpressions == null || updateStatement.UpdateExpressions.Count == 0)
            throw new ArgumentException("No values specified for update", nameof(updateStatement));

        var updateBuilder = StringBuilderCache.Acquire();

        if (updateStatement.CommentExpressions?.Count > 0)
        {
            updateBuilder
                .AppendJoin(Environment.NewLine, updateStatement.CommentExpressions)
                .AppendLine();
        }

        var table = TableExpression(updateStatement.TableExpression);

        updateBuilder
            .Append("UPDATE ")
            .Append(table)
            .AppendLine()
            .Append("SET ")
            .AppendJoin(", ", updateStatement.UpdateExpressions.Select(UpdateExpression));

        if (updateStatement.FromExpressions?.Count > 0)
        {
            updateBuilder
                .AppendLine()
                .Append("FROM ")
                .AppendJoin(", ", updateStatement.FromExpressions.Select(TableExpression));
        }

        if (updateStatement.JoinExpressions?.Count > 0)
        {
            foreach (var joinExpression in updateStatement.JoinExpressions)
            {
                updateBuilder
                    .AppendLine()
                    .Append(JoinExpression(joinExpression));
            }
        }

        if (updateStatement.WhereExpressions?.Count > 0)
        {
            updateBuilder
                .AppendLine()
                .Append("WHERE ")
                .Append("(")
                .AppendJoin(" AND ", updateStatement.WhereExpressions.Select(WhereExpression))
                .Append(")");
        }

        if (updateStatement.OutputExpressions?.Count > 0)
        {
            updateBuilder
                .AppendLine()
                .Append("RETURNING ")
                .AppendJoin(", ", updateStatement.OutputExpressions.Select(ColumnExpression));
        }

        updateBuilder.AppendLine(";");

        return StringBuilderCache.ToString(updateBuilder);
    }

    /// <summary>
    /// Builds a SQL DELETE statement for PostgreSQL, including support for FROM, JOIN, WHERE, RETURNING, and comments.
    /// </summary>
    /// <param name="deleteStatement">The <see cref="DeleteStatement"/> containing the DELETE statement configuration.</param>
    /// <returns>A SQL DELETE statement string for PostgreSQL.</returns>
    /// <exception cref="ArgumentException">Thrown if the table is not specified.</exception>
    public override string BuildDelete(DeleteStatement deleteStatement)
    {
        if (deleteStatement.TableExpression == null)
            throw new ArgumentException("No table specified to delete from", nameof(deleteStatement));

        var deleteBuilder = StringBuilderCache.Acquire();

        if (deleteStatement.CommentExpressions?.Count > 0)
        {
            deleteBuilder
                .AppendJoin(Environment.NewLine, deleteStatement.CommentExpressions)
                .AppendLine();
        }

        var table = TableExpression(deleteStatement.TableExpression);

        deleteBuilder
            .Append("DELETE FROM ")
            .Append(table);

        if (deleteStatement.FromExpressions?.Count > 0)
        {
            deleteBuilder
                .AppendLine()
                .Append("FROM ")
                .AppendJoin(", ", deleteStatement.FromExpressions.Select(TableExpression));
        }

        if (deleteStatement.JoinExpressions?.Count > 0)
        {
            foreach (var joinExpression in deleteStatement.JoinExpressions)
            {
                deleteBuilder
                    .AppendLine()
                    .Append(JoinExpression(joinExpression));
            }
        }

        if (deleteStatement.WhereExpressions?.Count > 0)
        {
            deleteBuilder
                .AppendLine()
                .Append("WHERE ")
                .Append("(")
                .AppendJoin(" AND ", deleteStatement.WhereExpressions.Select(WhereExpression))
                .Append(")");
        }

        if (deleteStatement.OutputExpressions?.Count > 0)
        {
            deleteBuilder
                .AppendLine()
                .Append("RETURNING ")
                .AppendJoin(", ", deleteStatement.OutputExpressions.Select(ColumnExpression));
        }

        deleteBuilder.AppendLine(";");

        return StringBuilderCache.ToString(deleteBuilder);
    }

    /// <summary>
    /// Builds a SQL table expression for PostgreSQL, omitting schema (PostgreSQL does not support schema in the same way as SQL Server).
    /// </summary>
    /// <param name="tableExpression">The <see cref="TableExpression"/> representing the table.</param>
    /// <returns>A SQL table expression string for PostgreSQL.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tableExpression"/> is <c>null</c>.</exception>
    public override string TableExpression(TableExpression tableExpression)
    {
        if (tableExpression is null)
            throw new ArgumentNullException(nameof(tableExpression));

        // sqlite doesn't support schema
        tableExpression = tableExpression with { TableSchema = null };

        return base.TableExpression(tableExpression);
    }

    /// <summary>
    /// Builds a SQL WHERE expression for PostgreSQL, handling various filter operators and parameterization.
    /// </summary>
    /// <param name="whereExpression">The <see cref="WhereExpression"/> representing the condition.</param>
    /// <returns>A SQL WHERE expression string for PostgreSQL.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="whereExpression"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if required properties are not specified.</exception>
    public override string WhereExpression(WhereExpression whereExpression)
    {
        if (whereExpression is null)
            throw new ArgumentNullException(nameof(whereExpression));

        if (string.IsNullOrWhiteSpace(whereExpression.ColumnName))
            throw new ArgumentException($"'{nameof(whereExpression.ColumnName)}' property cannot be null or empty.", nameof(whereExpression));

        if (whereExpression.IsRaw)
            return whereExpression.ColumnName;

        var parameterlessFilters = new[] { FilterOperators.IsNull, FilterOperators.IsNotNull };
        if (!parameterlessFilters.Contains(whereExpression.FilterOperator) && string.IsNullOrWhiteSpace(whereExpression.ParameterName))
            throw new ArgumentException($"'{nameof(whereExpression.ParameterName)}' property cannot be null or empty.", nameof(whereExpression));

        var columnName = ColumnExpression(whereExpression);

        return whereExpression.FilterOperator switch
        {
            FilterOperators.StartsWith => $"{columnName} LIKE {whereExpression.ParameterName} || '%'",
            FilterOperators.EndsWith => $"{columnName} LIKE '%' || {whereExpression.ParameterName}",
            FilterOperators.Contains => $"{columnName} LIKE '%' || {whereExpression.ParameterName} || '%'",
            FilterOperators.Equal => $"{columnName} = {whereExpression.ParameterName}",
            FilterOperators.NotEqual => $"{columnName} != {whereExpression.ParameterName}",
            FilterOperators.LessThan => $"{columnName} < {whereExpression.ParameterName}",
            FilterOperators.LessThanOrEqual => $"{columnName} <= {whereExpression.ParameterName}",
            FilterOperators.GreaterThan => $"{columnName} > {whereExpression.ParameterName}",
            FilterOperators.GreaterThanOrEqual => $"{columnName} >= {whereExpression.ParameterName}",
            FilterOperators.IsNull => $"{columnName} IS NULL",
            FilterOperators.IsNotNull => $"{columnName} IS NOT NULL",
            FilterOperators.In => $"{columnName} IN ({whereExpression.ParameterName})",
            _ => $"{columnName} = {whereExpression.ParameterName}",
        };
    }

    /// <summary>
    /// Builds a SQL LIMIT/OFFSET expression for PostgreSQL.
    /// </summary>
    /// <param name="limitExpression">The <see cref="LimitExpression"/> representing the limit and offset.</param>
    /// <returns>A SQL LIMIT/OFFSET expression string for PostgreSQL, or an empty string if not applicable.</returns>
    public override string LimitExpression(LimitExpression limitExpression)
    {
        if (limitExpression is null || limitExpression.Size == 0)
            return string.Empty;

        return $"LIMIT {limitExpression.Size} OFFSET {limitExpression.Offset}";
    }

    /// <summary>
    /// Quotes an identifier (such as a table or column name) for PostgreSQL, using double quotes.
    /// </summary>
    /// <param name="name">The identifier to quote.</param>
    /// <returns>The quoted identifier, or the original name if quoting is not required.</returns>
    public override string QuoteIdentifier(string name)
    {
        if (name.IsNullOrWhiteSpace())
            return string.Empty;

        if (name == "*")
            return name;

        if (name.StartsWith("\"") && name.EndsWith("\""))
            return name;

        return "\"" + name.Replace("\"", "\"\"") + "\"";
    }

    /// <summary>
    /// Parses a quoted identifier and returns the unquoted name for PostgreSQL.
    /// </summary>
    /// <param name="name">The quoted identifier.</param>
    /// <returns>The unquoted identifier name.</returns>
    public override string ParseIdentifier(string name)
    {
        if (name.StartsWith("\"") && name.EndsWith("\""))
            return name.Substring(1, name.Length - 2);

        return name;
    }
}
