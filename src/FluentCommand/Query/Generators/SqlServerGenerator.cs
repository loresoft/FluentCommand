using FluentCommand.Extensions;
using FluentCommand.Internal;

namespace FluentCommand.Query.Generators;

/// <summary>
/// Provides a SQL generator for SQL Server, implementing SQL statement and expression generation
/// with SQL Server-specific syntax and conventions.
/// </summary>
public class SqlServerGenerator : IQueryGenerator
{
    /// <summary>
    /// Builds a SQL SELECT statement for SQL Server, including support for JOIN, WHERE, GROUP BY, ORDER BY, LIMIT, and comments.
    /// </summary>
    /// <param name="selectStatement">The <see cref="SelectStatement"/> containing the SELECT statement configuration.</param>
    /// <returns>A SQL SELECT statement string for SQL Server.</returns>
    /// <exception cref="ArgumentException">Thrown if no table is specified in <paramref name="selectStatement"/>.</exception>
    public virtual string BuildSelect(SelectStatement selectStatement)
    {
        if (selectStatement.FromExpressions == null || selectStatement.FromExpressions.Count == 0)
            throw new ArgumentException("No table specified to select from", nameof(selectStatement.FromExpressions));

        var selectBuilder = StringBuilderCache.Acquire();

        if (selectStatement.CommentExpressions?.Count > 0)
        {
            selectBuilder
                .AppendJoin(Environment.NewLine, selectStatement.CommentExpressions)
                .AppendLine();
        }

        selectBuilder
            .Append("SELECT ");

        if (selectStatement.SelectExpressions?.Count > 0)
            selectBuilder.AppendJoin(", ", selectStatement.SelectExpressions.Select(SelectExpression));
        else
            selectBuilder.Append("*");

        selectBuilder
            .AppendLine()
            .Append("FROM ")
            .AppendJoin(", ", selectStatement.FromExpressions.Select(TableExpression));

        if (selectStatement.JoinExpressions?.Count > 0)
        {
            foreach (var joinExpression in selectStatement.JoinExpressions)
            {
                selectBuilder
                    .AppendLine()
                    .Append(JoinExpression(joinExpression));
            }
        }

        if (selectStatement.WhereExpressions?.Count > 0)
        {
            selectBuilder
                .AppendLine()
                .Append("WHERE ")
                .Append("(")
                .AppendJoin(" AND ", selectStatement.WhereExpressions.Select(WhereExpression))
                .Append(")");
        }

        if (selectStatement.GroupExpressions?.Count > 0)
        {
            selectBuilder
                .AppendLine()
                .Append("GROUP BY ")
                .AppendJoin(", ", selectStatement.GroupExpressions.Select(GroupExpression));
        }

        if (selectStatement.SortExpressions?.Count > 0)
        {
            selectBuilder
                .AppendLine()
                .Append("ORDER BY ")
                .AppendJoin(", ", selectStatement.SortExpressions.Select(SortExpression));
        }

        if (selectStatement.LimitExpressions?.Count > 0)
        {
            selectBuilder
                .AppendLine()
                .AppendJoin(" ", selectStatement.LimitExpressions.Select(LimitExpression));
        }

        selectBuilder.AppendLine(";");

        return StringBuilderCache.ToString(selectBuilder);
    }

    /// <summary>
    /// Builds a SQL INSERT statement for SQL Server, including support for OUTPUT and comments.
    /// </summary>
    /// <param name="insertStatement">The <see cref="InsertStatement"/> containing the INSERT statement configuration.</param>
    /// <returns>A SQL INSERT statement string for SQL Server.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="insertStatement"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if the table or values are not specified.</exception>
    public virtual string BuildInsert(InsertStatement insertStatement)
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

        if (insertStatement.OutputExpressions?.Count > 0)
        {

            insertBuilder
                .AppendLine()
                .Append("OUTPUT ")
                .AppendJoin(", ", insertStatement.OutputExpressions.Select(c => ColumnExpression(c, "INSERTED")));
        }

        insertBuilder
            .AppendLine()
            .Append("VALUES ")
            .Append("(")
            .AppendJoin(", ", insertStatement.ValueExpressions)
            .Append(");");

