using System;
using System.Collections.Generic;

using FluentCommand.Extensions;

namespace FluentCommand.Query.Generators;


public class SqlServerGenerator : IQueryGenerator
{
    public virtual string BuildSelect(
        IReadOnlyCollection<string> selectClause,
        IReadOnlyCollection<string> fromClause,
        IReadOnlyCollection<string> whereClause,
        IReadOnlyCollection<string> orderByClause,
        IReadOnlyCollection<string> groupByClause,
        IReadOnlyCollection<string> limitClause,
        IReadOnlyCollection<string> commentExpression)
    {
        if (fromClause == null || fromClause.Count == 0)
            throw new ArgumentException("No table specified to select from", nameof(fromClause));

        var selectBuilder = StringBuilderCache.Acquire();

        if (commentExpression?.Count > 0)
        {
            selectBuilder
                .AppendJoin(Environment.NewLine, commentExpression)
                .AppendLine();
        }

        selectBuilder
            .Append("SELECT ");

        if (selectClause?.Count > 0)
            selectBuilder.AppendJoin(", ", selectClause);
        else
            selectBuilder.Append("*");

        selectBuilder
            .AppendLine()
            .Append("FROM ")
            .AppendJoin(", ", fromClause);

        if (whereClause?.Count > 0)
        {
            selectBuilder
                .AppendLine()
                .Append("WHERE ")
                .Append("(")
                .AppendJoin(" AND ", whereClause)
                .Append(")");
        }
        if (groupByClause?.Count > 0)
        {
            selectBuilder
                .AppendLine()
                .Append("GROUP BY ")
                .AppendJoin(", ", groupByClause);
        }

        if (orderByClause?.Count > 0)
        {
            selectBuilder
                .AppendLine()
                .Append("ORDER BY ")
                .AppendJoin(", ", orderByClause);
        }

        if (limitClause?.Count > 0)
        {
            selectBuilder
                .AppendLine()
                .AppendJoin(" ", limitClause);
        }

        selectBuilder.AppendLine(";");

        return StringBuilderCache.ToString(selectBuilder);
    }

    public virtual string BuildInsert(
        string tableClause,
        IReadOnlyCollection<string> columnExpression,
        IReadOnlyCollection<string> outputClause,
        IReadOnlyCollection<string> valueExpression,
        IReadOnlyCollection<string> commentExpression)
    {
        if (tableClause.IsNullOrEmpty())
            throw new ArgumentException("No table specified to insert into", nameof(tableClause));

        if (valueExpression == null || valueExpression.Count == 0)
            throw new ArgumentException("No values specified for insert", nameof(valueExpression));

        var insertBuilder = StringBuilderCache.Acquire();

        if (commentExpression?.Count > 0)
        {
            insertBuilder
                .AppendJoin(Environment.NewLine, commentExpression)
                .AppendLine();
        }

        insertBuilder
            .Append("INSERT INTO ")
            .Append(tableClause);

        if (columnExpression?.Count > 0)
        {
            insertBuilder
                .Append(" (")
                .AppendJoin(", ", columnExpression)
                .Append(")");
        }

        if (outputClause?.Count > 0)
        {
            insertBuilder
                .AppendLine()
                .Append("OUTPUT ")
                .AppendJoin(", ", outputClause);
        }

        insertBuilder
            .AppendLine()
            .Append("VALUES ")
            .Append("(")
            .AppendJoin(", ", valueExpression)
            .Append(");");

        return StringBuilderCache.ToString(insertBuilder);
    }

    public virtual string BuildUpdate(
        string tableClause,
        IReadOnlyCollection<string> updateClause,
        IReadOnlyCollection<string> outputClause,
        IReadOnlyCollection<string> fromClause,
        IReadOnlyCollection<string> whereClause,
        IReadOnlyCollection<string> commentExpression)
    {
        if (tableClause.IsNullOrEmpty())
            throw new ArgumentException("No table specified to update", nameof(tableClause));

        if (updateClause == null || updateClause.Count == 0)
            throw new ArgumentException("No values specified for update", nameof(updateClause));

        var updateBuilder = StringBuilderCache.Acquire();

        if (commentExpression?.Count > 0)
        {
            updateBuilder
                .AppendJoin(Environment.NewLine, commentExpression)
                .AppendLine();
        }

        updateBuilder
            .Append("UPDATE ")
            .Append(tableClause)
            .AppendLine()
            .Append("SET ")
            .AppendJoin(", ", updateClause);

        if (outputClause?.Count > 0)
        {
            updateBuilder
                .AppendLine()
                .Append("OUTPUT ")
                .AppendJoin(", ", outputClause);
        }

        if (fromClause?.Count > 0)
        {
            updateBuilder
                .AppendLine()
                .Append("FROM ")
                .AppendJoin(", ", fromClause);
        }

        if (whereClause?.Count > 0)
        {
            updateBuilder
                .AppendLine()
                .Append("WHERE ")
                .Append("(")
                .AppendJoin(" AND ", whereClause)
                .Append(")");
        }

        updateBuilder.AppendLine(";");

        return StringBuilderCache.ToString(updateBuilder);
    }

