using System;
using System.Collections.Generic;

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

    public TBuilder Where(
        string whereClause,
        IList<QueryParameter> parametes = null)
    {
        if (string.IsNullOrWhiteSpace(whereClause))
            throw new ArgumentException($"'{nameof(whereClause)}' cannot be null or empty.", nameof(whereClause));

        WhereClause.Add(whereClause);

        if (parametes != null)
            Parameters.AddRange(parametes);

        return (TBuilder)this;
    }
}
