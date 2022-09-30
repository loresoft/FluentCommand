using FluentCommand.Query.Generators;

namespace FluentCommand;

public class SelectBuilder : SelectBuilder<SelectBuilder>
{
    public SelectBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

public abstract class SelectBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : SelectBuilder<TBuilder>
{
    protected SelectBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    protected HashSet<string> SelectClause { get; } = new();

    protected HashSet<string> FromClause { get; } = new();

    protected HashSet<string> OrderByClause { get; } = new();

    protected HashSet<string> GroupByClause { get; } = new();

    protected HashSet<string> LimitClause { get; } = new();


    public TBuilder Column(
        string columnName,
        string prefix = null,
        string alias = null)
    {
        var selectClause = QueryGenerator.SelectClause(columnName, prefix, alias);

        SelectClause.Add(selectClause);

        return (TBuilder)this;
    }

    public TBuilder ColumnIf(
        string columnName,
        string prefix = null,
        string alias = null,
        Func<string, bool> condition = null)
    {
        if (condition != null && !condition(columnName))
            return (TBuilder)this;

        return Column(columnName, prefix, alias);
    }

    public TBuilder Columns(IEnumerable<string> columnNames)
    {
        if (columnNames is null)
            throw new ArgumentNullException(nameof(columnNames));

        foreach (var column in columnNames)
            Column(column);

        return (TBuilder)this;
    }


    public TBuilder Count(
        string columnName = "*",
        string prefix = null,
        string alias = null)
    {
        var selectClause = QueryGenerator.AggregateClause(AggregateFunctions.Count, columnName, prefix, alias);

        SelectClause.Add(selectClause);

        return (TBuilder)this;
    }

    public TBuilder Aggregate(
        AggregateFunctions function,
        string columnName,
        string prefix = null,
        string alias = null)
    {
        var selectClause = QueryGenerator.AggregateClause(function, columnName, prefix, alias);

        SelectClause.Add(selectClause);

        return (TBuilder)this;
    }


    public TBuilder From(
        string tableName,
        string tableSchema = null,
        string alias = null)
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


    public TBuilder OrderBy(
        string columnName,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        return OrderBy(columnName, null, sortDirection);
    }

    public TBuilder OrderBy(
        string columnName,
        string prefix,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        var orderClause = QueryGenerator.OrderClause(columnName, prefix, sortDirection);

        OrderByClause.Add(orderClause);

        return (TBuilder)this;
    }

    public TBuilder OrderByIf(
        string columnName,
        string prefix = null,
        SortDirections sortDirection = SortDirections.Ascending,
        Func<string, bool> condition = null)
    {
        if (condition != null && !condition(columnName))
            return (TBuilder)this;

        return OrderBy(columnName, prefix, sortDirection);
    }

    public TBuilder OrderBy(IEnumerable<string> sortExpressions)
    {
        if (sortExpressions is null)
            throw new ArgumentNullException(nameof(sortExpressions));

        foreach (var sortExpression in sortExpressions)
            OrderByClause.Add(sortExpression);

        return (TBuilder)this;
    }


    public TBuilder GroupBy(
        string columnName,
        string prefix = null)
    {
        var orderClause = QueryGenerator.GroupClause(columnName, prefix);

        GroupByClause.Add(orderClause);

        return (TBuilder)this;
    }


    public TBuilder Limit(int offset = 0, int size = 0)
    {
        // no paging
        if (size == 0)
            return (TBuilder)this;

        var limitClause = QueryGenerator.LimitClause(offset: offset, size: size);
        LimitClause.Add(limitClause);

        return (TBuilder)this;
    }

    public TBuilder Page(int page = 0, int pageSize = 0)
    {
        // no paging
        if (pageSize <= 0 || page <= 0)
            return (TBuilder)this;

        int offset = Math.Max(pageSize * (page - 1), 0);
        var limitClause = QueryGenerator.LimitClause(offset: offset, size: pageSize);

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
            groupByClause: GroupByClause,
            limitClause: LimitClause,
            commentExpression: CommentExpressions);

        return new QueryStatement(statement, Parameters);
    }
}
