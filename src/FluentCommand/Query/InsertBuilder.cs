using System;
using System.Collections.Generic;

using FluentCommand.Query.Generators;

namespace FluentCommand;

public class InsertBuilder : InsertBuilder<InsertBuilder>
{
    public InsertBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    { }
}

public abstract class InsertBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : InsertBuilder<TBuilder>
{
    protected InsertBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    protected HashSet<string> ColumnExpression { get; } = new();

    protected HashSet<string> OutputClause { get; } = new();

    protected HashSet<string> ValueExpression { get; } = new();

    protected string TableClause { get; private set; }


    public TBuilder Into(
        string tableName,
        string tableSchema = null)
    {
        TableClause = QueryGenerator.FromClause(tableName, tableSchema);

        return (TBuilder)this;
    }

    public TBuilder Value<TValue>(
        string columnName,
        TValue parameterValue)
    {
        return Value(columnName, parameterValue, typeof(TValue));
    }

    public TBuilder Value(
        string columnName,
        object parameterValue,
        Type parameterType)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException($"'{nameof(columnName)}' cannot be null or empty.", nameof(columnName));

        if (parameterType is null)
            throw new ArgumentNullException(nameof(parameterType));

        var paramterName = NextParameter();

        var columnExpression = QueryGenerator.SelectClause(columnName);

        ColumnExpression.Add(columnExpression);
        ValueExpression.Add(paramterName);

        Parameters.Add(new QueryParameter(paramterName, parameterValue, parameterType));

        return (TBuilder)this;
    }

    public TBuilder ValueIf<TValue>(
        string columnName,
        TValue parameterValue,
        Func<string, TValue, bool> condition)
    {
        if (condition != null && !condition(columnName, parameterValue))
            return (TBuilder)this;

        return Value(columnName, parameterValue);
    }


    public TBuilder Output(
        IEnumerable<string> columnNames,
        string prefix = "INSERTED")
    {
        if (columnNames is null)
            throw new ArgumentNullException(nameof(columnNames));

        foreach (var column in columnNames)
            Output(column, prefix);

        return (TBuilder)this;
    }

    public TBuilder Output(
        string columnName,
        string prefix = "INSERTED",
        string alias = null)
    {
        var outputClause = QueryGenerator.SelectClause(columnName, prefix, alias);

        OutputClause.Add(outputClause);

        return (TBuilder)this;
    }

    public TBuilder OutputIf(
        string columnName,
        string alias = null,
        Func<string, bool> condition = null)
    {
        if (condition != null && !condition(columnName))
            return (TBuilder)this;

        return Output(columnName, alias);
    }


    public override QueryStatement BuildStatement()
    {
        var statement = QueryGenerator.BuildInsert(
            tableClause: TableClause,
            columnExpression: ColumnExpression,
            outputClause: OutputClause,
            valueExpression: ValueExpression,
            commentExpression: CommentExpressions
        );

        return new QueryStatement(statement, Parameters);
    }

}
