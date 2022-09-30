using FluentCommand.Query.Generators;

namespace FluentCommand;

public abstract class WhereBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : WhereBuilder<TBuilder>

{
    protected WhereBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters)
    {
        LogicalOperator = logicalOperator;
    }

    protected HashSet<string> WhereClause { get; } = new();

    protected LogicalOperators LogicalOperator { get; }

    public TBuilder Where<TValue>(
        string columnName,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        var paramterName = NextParameter();
        var whereClause = QueryGenerator.WhereClause(columnName, paramterName, filterOperator);

        WhereClause.Add(whereClause);
        Parameters.Add(new QueryParameter(paramterName, parameterValue, typeof(TValue)));

        return (TBuilder)this;
    }

    public TBuilder WhereIf<TValue>(
        string columnName,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        if (condition != null && !condition(columnName, parameterValue))
            return (TBuilder)this;

        return Where(columnName, parameterValue, filterOperator);
    }

    public TBuilder WhereRaw(
        string whereClause,
        IEnumerable<QueryParameter> parametes = null)
    {
        if (string.IsNullOrWhiteSpace(whereClause))
            throw new ArgumentException($"'{nameof(whereClause)}' cannot be null or empty.", nameof(whereClause));

        WhereClause.Add(whereClause);

        if (parametes != null)
            Parameters.AddRange(parametes);

        return (TBuilder)this;
    }

    public TBuilder WhereRawIf(
        string whereClause,
        IEnumerable<QueryParameter> parametes = null,
        Func<string, IEnumerable<QueryParameter>, bool> condition = null)
    {
        if (condition != null && !condition(whereClause, parametes))
            return (TBuilder)this;

        return WhereRaw(whereClause, parametes);
    }
}