        return StringBuilderCache.ToString(insertBuilder);
    }

    /// <summary>
    /// Builds a SQL UPDATE statement for SQL Server, including support for OUTPUT, FROM, JOIN, WHERE, and comments.
    /// </summary>
    /// <param name="updateStatement">The <see cref="UpdateStatement"/> containing the UPDATE statement configuration.</param>
    /// <returns>A SQL UPDATE statement string for SQL Server.</returns>
    /// <exception cref="ArgumentException">Thrown if the table or update values are not specified.</exception>
    public virtual string BuildUpdate(UpdateStatement updateStatement)
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

        if (updateStatement.OutputExpressions?.Count > 0)
        {
            updateBuilder
                .AppendLine()
                .Append("OUTPUT ")
                .AppendJoin(", ", updateStatement.OutputExpressions.Select(c => ColumnExpression(c, "INSERTED")));
        }

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

        updateBuilder.AppendLine(";");

        return StringBuilderCache.ToString(updateBuilder);
    }

    /// <summary>
    /// Builds a SQL DELETE statement for SQL Server, including support for OUTPUT, FROM, JOIN, WHERE, and comments.
    /// </summary>
    /// <param name="deleteStatement">The <see cref="DeleteStatement"/> containing the DELETE statement configuration.</param>
    /// <returns>A SQL DELETE statement string for SQL Server.</returns>
    /// <exception cref="ArgumentException">Thrown if the table is not specified.</exception>
    public virtual string BuildDelete(DeleteStatement deleteStatement)
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

        if (deleteStatement.OutputExpressions?.Count > 0)
        {
            deleteBuilder
                .AppendLine()
                .Append("OUTPUT ")
                .AppendJoin(", ", deleteStatement.OutputExpressions.Select(c => ColumnExpression(c, "DELETED")));
        }

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

        deleteBuilder.AppendLine(";");

        return StringBuilderCache.ToString(deleteBuilder);
    }

    /// <summary>
    /// Builds a SQL WHERE clause from the specified collection of <see cref="WhereExpression"/> objects.
    /// </summary>
    /// <param name="whereExpressions">A collection of <see cref="WhereExpression"/> objects representing WHERE conditions.</param>
    /// <returns>A SQL WHERE clause string, or <c>null</c> if no expressions are provided.</returns>
    public virtual string BuildWhere(IReadOnlyCollection<WhereExpression> whereExpressions)
    {
        if (whereExpressions == null || whereExpressions.Count == 0)
            return null;

        var whereBuilder = StringBuilderCache.Acquire();

        if (whereExpressions?.Count > 0)
        {
            whereBuilder
                .Append("(")
                .AppendJoin(" AND ", whereExpressions.Select(WhereExpression))
                .Append(")");
        }

        return StringBuilderCache.ToString(whereBuilder);
    }

    /// <summary>
    /// Builds a SQL ORDER BY clause from the specified collection of <see cref="SortExpression"/> objects.
    /// </summary>
    /// <param name="sortExpressions">A collection of <see cref="SortExpression"/> objects representing sort conditions.</param>
    /// <returns>A SQL ORDER BY clause string, or <c>null</c> if no expressions are provided.</returns>
    public virtual string BuildOrder(IReadOnlyCollection<SortExpression> sortExpressions)
    {
        if (sortExpressions == null || sortExpressions.Count == 0)
            return null;

        var orderBuilder = StringBuilderCache.Acquire();

        if (sortExpressions?.Count > 0)
        {
            orderBuilder
                .AppendJoin(", ", sortExpressions.Select(SortExpression));
        }

        return StringBuilderCache.ToString(orderBuilder);
    }

    /// <summary>
    /// Builds a SQL comment expression.
    /// </summary>
    /// <param name="comment">The comment text.</param>
    /// <returns>A SQL comment string.</returns>
    public virtual string CommentExpression(string comment)
    {
        return $"/* {comment} */";
    }

    /// <summary>
    /// Builds a SQL SELECT column or aggregate expression.
    /// </summary>
    /// <param name="columnExpression">
    /// The <see cref="FluentCommand.Query.Generators.ColumnExpression"/> or
    /// <see cref="FluentCommand.Query.Generators.AggregateExpression"/> to select.
    /// </param>
    public virtual string SelectExpression(ColumnExpression columnExpression)
    {
        if (columnExpression is AggregateExpression aggregateExpression)
            return AggregateExpression(aggregateExpression);

        return ColumnExpression(columnExpression);
    }

    /// <summary>
    /// Builds a SQL column expression from the specified <see cref="FluentCommand.Query.Generators.ColumnExpression"/>.
    /// </summary>
    /// <param name="columnExpression">The <see cref="FluentCommand.Query.Generators.ColumnExpression"/> representing the column.</param>
    /// <returns>A SQL column expression string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="columnExpression"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if the column name is not specified.</exception>
    public virtual string ColumnExpression(ColumnExpression columnExpression)
    {
        if (columnExpression is null)
            throw new ArgumentNullException(nameof(columnExpression));

        if (string.IsNullOrWhiteSpace(columnExpression.ColumnName))
            throw new ArgumentException($"'{nameof(columnExpression.ColumnName)}' property cannot be null or empty.", nameof(columnExpression));

        if (columnExpression.IsRaw)
            return columnExpression.ColumnName;

        var quotedName = QuoteIdentifier(columnExpression.ColumnName);

        var clause = columnExpression.TableAlias.HasValue()
            ? $"{QuoteIdentifier(columnExpression.TableAlias)}.{quotedName}"
            : quotedName;

        if (columnExpression.ColumnAlias.HasValue())
            clause += $" AS {QuoteIdentifier(columnExpression.ColumnAlias)}";

        return clause;
    }

    /// <summary>
    /// Builds a SQL aggregate expression (e.g., COUNT, SUM) from the specified <see cref="AggregateExpression"/>.
    /// </summary>
    /// <param name="aggregateExpression">The <see cref="AggregateExpression"/> representing the aggregate function.</param>
    /// <returns>A SQL aggregate expression string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="aggregateExpression"/> is <c>null</c>.</exception>
    public virtual string AggregateExpression(AggregateExpression aggregateExpression)
    {
        if (aggregateExpression is null)
            throw new ArgumentNullException(nameof(aggregateExpression));

        if (aggregateExpression.IsRaw)
            return aggregateExpression.ColumnName;

        var selectClause = ColumnExpression(aggregateExpression);

        return aggregateExpression.Aggregate switch
        {
            AggregateFunctions.Average => $"AVG({selectClause})",
            AggregateFunctions.Count => $"COUNT({selectClause})",
            AggregateFunctions.Max => $"MAX({selectClause})",
            AggregateFunctions.Min => $"MIN({selectClause})",
            AggregateFunctions.Sum => $"SUM({selectClause})",
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Builds a SQL table expression from the specified <see cref="TableExpression"/>.
    /// </summary>
    /// <param name="tableExpression">The <see cref="TableExpression"/> representing the table.</param>
    /// <returns>A SQL table expression string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tableExpression"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if the table name is not specified.</exception>
    public virtual string TableExpression(TableExpression tableExpression)
    {
        if (tableExpression is null)
            throw new ArgumentNullException(nameof(tableExpression));

        if (string.IsNullOrWhiteSpace(tableExpression.TableName))
            throw new ArgumentException($"'{nameof(tableExpression.TableName)}' property cannot be null or empty.", nameof(tableExpression));

        if (tableExpression.IsRaw)
            return tableExpression.TableName;

        var quotedName = QuoteIdentifier(tableExpression.TableName);

        var fromClause = tableExpression.TableSchema.HasValue()
            ? $"{QuoteIdentifier(tableExpression.TableSchema)}.{quotedName}"
            : quotedName;

        if (tableExpression.TableAlias.HasValue())
            fromClause += $" AS {QuoteIdentifier(tableExpression.TableAlias)}";

        return fromClause;
    }

    /// <summary>
    /// Builds a SQL sort expression from the specified <see cref="SortExpression"/>.
    /// </summary>
    /// <param name="sortExpression">The <see cref="SortExpression"/> representing the sort condition.</param>
    /// <returns>A SQL sort expression string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sortExpression"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if the column name is not specified.</exception>
    public virtual string SortExpression(SortExpression sortExpression)
    {
        if (sortExpression is null)
            throw new ArgumentNullException(nameof(sortExpression));

        if (string.IsNullOrWhiteSpace(sortExpression.ColumnName))
            throw new ArgumentException($"'{nameof(sortExpression.ColumnName)}' property cannot be null or empty.", nameof(sortExpression));

        if (sortExpression.IsRaw)
            return sortExpression.ColumnName;

        var quotedName = ColumnExpression(sortExpression);

        return sortExpression.SortDirection == SortDirections.Ascending
            ? $"{quotedName} ASC"
            : $"{quotedName} DESC";
    }

    /// <summary>
    /// Builds a SQL GROUP BY expression from the specified <see cref="GroupExpression"/>.
    /// </summary>
    /// <param name="groupExpression">The <see cref="GroupExpression"/> representing the group by condition.</param>
    /// <returns>A SQL GROUP BY expression string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="groupExpression"/> is <c>null</c>.</exception>
    public virtual string GroupExpression(GroupExpression groupExpression)
    {
        if (groupExpression is null)
            throw new ArgumentNullException(nameof(groupExpression));

        return ColumnExpression(groupExpression);
    }

    /// <summary>
    /// Builds a SQL WHERE expression from the specified <see cref="WhereExpression"/>.
    /// </summary>
    /// <param name="whereExpression">The <see cref="WhereExpression"/> representing the condition.</param>
    /// <returns>A SQL WHERE expression string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="whereExpression"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if required properties are not specified.</exception>
    public virtual string WhereExpression(WhereExpression whereExpression)
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
            FilterOperators.StartsWith => $"{columnName} LIKE {whereExpression.ParameterName} + '%'",
            FilterOperators.EndsWith => $"{columnName} LIKE '%' + {whereExpression.ParameterName}",
            FilterOperators.Contains => $"{columnName} LIKE '%' + {whereExpression.ParameterName} + '%'",
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
    /// Builds a logical SQL expression (e.g., AND/OR group) from the specified WHERE expressions and logical operator.
    /// </summary>
    /// <param name="whereExpressions">A collection of <see cref="WhereExpression"/> objects representing conditions.</param>
    /// <param name="logicalOperator">The <see cref="LogicalOperators"/> value to combine the expressions.</param>
    /// <returns>A logical SQL expression string, or an empty string if no expressions are provided.</returns>
    public virtual string LogicalExpression(IReadOnlyCollection<WhereExpression> whereExpressions, LogicalOperators logicalOperator)
    {
        if (whereExpressions == null || whereExpressions.Count == 0)
            return string.Empty;

        var stringBuilder = StringBuilderCache.Acquire();
        var logical = logicalOperator == LogicalOperators.And ? " AND " : " OR ";

        stringBuilder
            .Append("(")
            .AppendJoin(logical, whereExpressions.Select(WhereExpression))
            .Append(")");

        return StringBuilderCache.ToString(stringBuilder);
    }

    /// <summary>
    /// Builds a SQL LIMIT/OFFSET expression for SQL Server.
    /// </summary>
    /// <param name="limitExpression">The <see cref="LimitExpression"/> representing the limit and offset.</param>
    /// <returns>A SQL LIMIT/OFFSET expression string for SQL Server, or an empty string if not applicable.</returns>
    public virtual string LimitExpression(LimitExpression limitExpression)
    {
        if (limitExpression is null || limitExpression.Size == 0)
            return string.Empty;

        return $"OFFSET {limitExpression.Offset} ROWS FETCH NEXT {limitExpression.Size} ROWS ONLY";
    }

    /// <summary>
    /// Builds a SQL update expression from the specified <see cref="UpdateExpression"/>.
    /// </summary>
    /// <param name="updateExpression">The <see cref="UpdateExpression"/> representing the update operation.</param>
    /// <returns>A SQL update expression string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="updateExpression"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if required properties are not specified.</exception>
    public virtual string UpdateExpression(UpdateExpression updateExpression)
    {
        if (updateExpression is null)
            throw new ArgumentNullException(nameof(updateExpression));

        if (string.IsNullOrWhiteSpace(updateExpression.ColumnName))
            throw new ArgumentException($"'{nameof(updateExpression.ColumnName)}' cannot be null or empty.", nameof(updateExpression));

        if (updateExpression.IsRaw)
            return updateExpression.ColumnName;

        if (string.IsNullOrWhiteSpace(updateExpression.ParameterName))
            throw new ArgumentException($"'{nameof(updateExpression.ParameterName)}' cannot be null or empty.", nameof(updateExpression));

        var quotedName = ColumnExpression(updateExpression);

        return $"{quotedName} = {updateExpression.ParameterName}";
    }

    /// <summary>
    /// Builds a SQL JOIN expression from the specified <see cref="JoinExpression"/>.
    /// </summary>
    /// <param name="joinExpression">The <see cref="JoinExpression"/> representing the join operation.</param>
    /// <returns>A SQL JOIN expression string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="joinExpression"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if required properties are not specified.</exception>
    public virtual string JoinExpression(JoinExpression joinExpression)
    {
        if (joinExpression is null)
            throw new ArgumentNullException(nameof(joinExpression));

        if (string.IsNullOrWhiteSpace(joinExpression.LeftColumnName))
            throw new ArgumentException($"'{nameof(joinExpression.LeftColumnName)}' cannot be null or empty.", nameof(joinExpression));

        if (string.IsNullOrWhiteSpace(joinExpression.LeftTableAlias))
            throw new ArgumentException($"'{nameof(joinExpression.LeftTableAlias)}' cannot be null or empty.", nameof(joinExpression));

        if (string.IsNullOrWhiteSpace(joinExpression.RightColumnName))
            throw new ArgumentException($"'{nameof(joinExpression.RightColumnName)}' cannot be null or empty.", nameof(joinExpression));

        if (string.IsNullOrWhiteSpace(joinExpression.RightTableName))
            throw new ArgumentException($"'{nameof(joinExpression.RightTableName)}' cannot be null or empty.", nameof(joinExpression));

        if (string.IsNullOrWhiteSpace(joinExpression.RightTableAlias))
            throw new ArgumentException($"'{nameof(joinExpression.RightTableAlias)}' cannot be null or empty.", nameof(joinExpression));


        var leftColumn = ColumnExpression(new ColumnExpression(joinExpression.LeftColumnName, joinExpression.LeftTableAlias));
        var rightColumn = ColumnExpression(new ColumnExpression(joinExpression.RightColumnName, joinExpression.RightTableAlias));
        var rightTable = TableExpression(new TableExpression(joinExpression.RightTableName, joinExpression.RightTableSchema, joinExpression.RightTableAlias));

        var joinType = joinExpression.JoinType switch
        {
            JoinTypes.Inner => "INNER JOIN",
            JoinTypes.Left => "LEFT OUTER JOIN",
            JoinTypes.Right => "RIGHT OUTER JOIN",
            _ => throw new NotImplementedException(),
        };

        return $"{joinType} {rightTable} ON {leftColumn} = {rightColumn}";
    }

    /// <summary>
    /// Quotes an identifier (such as a table or column name) for SQL Server, using square brackets.
    /// </summary>
    /// <param name="name">The identifier to quote.</param>
    /// <returns>The quoted identifier, or the original name if quoting is not required.</returns>
    public virtual string QuoteIdentifier(string name)
    {
        if (name.IsNullOrWhiteSpace())
            return string.Empty;

        if (name == "*")
            return name;

        if (name.StartsWith("[") && name.EndsWith("]"))
            return name;

        return "[" + name.Replace("]", "]]") + "]";
    }

    /// <summary>
    /// Parses a quoted identifier and returns the unquoted name for SQL Server.
    /// </summary>
    /// <param name="name">The quoted identifier.</param>
    /// <returns>The unquoted identifier name.</returns>
    public virtual string ParseIdentifier(string name)
    {
        if (name.StartsWith("[") && name.EndsWith("]"))
            return name.Substring(1, name.Length - 2);

        return name;
    }

    /// <summary>
    /// Builds a SQL column expression with a specific table alias.
    /// </summary>
    /// <param name="columnExpression">The <see cref="Generators.ColumnExpression"/> representing the column.</param>
    /// <param name="tableAlias">The table alias to use if not already set.</param>
    /// <returns>A SQL column expression string with the specified table alias.</returns>
    private string ColumnExpression(ColumnExpression columnExpression, string tableAlias)
    {
        var column = columnExpression with
        {
            TableAlias = columnExpression.TableAlias ?? tableAlias
        };

        return ColumnExpression(column);
    }
}
