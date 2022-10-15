using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

public class LogicalBuilder : LogicalBuilder<LogicalBuilder>
{
    public LogicalBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

public abstract class LogicalBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : LogicalBuilder<TBuilder>
{
    protected LogicalBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    public override QueryStatement BuildStatement()
    {
        if (WhereExpressions.Count == 0)
            return null;

        var statement = QueryGenerator.LogicalExpression(WhereExpressions, LogicalOperator);

        return new QueryStatement(statement, Parameters);
    }
}
