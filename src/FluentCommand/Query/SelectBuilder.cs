using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Select query builder
/// </summary>
public class SelectBuilder : SelectBuilder<SelectBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    /// <param name="logicalOperator">The logical operator.</param>
    public SelectBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

/// <summary>
/// Select query builder
/// </summary>
public abstract class SelectBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : SelectBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    /// <param name="logicalOperator">The logical operator.</param>
    protected SelectBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    /// <summary>
    /// Gets the select expressions.
    /// </summary>
    /// <value>
    /// The select expressions.
    /// </value>
    protected HashSet<ColumnExpression> SelectExpressions { get; } = new();

    /// <summary>
    /// Gets from expressions.
    /// </summary>
    /// <value>
    /// From expressions.
    /// </value>
    protected HashSet<TableExpression> FromExpressions { get; } = new();

    /// <summary>
    /// Gets the sort expressions.
    /// </summary>
    /// <value>
    /// The sort expressions.
    /// </value>
    protected HashSet<SortExpression> SortExpressions { get; } = new();

    /// <summary>
    /// Gets the group expressions.
    /// </summary>
    /// <value>
    /// The group expressions.
    /// </value>
    protected HashSet<GroupExpression> GroupExpressions { get; } = new();

    /// <summary>
    /// Gets the join expressions.
    /// </summary>
    /// <value>
    /// The join expressions.
    /// </value>
    protected HashSet<JoinExpression> JoinExpressions { get; } = new();

    /// <summary>
    /// Gets the limit expressions.
    /// </summary>
    /// <value>
    /// The limit expressions.
    /// </value>
    protected HashSet<LimitExpression> LimitExpressions { get; } = new();


    /// <summary>
    /// Adds a column expression with the specified name.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder Column(
        string columnName,
        string tableAlias = null,
        string columnAlias = null)
    {
        var selectClause = new ColumnExpression(columnName, tableAlias, columnAlias);

        SelectExpressions.Add(selectClause);

        return (TBuilder)this;
    }

    /// <summary>
    /// Conditionally adds a column expression with the specified name.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
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

    /// <summary>
    /// Adds a column expression for each of specified names.
    /// </summary>
    /// <param name="columnNames">The column names.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">columnNames</exception>
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


    /// <summary>
    /// Adds a count expression using the specified column name.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder Count(
        string columnName = "*",
        string tableAlias = null,
        string columnAlias = null)
    {
        var selectClause = new AggregateExpression(AggregateFunctions.Count, columnName, tableAlias, columnAlias);

        SelectExpressions.Add(selectClause);

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds an aggregate expression using the specified function and column name.
    /// </summary>
    /// <param name="function">The aggregate function.</param>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder Aggregate(
        AggregateFunctions function,
        string columnName,
        string tableAlias = null,
        string columnAlias = null)
    {
        var selectClause = new AggregateExpression(function, columnName, tableAlias, columnAlias);

        SelectExpressions.Add(selectClause);

        return (TBuilder)this;
    }


    /// <summary>
    /// Add a from clause to the query.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="tableSchema">The table schema.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public virtual TBuilder From(
        string tableName,
        string tableSchema = null,
        string tableAlias = null)
    {
        var fromClause = new TableExpression(tableName, tableSchema, tableAlias);

        FromExpressions.Add(fromClause);

        return (TBuilder)this;
    }

    /// <summary>
    /// Add a from clause to the query.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder From<TEntity>(
        string tableAlias = null)
    {
        var typeAccessor = TypeAccessor.GetAccessor<TEntity>();

        var fromClause = new TableExpression(typeAccessor.TableName, typeAccessor.TableSchema, tableAlias);

        FromExpressions.Add(fromClause);

        return (TBuilder)this;
    }


    /// <summary>
    /// Add a raw from clause to the query.
    /// </summary>
    /// <param name="fromClause">From clause.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder FromRaw(string fromClause)
    {
        if (fromClause.HasValue())
            FromExpressions.Add(new TableExpression(fromClause, IsRaw: true));

        return (TBuilder)this;
    }


    /// <summary>
    /// Add a join clause using the specified builder action
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder Join(Action<JoinBuilder> builder)
    {
        var innerBuilder = new JoinBuilder(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return (TBuilder)this;
    }


    /// <summary>
    /// Add an order by clause with the specified column name and sort direction.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder OrderBy(
        string columnName,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        return OrderBy(columnName, null, sortDirection);
    }

    /// <summary>
    /// Add an order by clause with the specified column name, sort direction and table alias.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder OrderBy(
        string columnName,
        string tableAlias,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        var orderClause = new SortExpression(columnName, tableAlias, sortDirection);

        SortExpressions.Add(orderClause);

        return (TBuilder)this;
    }

    /// <summary>
    /// Conditionally add an order by clause with the specified column name and sort direction.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder OrderByIf(
        string columnName,
        SortDirections sortDirection = SortDirections.Ascending,
        Func<string, bool> condition = null)
    {
        return OrderByIf(columnName, null, sortDirection, condition);
    }

    /// <summary>
    /// Conditionally add an order by clause with the specified column name, sort direction and table alias.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
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

    /// <summary>
    /// Add a raw order by clause to the query.
    /// </summary>
    /// <param name="sortExpression">The order by clause.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder OrderByRaw(string sortExpression)
    {
        if (sortExpression.HasValue())
            SortExpressions.Add(new SortExpression(sortExpression, IsRaw: true));

        return (TBuilder)this;
    }

    /// <summary>
    /// Conditionally add a raw order by clause to the query.
    /// </summary>
    /// <param name="sortExpression">The order by clause.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder OrderByRawIf(
        string sortExpression,
        Func<string, bool> condition = null)
    {
        if (condition != null && !condition(sortExpression))
            return (TBuilder)this;

        return OrderByRaw(sortExpression);
    }

    /// <summary>
    /// Add multiple raw order by clauses to the query.
    /// </summary>
    /// <param name="sortExpressions">The order by clauses.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder OrderByRaw(IEnumerable<string> sortExpressions)
    {
        if (sortExpressions is null)
            throw new ArgumentNullException(nameof(sortExpressions));

        foreach (var sortExpression in sortExpressions)
            SortExpressions.Add(new SortExpression(sortExpression, IsRaw: true));

        return (TBuilder)this;
    }


    /// <summary>
    /// Adds a group by clause with the specified column name
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder GroupBy(
        string columnName,
        string tableAlias = null)
    {
        var orderClause = new GroupExpression(columnName, tableAlias);

        GroupExpressions.Add(orderClause);

        return (TBuilder)this;
    }


    /// <summary>
    /// Adds a limit expression with specified offset and size.
    /// </summary>
    /// <param name="offset">The offset.</param>
    /// <param name="size">The size.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder Limit(int offset = 0, int size = 0)
    {
        // no paging
        if (size <= 0)
            return (TBuilder)this;

        var limitClause = new LimitExpression(offset, size);
        LimitExpressions.Add(limitClause);

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a page limit expression with specified page and page size.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
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

    /// <inheritdoc />
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
