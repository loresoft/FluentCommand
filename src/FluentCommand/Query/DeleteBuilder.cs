using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

public class DeleteBuilder : DeleteBuilder<DeleteBuilder>
{
    public DeleteBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

public abstract class DeleteBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : DeleteBuilder<TBuilder>
{
    protected DeleteBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }


    protected HashSet<string> OutputClause { get; } = new();

    protected HashSet<string> FromClause { get; } = new();

    protected string TableClause { get; private set; }


    public TBuilder Table(
        string tableName,
        string tableSchema = null,
        string tableAlias = null)
    {
        TableClause = QueryGenerator.FromClause(tableName, tableSchema, tableAlias);

        return (TBuilder)this;
    }


    public TBuilder Output(
        IEnumerable<string> columnNames,
        string columnPrefix = "DELETED")
    {
        if (columnNames is null)
            throw new ArgumentNullException(nameof(columnNames));

        foreach (var column in columnNames)
            Output(column, columnPrefix);

        return (TBuilder)this;
    }

    public TBuilder Output(
        string columnName,
        string columnPrefix = "DELETED",
        string columnAlias = null)
    {
        var outputClause = QueryGenerator.SelectClause(columnName, columnPrefix, columnAlias);

        OutputClause.Add(outputClause);

        return (TBuilder)this;
    }

    public TBuilder OutputIf(
        string columnName,
        string columnPrefix = "DELETED",
        string columnAlias = null,
        Func<string, bool> condition = null)
    {
        if (condition != null && !condition(columnName))
            return (TBuilder)this;

        return Output(columnName, columnPrefix, columnAlias);
    }


    public TBuilder From(
        string tableName,
        string tableSchema = null,
        string tableAlias = null)
    {
        var fromClause = QueryGenerator.FromClause(tableName, tableSchema, tableAlias);

        FromClause.Add(fromClause);

        return (TBuilder)this;
    }


    public TBuilder Where(Action<LogicalBuilder> builder)
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
