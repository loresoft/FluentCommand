using System;
using System.Collections.Generic;

using FluentCommand.Query.Generators;

namespace FluentCommand;

public class SelectBuilder : SelectBuilder<SelectBuilder>
{
    public SelectBuilder(IQueryGenerator queryGenerator, Dictionary<string, object> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

public abstract class SelectBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : SelectBuilder<TBuilder>
{
    protected SelectBuilder(IQueryGenerator queryGenerator, Dictionary<string, object> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    public HashSet<string> SelectClause { get; } = new();

    public HashSet<string> FromClause { get; } = new();

    public HashSet<string> OrderByClause { get; } = new();

    public HashSet<string> LimitClause { get; } = new();


    public TBuilder Column(string columnName, string prefix = null, string alias = null)
    {
        var selectClause = QueryGenerator.SelectClause(columnName, prefix, alias);

        SelectClause.Add(selectClause);

        return (TBuilder)this;
    }

    public TBuilder Column(IEnumerable<string> columnNames)
    {
        if (columnNames is null)
            throw new ArgumentNullException(nameof(columnNames));

        foreach (var column in columnNames)
            Column(column);

        return (TBuilder)this;
    }

    public TBuilder From(string tableName, string tableSchema = null, string alias = null)
    {
        var fromClause = QueryGenerator.FromClause(tableName, tableSchema, alias);

        FromClause.Add(fromClause);

        return (TBuilder)this;
    }

    public TBuilder Where(Action<LogicalBuilder> builder)
    {
        var innerBuilder = new LogicalBuilder(QueryGenerator, Parameters, CommentExpressions, LogicalOperators.And);
        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();
        WhereClause.Add(statement.Statement);

        return (TBuilder)this;
    }

    public TBuilder OrderBy(string columnName, SortDirections sortDirection = SortDirections.Ascending)
    {
        var orderClause = QueryGenerator.OrderClause(columnName, sortDirection);

        OrderByClause.Add(orderClause);

        return (TBuilder)this;
    }

    public TBuilder OrderBy(IEnumerable<string> sortExpressions)
    {
        if (sortExpressions is null)
            throw new ArgumentNullException(nameof(sortExpressions));

        foreach (var sortExpression in sortExpressions)
            OrderByClause.Add(sortExpression);

        return (TBuilder)this;
    }

    public TBuilder Limit(int offset = 0, int size = 20)
    {
        var limitClause = QueryGenerator.LimitClause(offset: offset, size: size);
        LimitClause.Add(limitClause);

        return (TBuilder)this;
    }

    public override QueryStatement BuildStatement()
    {
        var statement = QueryGenerator.BuildSelect(
            selectClause: SelectClause,
            fromClause: FromClause,
            whereClause: WhereClause,
            orderByClause: OrderByClause,
            limitClause: LimitClause,
            commentExpression: CommentExpressions);

        return new QueryStatement(statement, Parameters);
    }
}
