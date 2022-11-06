using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

public class WhereBuilder : WhereBuilder<WhereBuilder>
{
    public WhereBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    public override QueryStatement BuildStatement()
    {
        var statement = QueryGenerator.BuildWhere(
            whereExpressions: WhereExpressions
        );

        return new QueryStatement(statement, Parameters);
    }
}

public abstract class WhereBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : WhereBuilder<TBuilder>

{
    protected WhereBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters)
    {
        LogicalOperator = logicalOperator;
    }

    protected HashSet<WhereExpression> WhereExpressions { get; } = new();

    protected LogicalOperators LogicalOperator { get; }

    public TBuilder Where<TValue>(
       string columnName,
       TValue parameterValue,
       FilterOperators filterOperator = FilterOperators.Equal)
    {
        return Where(columnName, parameterValue, null, filterOperator);
    }

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

    public TBuilder WhereIf<TValue>(
        string columnName,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        return WhereIf(columnName, parameterValue, null, filterOperator, condition);
    }

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

    public TBuilder WhereRawIf(
        string whereClause,
        IEnumerable<QueryParameter> parameters = null,
        Func<string, IEnumerable<QueryParameter>, bool> condition = null)
    {
        if (condition != null && !condition(whereClause, parameters))
            return (TBuilder)this;

        return WhereRaw(whereClause, parameters);
    }

    public TBuilder WhereOr(Action<LogicalBuilder> builder)
    {
        var innerBuilder = new LogicalBuilder(QueryGenerator, Parameters, LogicalOperators.Or);
        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null && statement.Statement.HasValue())
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return (TBuilder)this;
    }

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
