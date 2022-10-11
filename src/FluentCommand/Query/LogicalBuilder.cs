using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

public class LogicalBuilder : LogicalBuilder<LogicalBuilder>
{
    public LogicalBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        List<string> comments,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, comments, logicalOperator)
    {
    }
}

public abstract class LogicalBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : LogicalBuilder<TBuilder>
{
    protected LogicalBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        List<string> comments,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
        CommentExpressions = comments;
    }

    public TBuilder WhereOr(Action<LogicalBuilder> builder)
    {
        var innerBuilder = new LogicalBuilder(QueryGenerator, Parameters, CommentExpressions, LogicalOperators.Or);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null)
            WhereClause.Add(statement.Statement);

        return (TBuilder)this;
    }

    public TBuilder WhereAnd(Action<LogicalBuilder> builder)
    {
        var innerBuilder = new LogicalBuilder(QueryGenerator, Parameters, CommentExpressions, LogicalOperators.And);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null)
            WhereClause.Add(statement.Statement);

        return (TBuilder)this;
    }


    public override QueryStatement BuildStatement()
    {
        if (WhereClause.Count == 0)
            return null;

        var statement = QueryGenerator.LogicalClause(WhereClause, LogicalOperator);

        return new QueryStatement(statement, Parameters);
    }

}