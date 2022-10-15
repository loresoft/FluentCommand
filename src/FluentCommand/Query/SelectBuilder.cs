using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

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

    protected HashSet<ColumnExpression> SelectExpressions { get; } = new();

    protected HashSet<TableExpression> FromExpressions { get; } = new();

    protected HashSet<SortExpression> SortExpressions { get; } = new();

    protected HashSet<GroupExpression> GroupExpressions { get; } = new();

    protected HashSet<JoinExpression> JoinExpressions { get; } = new();

    protected HashSet<LimitExpression> LimitExpressions { get; } = new();


    public TBuilder Column(
        string columnName,
        string tableAlias = null,
        string columnAlias = null)
    {
        var selectClause = new ColumnExpression(columnName, tableAlias, columnAlias);

        SelectExpressions.Add(selectClause);

        return (TBuilder)this;
    }

    public TBuilder ColumnIf(
        string columnName,
        string tableAlias = null,
        string columnAlias = null,
        Func<string, bool> condition = null)
    {
        if (condition != null && !condition(columnName))
            return (TBuilder)this;

        return Column(columnName, tableAlias, columnAlias);
    }

    public virtual TBuilder Columns(
        IEnumerable<string> columnNames,
        string tableAlias = null)
    {
        if (columnNames is null)
            throw new ArgumentNullException(nameof(columnNames));

        foreach (var column in columnNames)
            Column(column, tableAlias);

        return (TBuilder)this;
    }


    public TBuilder Count(
        string columnName = "*",
        string tableAlias = null,
        string columnAlias = null)
    {
        var selectClause = new AggergateExpression(AggregateFunctions.Count, columnName, tableAlias, columnAlias);

        SelectExpressions.Add(selectClause);

        return (TBuilder)this;
    }

    public TBuilder Aggregate(
        AggregateFunctions function,
        string columnName,
        string tableAlias = null,
        string columnAlias = null)
    {
        var selectClause = new AggergateExpression(function, columnName, tableAlias, columnAlias);

        SelectExpressions.Add(selectClause);

        return (TBuilder)this;
    }


    public virtual TBuilder From(
        string tableName,
        string tableSchema = null,
        string tableAlias = null)
    {
        var fromClause = new TableExpression(tableName, tableSchema, tableAlias);

        FromExpressions.Add(fromClause);

        return (TBuilder)this;
    }

    public TBuilder FromRaw(string fromClause)
    {
        if (fromClause.HasValue())
            FromExpressions.Add(new TableExpression(fromClause, IsRaw: true));

        return (TBuilder)this;
    }


    public TBuilder Join(Action<JoinBuilder> builder)
    {
        var innerBuilder = new JoinBuilder(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

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
        string tableAlias,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        var orderClause = new SortExpression(columnName, tableAlias, sortDirection);

        SortExpressions.Add(orderClause);

        return (TBuilder)this;
    }

    public TBuilder OrderByIf(
        string columnName,
        SortDirections sortDirection = SortDirections.Ascending,
        Func<string, bool> condition = null)
    {
        return OrderByIf(columnName, null, sortDirection, condition);
    }

    public TBuilder OrderByIf(
        string columnName,
        string tableAlias,
        SortDirections sortDirection = SortDirections.Ascending,
        Func<string, bool> condition = null)
    {
        if (condition != null && !condition(columnName))
            return (TBuilder)this;

        return OrderBy(columnName, tableAlias, sortDirection);
    }

    public TBuilder OrderByRaw(string sortExpression)
    {
        if (sortExpression.HasValue())
            SortExpressions.Add(new SortExpression(sortExpression, IsRaw: true));

        return (TBuilder)this;
    }

    public TBuilder OrderByRawIf(
        string sortExpression,
        Func<string, bool> condition = null)
    {
        if (condition != null && !condition(sortExpression))
            return (TBuilder)this;

        return OrderByRaw(sortExpression);
    }

    public TBuilder OrderByRaw(IEnumerable<string> sortExpressions)
    {
        if (sortExpressions is null)
            throw new ArgumentNullException(nameof(sortExpressions));

        foreach (var sortExpression in sortExpressions)
            SortExpressions.Add(new SortExpression(sortExpression, IsRaw: true));

        return (TBuilder)this;
    }


    public TBuilder GroupBy(
        string columnName,
        string tableAlias = null)
    {
        var orderClause = new GroupExpression(columnName, tableAlias);

        GroupExpressions.Add(orderClause);

        return (TBuilder)this;
    }


    public TBuilder Limit(int offset = 0, int size = 0)
    {
        // no paging
        if (size <= 0)
            return (TBuilder)this;

        var limitClause = new LimitExpression(offset, size);
        LimitExpressions.Add(limitClause);

        return (TBuilder)this;
    }

    public TBuilder Page(int page = 0, int pageSize = 0)
    {
        // no paging
        if (pageSize <= 0 || page <= 0)
            return (TBuilder)this;

        int offset = Math.Max(pageSize * (page - 1), 0);
        var limitClause = new LimitExpression(offset, pageSize);

        LimitExpressions.Add(limitClause);

        return (TBuilder)this;
    }

    public override QueryStatement BuildStatement()
    {
        var selectStatement = new SelectStatement(
            SelectExpressions,
            FromExpressions,
            JoinExpressions,
            WhereExpressions,
            SortExpressions,
            GroupExpressions,
            LimitExpressions,
            CommentExpressions);

        var statement = QueryGenerator.BuildSelect(selectStatement);

        return new QueryStatement(statement, Parameters);
    }
}
