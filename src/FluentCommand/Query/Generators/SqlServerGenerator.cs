using FluentCommand.Extensions;

namespace FluentCommand.Query.Generators;


public class SqlServerGenerator : IQueryGenerator
{
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
            selectBuilder.AppendJoin(", ", selectStatement.SelectExpressions.Select(s => SelectExpression(s)));
        else
            selectBuilder.Append("*");

        selectBuilder
            .AppendLine()
            .Append("FROM ")
            .AppendJoin(", ", selectStatement.FromExpressions.Select(f => TableExpression(f)));

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
                .AppendJoin(" AND ", selectStatement.WhereExpressions.Select(w => WhereExpression(w)))
                .Append(")");
        }

        if (selectStatement.GroupExpressions?.Count > 0)
        {
            selectBuilder
                .AppendLine()
                .Append("GROUP BY ")
                .AppendJoin(", ", selectStatement.GroupExpressions.Select(g => GroupExpression(g)));
        }

        if (selectStatement.SortExpressions?.Count > 0)
        {
            selectBuilder
                .AppendLine()
                .Append("ORDER BY ")
                .AppendJoin(", ", selectStatement.SortExpressions.Select(s => SortExpression(s)));
        }

        if (selectStatement.LimitExpressions?.Count > 0)
        {
            selectBuilder
                .AppendLine()
                .AppendJoin(" ", selectStatement.LimitExpressions.Select(l => LimitExpression(l)));
        }

        selectBuilder.AppendLine(";");

        return StringBuilderCache.ToString(selectBuilder);
    }

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
                .AppendJoin(", ", insertStatement.ColumnExpressions.Select(c => ColumnExpression(c)))
                .Append(")");
        }

        if (insertStatement.OutputExpressions?.Count > 0)
        {
            insertBuilder
                .AppendLine()
                .Append("OUTPUT ")
                .AppendJoin(", ", insertStatement.OutputExpressions.Select(o => ColumnExpression(o)));
        }

        insertBuilder
            .AppendLine()
            .Append("VALUES ")
            .Append("(")
            .AppendJoin(", ", insertStatement.ValueExpressions)
            .Append(");");

        return StringBuilderCache.ToString(insertBuilder);
    }

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
            .AppendJoin(", ", updateStatement.UpdateExpressions.Select(u => UpdateExpression(u)));

        if (updateStatement.OutputExpressions?.Count > 0)
        {
            updateBuilder
                .AppendLine()
                .Append("OUTPUT ")
                .AppendJoin(", ", updateStatement.OutputExpressions.Select(o => ColumnExpression(o)));
        }

        if (updateStatement.FromExpressions?.Count > 0)
        {
            updateBuilder
                .AppendLine()
                .Append("FROM ")
                .AppendJoin(", ", updateStatement.FromExpressions.Select(f => TableExpression(f)));
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
                .AppendJoin(" AND ", updateStatement.WhereExpressions.Select(w => WhereExpression(w)))
                .Append(")");
        }

        updateBuilder.AppendLine(";");

        return StringBuilderCache.ToString(updateBuilder);
    }

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
                .AppendJoin(", ", deleteStatement.OutputExpressions.Select(o => ColumnExpression(o)));
        }

        if (deleteStatement.FromExpressions?.Count > 0)
        {
            deleteBuilder
                .AppendLine()
                .Append("FROM ")
                .AppendJoin(", ", deleteStatement.FromExpressions.Select(f => TableExpression(f)));
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
                .AppendJoin(" AND ", deleteStatement.WhereExpressions.Select(w => WhereExpression(w)))
                .Append(")");
        }

        deleteBuilder.AppendLine(";");

        return StringBuilderCache.ToString(deleteBuilder);
    }

    public virtual string BuildWhere(IReadOnlyCollection<WhereExpression> whereExpressions)
    {
        if (whereExpressions == null || whereExpressions.Count == 0)
            return null;

        var whereBuilder = StringBuilderCache.Acquire();

        if (whereExpressions?.Count > 0)
        {
            whereBuilder
                .Append("(")
                .AppendJoin(" AND ", whereExpressions)
                .Append(")");
        }

        return StringBuilderCache.ToString(whereBuilder);
    }

    public virtual string BuildOrder(IReadOnlyCollection<SortExpression> sortExpressions)
    {
        if (sortExpressions == null || sortExpressions.Count == 0)
            return null;

        var orderBuilder = StringBuilderCache.Acquire();

        if (sortExpressions?.Count > 0)
        {
            orderBuilder
                .AppendJoin(", ", sortExpressions);
        }

        return StringBuilderCache.ToString(orderBuilder);
    }


    public virtual string CommentExpression(string comment)
    {
        return $"/* {comment} */";
    }

    public virtual string SelectExpression(ColumnExpression columnExpression)
    {
        if (columnExpression is AggergateExpression aggergateExpression)
            return AggregateExpression(aggergateExpression);

        return ColumnExpression(columnExpression);
    }

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

    public virtual string AggregateExpression(AggergateExpression aggergateExpression)
    {
        if (aggergateExpression is null)
            throw new ArgumentNullException(nameof(aggergateExpression));

        if (aggergateExpression.IsRaw)
            return aggergateExpression.ColumnName;

        var selectClause = ColumnExpression(aggergateExpression);

        return aggergateExpression.Aggregate switch
        {
            AggregateFunctions.Average => $"AVG({selectClause})",
            AggregateFunctions.Count => $"COUNT({selectClause})",
            AggregateFunctions.Max => $"MAX({selectClause})",
            AggregateFunctions.Min => $"MIN({selectClause})",
            AggregateFunctions.Sum => $"SUM({selectClause})",
            _ => throw new NotImplementedException(),
        };
    }

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

    public virtual string GroupExpression(GroupExpression groupExpression)
    {
        if (groupExpression is null)
            throw new ArgumentNullException(nameof(groupExpression));

        return ColumnExpression(groupExpression);
    }

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
            FilterOperators.StartsWith => $"{columnName} LIKE '%' + {whereExpression.ParameterName}",
            FilterOperators.EndsWith => $"{columnName} LIKE {whereExpression.ParameterName} + '%'",
            FilterOperators.Contains => $"{columnName} LIKE '%' + {whereExpression.ParameterName} + '%'",
            FilterOperators.Equal => $"{columnName} = {whereExpression.ParameterName}",
            FilterOperators.NotEqual => $"{columnName} != {whereExpression.ParameterName}",
            FilterOperators.LessThan => $"{columnName} < {whereExpression.ParameterName}",
            FilterOperators.LessThanOrEqual => $"{columnName} <= {whereExpression.ParameterName}",
            FilterOperators.GreaterThan => $"{columnName} > {whereExpression.ParameterName}",
            FilterOperators.GreaterThanOrEqual => $"{columnName} >= {whereExpression.ParameterName}",
            FilterOperators.IsNull => $"{columnName} IS NULL",
            FilterOperators.IsNotNull => $"{columnName} IS NOT NULL",
            _ => $"{columnName} = {whereExpression.ParameterName}",
        };
    }

    public virtual string LogicalExpression(IReadOnlyCollection<WhereExpression> whereExpressions, LogicalOperators logicalOperator)
    {
        if (whereExpressions == null || whereExpressions.Count == 0)
            return string.Empty;

        var stringBuilder = StringBuilderCache.Acquire();
        var oparator = logicalOperator == LogicalOperators.And ? " AND " : " OR ";

        stringBuilder
            .Append("(")
            .AppendJoin(oparator, whereExpressions.Select(w => WhereExpression(w)))
            .Append(")");

        return StringBuilderCache.ToString(stringBuilder);
    }

    public virtual string LimitExpression(LimitExpression limitExpression)
    {
        if (limitExpression is null || limitExpression.Size == 0)
            return string.Empty;

        return $"OFFSET {limitExpression.Offset} ROWS FETCH NEXT {limitExpression.Size} ROWS ONLY";
    }

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

    public virtual string ParseIdentifier(string name)
    {
        if (name.StartsWith("[") && name.EndsWith("]"))
            return name.Substring(1, name.Length - 2);

        return name;
    }

}
