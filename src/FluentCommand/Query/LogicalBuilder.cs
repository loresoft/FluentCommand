using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing logical SQL query expressions (such as grouped AND/OR conditions) with fluent, chainable methods.
/// </summary>
public class LogicalBuilder : LogicalBuilder<LogicalBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogicalBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    /// <param name="logicalOperator">The logical operator (<see cref="LogicalOperators"/>) to combine WHERE expressions. Defaults to <see cref="LogicalOperators.And"/>.</param>
    public LogicalBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

/// <summary>
/// Provides a generic base class for building logical SQL query expressions (such as grouped AND/OR conditions) with fluent, chainable methods.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder for fluent chaining.</typeparam>
public abstract class LogicalBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : LogicalBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogicalBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    /// <param name="logicalOperator">The logical operator (<see cref="LogicalOperators"/>) to combine WHERE expressions. Defaults to <see cref="LogicalOperators.And"/>.</param>
    protected LogicalBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    /// <summary>
    /// Builds the logical SQL expression using the current WHERE expressions and logical operator.
    /// </summary>
    /// <returns>
    /// A <see cref="QueryStatement"/> containing the logical SQL expression and its parameters,
    /// or <c>null</c> if no WHERE expressions are present.
    /// </returns>
    public override QueryStatement BuildStatement()
    {
        if (WhereExpressions.Count == 0)
            return null;

        var statement = QueryGenerator.LogicalExpression(WhereExpressions, LogicalOperator);

        return new QueryStatement(statement, Parameters);
    }
}
