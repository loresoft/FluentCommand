using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Where clause builder
/// </summary>
public class WhereBuilder : WhereBuilder<WhereBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WhereBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="logicalOperator">The logical operator.</param>
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
/// Where clause builder
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public abstract class WhereBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : WhereBuilder<TBuilder>

{
    /// <summary>
    /// Initializes a new instance of the <see cref="WhereBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    /// <param name="logicalOperator">The logical operator.</param>
    protected WhereBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters)
    {
        LogicalOperator = logicalOperator;
    }

    /// <summary>
    /// Gets the where expressions.
    /// </summary>
    /// <value>
    /// The where expressions.
    /// </value>
    protected HashSet<WhereExpression> WhereExpressions { get; } = new();

    /// <summary>
    /// Gets the logical operator.
    /// </summary>
    /// <value>
    /// The logical operator.
    /// </value>
    protected LogicalOperators LogicalOperator { get; }

    /// <summary>
    /// Create a where clause with the specified column, value and operator
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="filterOperator">The filter operator.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder Where<TValue>(
       string columnName,
       TValue parameterValue,
       FilterOperators filterOperator = FilterOperators.Equal)
    {
        return Where(columnName, parameterValue, null, filterOperator);
    }

    /// <summary>
    /// Create a where clause with the specified column, value, operator and table alias
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="filterOperator">The filter operator.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Create a where in clause with the specified column, values and table alias
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="parameterValues">The parameter values.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Conditionally create a where in clause with the specified column and values
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="parameterValues">The parameter values.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Conditionally create a where in clause with the specified column, values and table alias
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="parameterValues">The parameter values.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Conditionally create a where clause with the specified column, value and operator
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="filterOperator">The filter operator.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Conditionally create a where clause with the specified property, value, operator and table alias
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="filterOperator">The filter operator.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Create a where clause with the specified raw query
    /// </summary>
    /// <param name="whereClause">The where clause.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns></returns>
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
    /// Conditionally create a where clause with the specified raw query
    /// </summary>
    /// <param name="whereClause">The where clause.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="condition">The condition.</param>
    /// <returns></returns>
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
    /// Create a logical AND where clause group
    /// </summary>
    /// <param name="builder">The logical AND where clause builder.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Create a logical OR where clause group
    /// </summary>
    /// <param name="builder">The logical OR where clause builder.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
