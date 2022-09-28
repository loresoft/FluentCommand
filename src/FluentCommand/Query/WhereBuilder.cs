using System;
using System.Collections.Generic;

using FluentCommand.Query.Generators;

namespace FluentCommand;

public abstract class WhereBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : WhereBuilder<TBuilder>

{
    protected WhereBuilder(IQueryGenerator queryGenerator, Dictionary<string, object> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters)
    {
        LogicalOperator = logicalOperator;
    }

    public HashSet<string> WhereClause { get; internal set; } = new();

    public LogicalOperators LogicalOperator { get; }

    public TBuilder Where(
        string columnName,
        object parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        var paramterName = NextParameter();
        var whereClause = QueryGenerator.WhereClause(columnName, paramterName, filterOperator);

        WhereClause.Add(whereClause);
        Parameters[paramterName] = parameterValue ?? DBNull.Value;

        return (TBuilder)this;
    }

    public TBuilder Where(
        string whereClause,
        IDictionary<string, object> parametes = null)
    {
        if (string.IsNullOrWhiteSpace(whereClause))
            throw new ArgumentException($"'{nameof(whereClause)}' cannot be null or empty.", nameof(whereClause));

        WhereClause.Add(whereClause);

        if (parametes != null)
            foreach (var parameter in parametes)
                Parameters[parameter.Key] = parameter.Value;

        return (TBuilder)this;
    }
}
