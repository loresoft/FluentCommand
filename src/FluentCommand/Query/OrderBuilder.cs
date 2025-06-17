using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL ORDER BY clauses with fluent, chainable methods.
/// </summary>
public class OrderBuilder : OrderBuilder<OrderBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    public OrderBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }
}

/// <summary>
/// Provides a generic base class for building SQL ORDER BY clauses with fluent, chainable methods.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder for fluent chaining.</typeparam>
public abstract class OrderBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : OrderBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    protected OrderBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Gets the collection of sort expressions for the ORDER BY clause.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{SortExpression}"/> containing the sort expressions.
    /// </value>
    protected HashSet<SortExpression> SortExpressions { get; } = new();

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
        string tableAlias = null,
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
    public TBuilder OrderByRawIf(string sortExpression, Func<string, bool> condition = null)
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
            OrderByRaw(sortExpression);

        return (TBuilder)this;
    }

    /// <summary>
    /// Builds the SQL ORDER BY statement using the current sort expressions.
    /// </summary>
    /// <returns>
    /// A <see cref="QueryStatement"/> containing the SQL ORDER BY clause and its parameters,
    /// or <c>null</c> if no sort expressions are present.
    /// </returns>
    public override QueryStatement BuildStatement()
    {
        if (SortExpressions == null || SortExpressions.Count == 0)
            return null;

        var statement = QueryGenerator.BuildOrder(SortExpressions);

        return new QueryStatement(statement, Parameters);
    }
}
