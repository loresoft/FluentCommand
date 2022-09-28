using System;
using System.Collections.Generic;

using FluentCommand.Query.Generators;

namespace FluentCommand;

public class DeleteBuilder : DeleteBuilder<DeleteBuilder>
{
    public DeleteBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

public abstract class DeleteBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : DeleteBuilder<TBuilder>
{
    protected DeleteBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }


    public HashSet<string> OutputClause { get; } = new();

    public HashSet<string> FromClause { get; } = new();

    public string TableClause { get; private set; }


    public TBuilder Table(string tableName, string tableSchema = null)
    {
        TableClause = QueryGenerator.FromClause(tableName, tableSchema);

        return (TBuilder)this;
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
        var statement = QueryGenerator.BuildDelete(
            tableClause: TableClause,
            outputClause: OutputClause,
            fromClause: FromClause,
            whereClause: WhereClause,
            commentExpression: CommentExpressions
        );

        return new QueryStatement(statement, Parameters);
    }

}