    public virtual string BuildDelete(
        string tableClause,
        IReadOnlyCollection<string> outputClause,
        IReadOnlyCollection<string> fromClause,
        IReadOnlyCollection<string> whereClause,
        IReadOnlyCollection<string> commentExpression)
    {
        if (tableClause.IsNullOrEmpty())
            throw new ArgumentException("No table specified to delete from", nameof(tableClause));

        var deleteBuilder = StringBuilderCache.Acquire();

        if (commentExpression?.Count > 0)
        {
            deleteBuilder
                .AppendJoin(Environment.NewLine, commentExpression)
                .AppendLine();
        }

        deleteBuilder
            .Append("DELETE FROM ")
            .Append(tableClause);

        if (outputClause?.Count > 0)
        {
            deleteBuilder
                .AppendLine()
                .Append("OUTPUT ")
                .AppendJoin(", ", outputClause);
        }

        if (fromClause?.Count > 0)
        {
            deleteBuilder
                .AppendLine()
                .Append("FROM ")
                .AppendJoin(", ", fromClause);
        }

        if (whereClause?.Count > 0)
        {
            deleteBuilder
                .AppendLine()
                .Append("WHERE ")
                .Append("(")
                .AppendJoin(" AND ", whereClause)
                .Append(")");
        }

        deleteBuilder.AppendLine(";");

        return StringBuilderCache.ToString(deleteBuilder);
    }


    public virtual string CommentClause(string comment)
    {
        return $"/* {comment} */";
    }

    public virtual string SelectClause(string columnName, string prefix = null, string alias = null)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException($"'{nameof(columnName)}' cannot be null or empty.", nameof(columnName));

        var quotedName = QuoteIdentifier(columnName);

        var clause = prefix.HasValue()
            ? $"{QuoteIdentifier(prefix)}.{quotedName}"
            : quotedName;

        if (alias.HasValue())
            clause += $" AS {QuoteIdentifier(alias)}";

        return clause;
    }

    public virtual string AggregateClause(AggregateFunctions aggregate, string columnName, string prefix = null, string alias = null)
    {
        var selectClause = SelectClause(columnName, prefix, alias);

        return aggregate switch
        {
            AggregateFunctions.Average => $"AVG({selectClause})",
            AggregateFunctions.Count => $"COUNT({selectClause})",
            AggregateFunctions.Max => $"MAX({selectClause})",
            AggregateFunctions.Min => $"MIN({selectClause})",
            AggregateFunctions.Sum => $"SUM({selectClause})",
            _ => throw new NotImplementedException(),
        };
    }

    public virtual string FromClause(string tableName, string tableSchema = null, string alias = null)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException($"'{nameof(tableName)}' cannot be null or empty.", nameof(tableName));

        var quotedName = QuoteIdentifier(tableName);

        var fromClause = tableSchema.HasValue()
            ? $"{QuoteIdentifier(tableSchema)}.{quotedName}"
            : quotedName;

        if (alias.HasValue())
            fromClause += $" AS {QuoteIdentifier(alias)}";

        return fromClause;
    }

    public virtual string OrderClause(string columnName, string prefix = null, SortDirections sortDirection = SortDirections.Ascending)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException($"'{nameof(columnName)}' cannot be null or empty.", nameof(columnName));

        var quotedName = SelectClause(columnName, prefix);

        return sortDirection == SortDirections.Ascending
            ? $"{quotedName} ASC"
            : $"{quotedName} DESC";
    }

    public virtual string GroupClause(string columnName, string prefix = null)
    {
        return SelectClause(columnName, prefix);
    }

    public virtual string WhereClause(string columnName, string parameterName, FilterOperators filterOperator = FilterOperators.Equal)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException($"'{nameof(columnName)}' cannot be null or empty.", nameof(columnName));

        if (string.IsNullOrWhiteSpace(parameterName))
            throw new ArgumentException($"'{nameof(parameterName)}' cannot be null or empty.", nameof(parameterName));

        var quotedName = QuoteIdentifier(columnName);


        return filterOperator switch
        {
            FilterOperators.StartsWith => $"{quotedName} LIKE '%' + {parameterName}",
            FilterOperators.EndsWith => $"{quotedName} LIKE {parameterName} + '%'",
            FilterOperators.Contains => $"{quotedName} LIKE '%' + {parameterName} + '%'",
            FilterOperators.Equal => $"{quotedName} = {parameterName}",
            FilterOperators.NotEqual => $"{quotedName} != {parameterName}",
            FilterOperators.LessThan => $"{quotedName} < {parameterName}",
            FilterOperators.LessThanOrEqual => $"{quotedName} <= {parameterName}",
            FilterOperators.GreaterThan => $"{quotedName} > {parameterName}",
            FilterOperators.GreaterThanOrEqual => $"{quotedName} >= {parameterName}",
            FilterOperators.IsNull => $"{quotedName} IS NULL",
            FilterOperators.IsNotNull => $"{quotedName} IS NOT NULL",
            FilterOperators.In => $"{quotedName} IN {parameterName}",
            _ => $"{quotedName} = {parameterName}",
        };
    }

    public virtual string LogicalClause(IReadOnlyCollection<string> whereClause, LogicalOperators logicalOperator)
    {
        if (whereClause == null || whereClause.Count == 0)
            return string.Empty;

        var stringBuilder = StringBuilderCache.Acquire();
        var oparator = logicalOperator == LogicalOperators.And ? " AND " : " OR ";

        stringBuilder
            .Append("(")
            .AppendJoin(oparator, whereClause)
            .Append(")");

        return StringBuilderCache.ToString(stringBuilder);
    }

    public virtual string LimitClause(int offset, int size)
    {
        return $"OFFSET {offset} ROWS FETCH NEXT {size} ROWS ONLY";
    }

    public virtual string UpdateClause(string columnName, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException($"'{nameof(columnName)}' cannot be null or empty.", nameof(columnName));

        if (string.IsNullOrWhiteSpace(parameterName))
            throw new ArgumentException($"'{nameof(parameterName)}' cannot be null or empty.", nameof(parameterName));

        var quotedName = QuoteIdentifier(columnName);

        return $"{quotedName} = {parameterName}";
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
