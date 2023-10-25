using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// A logical query expression builder 
/// </summary>
public class LogicalBuilder : LogicalBuilder<LogicalBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogicalBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    /// <param name="logicalOperator">The query logical operator.</param>
    public LogicalBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

/// <summary>
/// A logical query expression builder 
/// </summary>
public abstract class LogicalBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : LogicalBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogicalBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    /// <param name="logicalOperator">The query logical operator.</param>
    protected LogicalBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    /// <inheritdoc />
    public override QueryStatement BuildStatement()
    {
        if (WhereExpressions.Count == 0)
            return null;

        var statement = QueryGenerator.LogicalExpression(WhereExpressions, LogicalOperator);

        return new QueryStatement(statement, Parameters);
    }
}
