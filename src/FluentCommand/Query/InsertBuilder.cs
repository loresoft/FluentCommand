using System;
using System.Collections.Generic;

using FluentCommand.Query.Generators;

namespace FluentCommand;

public class InsertBuilder : InsertBuilder<InsertBuilder>
{
    public InsertBuilder(IQueryGenerator queryGenerator, Dictionary<string, object> parameters)
        : base(queryGenerator, parameters)
    { }
}

public abstract class InsertBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : InsertBuilder<TBuilder>
{
    protected InsertBuilder(IQueryGenerator queryGenerator, Dictionary<string, object> parameters)
        : base(queryGenerator, parameters)
    {
    }

    public HashSet<string> ColumnExpression { get; } = new();

    public HashSet<string> OutputClause { get; } = new();

    public HashSet<string> ValueExpression { get; } = new();

    public HashSet<string> IntoClause { get; } = new();


    public TBuilder Into(string tableName, string tableSchema = null)
    {
        var fromClause = QueryGenerator.FromClause(tableName, tableSchema);

        IntoClause.Add(fromClause);

        return (TBuilder)this;
    }


    public TBuilder Value(string columnName, object parameterValue)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException($"'{nameof(columnName)}' cannot be null or empty.", nameof(columnName));

        var paramterName = NextParameter();

        var columnExpression = QueryGenerator.SelectClause(columnName);

        ColumnExpression.Add(columnExpression);
        ValueExpression.Add(paramterName);

        // null as DBNull for ado
        Parameters[paramterName] = parameterValue ?? DBNull.Value;

        return (TBuilder)this;
    }

    public TBuilder ValueIf(string columnName, object parameterValue, Func<string, object, bool> condition)
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


    public override QueryStatement BuildStatement()
    {
        var statement = QueryGenerator.BuildInsert(
            intoClause: IntoClause,
            columnExpression: ColumnExpression,
            outputClause: OutputClause,
            valueExpression: ValueExpression,
            commentExpression: CommentExpressions
        );

        return new QueryStatement(statement, Parameters);
    }

}
