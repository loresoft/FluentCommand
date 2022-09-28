using System;
using System.Collections.Generic;

using FluentCommand.Query.Generators;

namespace FluentCommand;

public class UpdateBuilder : UpdateBuilder<UpdateBuilder>
{
    public UpdateBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

public abstract class UpdateBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : UpdateBuilder<TBuilder>
{
    protected UpdateBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }


    public HashSet<string> UpdateClause { get; } = new();

    public HashSet<string> OutputClause { get; } = new();

    public HashSet<string> FromClause { get; } = new();

    public string TableClause { get; private set; }


    public TBuilder Table(string tableName, string tableSchema = null)
    {
        TableClause = QueryGenerator.FromClause(tableName, tableSchema);

        return (TBuilder)this;
    }


    public TBuilder Value<TValue>(string columnName, TValue parameterValue)
    {
        return Value(columnName, parameterValue, typeof(TValue));
    }

    public TBuilder Value(string columnName, object parameterValue, Type parameterType)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException($"'{nameof(columnName)}' cannot be null or empty.", nameof(columnName));

        var paramterName = NextParameter();
        var updateClause = QueryGenerator.UpdateClause(columnName, paramterName);

        UpdateClause.Add(updateClause);
        Parameters.Add(new QueryParameter(paramterName, parameterValue, parameterType));

        return (TBuilder)this;
    }

    public TBuilder ValueIf<TValue>(string columnName, TValue parameterValue, Func<string, TValue, bool> condition)
    {
        if (condition != null && !condition(columnName, parameterValue))
            return (TBuilder)this;

        return Value(columnName, parameterValue);
    }


    public TBuilder Output(IEnumerable<string> columnNames, string prefix = "INSERTED")
    {
        if (columnNames is null)
            throw new ArgumentNullException(nameof(columnNames));

        foreach (var column in columnNames)
            Output(column, prefix);

        return (TBuilder)this;
    }

    public TBuilder Output(string columnName, string prefix = "INSERTED", string alias = null)
    {
        var outputClause = QueryGenerator.SelectClause(columnName, prefix, alias);

        OutputClause.Add(outputClause);

        return (TBuilder)this;
    }

    public TBuilder OutputIf(string columnName, string alias = null, Func<string, bool> condition = null)
    {
        if (condition != null && !condition(columnName))
            return (TBuilder)this;

        return Output(columnName, alias);
    }



    public TBuilder From(string tableName, string tableSchema = null, string alias = null)
    {
        var fromClause = QueryGenerator.FromClause(tableName, tableSchema, alias);

        FromClause.Add(fromClause);

        return (TBuilder)this;
    }


    public TBuilder Where(Action<LogicalBuilder> builder)
    {
        var innerBuilder = new LogicalBuilder(QueryGenerator, Parameters, CommentExpressions, LogicalOperators.And);
        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();
        WhereClause.Add(statement.Statement);

        return (TBuilder)this;
    }

    public override QueryStatement BuildStatement()
    {
        var statement = QueryGenerator.BuildUpdate(
            tableClause: TableClause,
            updateClause: UpdateClause,
            outputClause: OutputClause,
            fromClause: FromClause,
            whereClause: WhereClause,
            commentExpression: CommentExpressions
        );

        return new QueryStatement(statement, Parameters);
    }

}
