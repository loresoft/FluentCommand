using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL SELECT statements with fluent, chainable methods.
/// </summary>
public class SelectBuilder : SelectBuilder<SelectBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    /// <param name="logicalOperator">The logical operator (<see cref="LogicalOperators"/>) to combine WHERE expressions. Defaults to <see cref="LogicalOperators.And"/>.</param>
    public SelectBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

/// <summary>
/// Provides a generic base class for building SQL SELECT statements with fluent, chainable methods.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder for fluent chaining.</typeparam>
public abstract class SelectBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : SelectBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    /// <param name="logicalOperator">The logical operator (<see cref="LogicalOperators"/>) to combine WHERE expressions. Defaults to <see cref="LogicalOperators.And"/>.</param>
    protected SelectBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    /// <summary>
    /// Gets the collection of select column expressions for the SELECT statement.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{ColumnExpression}"/> containing the select column expressions.
    /// </value>
    protected HashSet<ColumnExpression> SelectExpressions { get; } = new();

    /// <summary>
    /// Gets the collection of FROM table expressions for the SELECT statement.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{TableExpression}"/> containing the FROM table expressions.
    /// </value>
    protected HashSet<TableExpression> FromExpressions { get; } = new();

    /// <summary>
    /// Gets the collection of sort expressions for the ORDER BY clause.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{SortExpression}"/> containing the sort expressions.
    /// </value>
    protected HashSet<SortExpression> SortExpressions { get; } = new();

    /// <summary>
    /// Gets the collection of group by expressions for the GROUP BY clause.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{GroupExpression}"/> containing the group by expressions.
    /// </value>
    protected HashSet<GroupExpression> GroupExpressions { get; } = new();

    /// <summary>
    /// Gets the collection of join expressions for the SELECT statement.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{JoinExpression}"/> containing the join expressions.
    /// </value>
    protected HashSet<JoinExpression> JoinExpressions { get; } = new();

    /// <summary>
    /// Gets the collection of limit expressions for the SELECT statement.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{LimitExpression}"/> containing the limit expressions.
    /// </value>
    protected HashSet<LimitExpression> LimitExpressions { get; } = new();

    /// <summary>
    /// Adds a column expression with the specified name to the SELECT clause.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="columnAlias">The alias for the column (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Conditionally adds a column expression with the specified name to the SELECT clause.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="columnAlias">The alias for the column (optional).</param>
    /// <param name="condition">A function that determines whether to add the column, based on the column name. If <c>null</c>, the column is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Adds a column expression for each of the specified column names to the SELECT clause.
    /// </summary>
    /// <param name="columnNames">The collection of column names.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="columnNames"/> is <c>null</c>.</exception>
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
    /// Adds a COUNT aggregate expression using the specified column name to the SELECT clause.
    /// </summary>
    /// <param name="columnName">The name of the column (default is <c>*</c>).</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="columnAlias">The alias for the column (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Adds an aggregate expression using the specified function and column name to the SELECT clause.
    /// </summary>
    /// <param name="function">The aggregate function to use (e.g., <see cref="AggregateFunctions.Sum"/>).</param>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="columnAlias">The alias for the column (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Adds a FROM clause to the query.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="tableSchema">The schema of the table (optional).</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Adds a FROM clause to the query using the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Adds a raw FROM clause to the query.
    /// </summary>
    /// <param name="fromClause">The raw SQL FROM clause.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder FromRaw(string fromClause)
    {
        if (fromClause.HasValue())
            FromExpressions.Add(new TableExpression(fromClause, IsRaw: true));

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a JOIN clause to the query using the specified builder action.
    /// </summary>
    /// <param name="builder">An action that configures the join using a <see cref="JoinBuilder"/>.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder Join(Action<JoinBuilder> builder)
    {
        var innerBuilder = new JoinBuilder(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds an ORDER BY clause with the specified column name and sort direction.
    /// </summary>
    /// <param name="columnName">The name of the column to sort by.</param>
    /// <param name="sortDirection">The sort direction (default is <see cref="SortDirections.Ascending"/>).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder OrderBy(
        string columnName,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        return OrderBy(columnName, null, sortDirection);
    }

    /// <summary>
    /// Adds an ORDER BY clause with the specified column name, sort direction, and table alias.
    /// </summary>
    /// <param name="columnName">The name of the column to sort by.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="sortDirection">The sort direction (default is <see cref="SortDirections.Ascending"/>).</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Conditionally adds an ORDER BY clause with the specified column name and sort direction.
    /// </summary>
    /// <param name="columnName">The name of the column to sort by.</param>
    /// <param name="sortDirection">The sort direction (default is <see cref="SortDirections.Ascending"/>).</param>
    /// <param name="condition">A function that determines whether to add the ORDER BY clause, based on the column name. If <c>null</c>, the clause is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder OrderByIf(
        string columnName,
        SortDirections sortDirection = SortDirections.Ascending,
        Func<string, bool> condition = null)
    {
        return OrderByIf(columnName, null, sortDirection, condition);
    }

    /// <summary>
    /// Conditionally adds an ORDER BY clause with the specified column name, sort direction, and table alias.
    /// </summary>
    /// <param name="columnName">The name of the column to sort by.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="sortDirection">The sort direction (default is <see cref="SortDirections.Ascending"/>).</param>
    /// <param name="condition">A function that determines whether to add the ORDER BY clause, based on the column name. If <c>null</c>, the clause is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Adds a raw ORDER BY clause to the query.
    /// </summary>
    /// <param name="sortExpression">The raw SQL ORDER BY clause.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder OrderByRaw(string sortExpression)
    {
        if (sortExpression.HasValue())
            SortExpressions.Add(new SortExpression(sortExpression, IsRaw: true));

        return (TBuilder)this;
    }

    /// <summary>
    /// Conditionally adds a raw ORDER BY clause to the query.
    /// </summary>
    /// <param name="sortExpression">The raw SQL ORDER BY clause.</param>
    /// <param name="condition">A function that determines whether to add the ORDER BY clause, based on the expression. If <c>null</c>, the clause is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Adds multiple raw ORDER BY clauses to the query.
    /// </summary>
    /// <param name="sortExpressions">A collection of raw SQL ORDER BY clauses.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sortExpressions"/> is <c>null</c>.</exception>
    public TBuilder OrderByRaw(IEnumerable<string> sortExpressions)
    {
        if (sortExpressions is null)
            throw new ArgumentNullException(nameof(sortExpressions));

        foreach (var sortExpression in sortExpressions)
            SortExpressions.Add(new SortExpression(sortExpression, IsRaw: true));

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a GROUP BY clause with the specified column name.
    /// </summary>
    /// <param name="columnName">The name of the column to group by.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Adds a LIMIT clause with the specified offset and size.
    /// </summary>
    /// <param name="offset">The number of rows to skip before starting to return rows.</param>
    /// <param name="size">The maximum number of rows to return.</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Adds a LIMIT clause for paging with the specified page number and page size.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of rows per page.</param>
    /// <returns>
    /// The same builder instance for method chaining.
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

    /// <summary>
    /// Builds the SQL SELECT statement using the current configuration.
    /// </summary>
    /// <returns>
    /// A <see cref="QueryStatement"/> containing the SQL SELECT statement and its parameters.
    /// </returns>
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
