using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Query order by builder
/// </summary>
public class OrderBuilder : OrderBuilder<OrderBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    public OrderBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }
}

/// <summary>
/// Query order by builder
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public abstract class OrderBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : OrderBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    protected OrderBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Gets the sort expressions.
    /// </summary>
    /// <value>
    /// The sort expressions.
    /// </value>
    protected HashSet<SortExpression> SortExpressions { get; } = new();


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
        string tableAlias = null,
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
    public TBuilder OrderByRawIf(string sortExpression, Func<string, bool> condition = null)
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
            OrderByRaw(sortExpression);

        return (TBuilder)this;
    }

    /// <inheritdoc />
    public override QueryStatement BuildStatement()
    {
        if (SortExpressions == null || SortExpressions.Count == 0)
            return null;

        var statement = QueryGenerator.BuildOrder(SortExpressions);

        return new QueryStatement(statement, Parameters);
    }
}
