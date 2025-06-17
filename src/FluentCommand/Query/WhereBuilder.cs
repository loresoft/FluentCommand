using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL WHERE clauses with fluent, chainable methods.
/// </summary>
public class WhereBuilder : WhereBuilder<WhereBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WhereBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    /// <param name="logicalOperator">The logical operator (<see cref="LogicalOperators"/>) to combine WHERE expressions. Defaults to <see cref="LogicalOperators.And"/>.</param>
    public WhereBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    /// <inheritdoc />
    public override QueryStatement BuildStatement()
    {
        var statement = QueryGenerator.BuildWhere(
            whereExpressions: WhereExpressions
        );

        return new QueryStatement(statement, Parameters);
    }
}

/// <summary>
/// Provides a generic base class for building SQL WHERE clauses with fluent, chainable methods.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder for fluent chaining.</typeparam>
public abstract class WhereBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : WhereBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WhereBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    /// <param name="logicalOperator">The logical operator (<see cref="LogicalOperators"/>) to combine WHERE expressions. Defaults to <see cref="LogicalOperators.And"/>.</param>
    protected WhereBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters)
    {
        LogicalOperator = logicalOperator;
    }

    /// <summary>
    /// Gets the collection of WHERE expressions for the query.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{WhereExpression}"/> containing the WHERE expressions.
    /// </value>
    protected HashSet<WhereExpression> WhereExpressions { get; } = new();

    /// <summary>
    /// Gets the logical operator used to combine WHERE expressions.
    /// </summary>
    /// <value>
    /// The <see cref="LogicalOperators"/> value.
    /// </value>
    protected LogicalOperators LogicalOperator { get; }

    /// <summary>
    /// Adds a WHERE clause for the specified column, value, and operator.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="parameterValue">The value to compare.</param>
    /// <param name="filterOperator">The filter operator (<see cref="FilterOperators"/>). Defaults to <see cref="FilterOperators.Equal"/>.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder Where<TValue>(
       string columnName,
       TValue parameterValue,
       FilterOperators filterOperator = FilterOperators.Equal)
    {
        return Where(columnName, parameterValue, null, filterOperator);
    }

    /// <summary>
    /// Adds a WHERE clause for the specified column, value, operator, and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="parameterValue">The value to compare.</param>
    /// <param name="tableAlias">The table alias, or <c>null</c> if not applicable.</param>
    /// <param name="filterOperator">The filter operator (<see cref="FilterOperators"/>). Defaults to <see cref="FilterOperators.Equal"/>.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder Where<TValue>(
        string columnName,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        var parameterName = NextParameter();

        WhereExpressions.Add(new WhereExpression(columnName, parameterName, tableAlias, filterOperator));
        Parameters.Add(new QueryParameter(parameterName, parameterValue, typeof(TValue)));

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a WHERE IN clause for the specified column, values, and optional table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="parameterValues">The collection of values for the IN clause.</param>
    /// <param name="tableAlias">The table alias, or <c>null</c> if not applicable.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder WhereIn<TValue>(
        string columnName,
        IEnumerable<TValue> parameterValues,
        string tableAlias = null)
    {
        var parameterNames = new List<string>();
        foreach (var parameterValue in parameterValues)
        {
            var parameterName = NextParameter();
            var parameter = new QueryParameter(parameterName, parameterValue, typeof(TValue));

            Parameters.Add(parameter);
            parameterNames.Add(parameterName);
        }

        var whereParameter = parameterNames.ToDelimitedString();

        WhereExpressions.Add(new WhereExpression(columnName, whereParameter, tableAlias, FilterOperators.In));

        return (TBuilder)this;
    }

    /// <summary>
    /// Conditionally adds a WHERE IN clause for the specified column and values if the condition is met.
    /// </summary>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="parameterValues">The collection of values for the IN clause.</param>
    /// <param name="condition">A function that determines whether to add the clause. If <c>null</c>, the clause is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder WhereInIf<TValue>(
        string columnName,
        IEnumerable<TValue> parameterValues,
        Func<string, IEnumerable<TValue>, bool> condition = null)
    {
        if (condition != null && !condition(columnName, parameterValues))
            return (TBuilder)this;

        return WhereIn(columnName, parameterValues);
    }

    /// <summary>
    /// Conditionally adds a WHERE IN clause for the specified column, values, and table alias if the condition is met.
    /// </summary>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="parameterValues">The collection of values for the IN clause.</param>
    /// <param name="tableAlias">The table alias, or <c>null</c> if not applicable.</param>
    /// <param name="condition">A function that determines whether to add the clause. If <c>null</c>, the clause is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder WhereInIf<TValue>(
        string columnName,
        IEnumerable<TValue> parameterValues,
        string tableAlias,
        Func<string, IEnumerable<TValue>, bool> condition = null)
    {
        if (condition != null && !condition(columnName, parameterValues))
            return (TBuilder)this;

        return WhereIn(columnName, parameterValues, tableAlias);
    }

    /// <summary>
    /// Conditionally adds a WHERE clause for the specified column, value, and operator if the condition is met.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="parameterValue">The value to compare.</param>
    /// <param name="filterOperator">The filter operator (<see cref="FilterOperators"/>). Defaults to <see cref="FilterOperators.Equal"/>.</param>
    /// <param name="condition">A function that determines whether to add the clause. If <c>null</c>, the clause is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder WhereIf<TValue>(
        string columnName,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        return WhereIf(columnName, parameterValue, null, filterOperator, condition);
    }

    /// <summary>
    /// Conditionally adds a WHERE clause for the specified column, value, operator, and table alias if the condition is met.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="parameterValue">The value to compare.</param>
    /// <param name="tableAlias">The table alias, or <c>null</c> if not applicable.</param>
    /// <param name="filterOperator">The filter operator (<see cref="FilterOperators"/>). Defaults to <see cref="FilterOperators.Equal"/>.</param>
    /// <param name="condition">A function that determines whether to add the clause. If <c>null</c>, the clause is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder WhereIf<TValue>(
        string columnName,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        if (condition != null && !condition(columnName, parameterValue))
            return (TBuilder)this;

        return Where(columnName, parameterValue, tableAlias, filterOperator);
    }

    /// <summary>
    /// Adds a raw WHERE clause to the query.
    /// </summary>
    /// <param name="whereClause">The raw SQL WHERE clause.</param>
    /// <param name="parameters">The collection of <see cref="QueryParameter"/> objects for the clause, or <c>null</c> if none.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="whereClause"/> is null or empty.</exception>
    public TBuilder WhereRaw(
        string whereClause,
        IEnumerable<QueryParameter> parameters = null)
    {
        if (string.IsNullOrWhiteSpace(whereClause))
            throw new ArgumentException($"'{nameof(whereClause)}' cannot be null or empty.", nameof(whereClause));

        WhereExpressions.Add(new WhereExpression(whereClause, IsRaw: true));

        if (parameters != null)
            Parameters.AddRange(parameters);

        return (TBuilder)this;
    }

    /// <summary>
    /// Conditionally adds a raw WHERE clause to the query if the condition is met.
    /// </summary>
    /// <param name="whereClause">The raw SQL WHERE clause.</param>
    /// <param name="parameters">The collection of <see cref="QueryParameter"/> objects for the clause, or <c>null</c> if none.</param>
    /// <param name="condition">A function that determines whether to add the clause. If <c>null</c>, the clause is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder WhereRawIf(
        string whereClause,
        IEnumerable<QueryParameter> parameters = null,
        Func<string, IEnumerable<QueryParameter>, bool> condition = null)
    {
        if (condition != null && !condition(whereClause, parameters))
            return (TBuilder)this;

        return WhereRaw(whereClause, parameters);
    }

    /// <summary>
    /// Adds a logical OR group of WHERE clauses using the specified builder action.
    /// </summary>
    /// <param name="builder">An action that configures the logical OR group using a <see cref="LogicalBuilder"/>.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder WhereOr(Action<LogicalBuilder> builder)
    {
        var innerBuilder = new LogicalBuilder(QueryGenerator, Parameters, LogicalOperators.Or);
        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null && statement.Statement.HasValue())
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a logical AND group of WHERE clauses using the specified builder action.
    /// </summary>
    /// <param name="builder">An action that configures the logical AND group using a <see cref="LogicalBuilder"/>.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder WhereAnd(Action<LogicalBuilder> builder)
    {
        var innerBuilder = new LogicalBuilder(QueryGenerator, Parameters, LogicalOperators.And);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null && statement.Statement.HasValue())
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return (TBuilder)this;
    }
}
