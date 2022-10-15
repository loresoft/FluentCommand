using FluentCommand.Extensions;
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


    protected HashSet<ColumnExpression> OutputExpressions { get; } = new();

    protected HashSet<TableExpression> FromExpressions { get; } = new();

    protected HashSet<JoinExpression> JoinExpressions { get; } = new();

    protected TableExpression TableExpression { get; private set; }


    public TBuilder Table(
        string tableName,
        string tableSchema = null,
        string tableAlias = null)
    {
        TableExpression = new TableExpression(tableName, tableSchema, tableAlias);

        return (TBuilder)this;
    }


    public TBuilder Output(
        IEnumerable<string> columnNames,
        string tableAlias = "DELETED")
    {
        if (columnNames is null)
            throw new ArgumentNullException(nameof(columnNames));

        foreach (var column in columnNames)
            Output(column, tableAlias);

        return (TBuilder)this;
    }

    public TBuilder Output(
        string columnName,
        string tableAlias = "DELETED",
        string columnAlias = null)
    {
        var outputClause = new ColumnExpression(columnName, tableAlias, columnAlias);

        OutputExpressions.Add(outputClause);

        return (TBuilder)this;
    }

    public TBuilder OutputIf(
        string columnName,
        string tableAlias = "DELETED",
        string columnAlias = null,
        Func<string, bool> condition = null)
    {
        if (condition != null && !condition(columnName))
            return (TBuilder)this;

        return Output(columnName, tableAlias, columnAlias);
    }


    public virtual TBuilder From(
        string tableName,
        string tableSchema = null,
        string tableAlias = null)
    {
        var fromClause = new TableExpression(tableName, tableSchema, tableAlias);

        FromExpressions.Add(fromClause);

        return (TBuilder)this;
    }

    public TBuilder FromRaw(string fromClause)
    {
        if (fromClause.HasValue())
            FromExpressions.Add(new TableExpression(fromClause, IsRaw: true));

        return (TBuilder)this;
    }

    public TBuilder Join(Action<JoinBuilder> builder)
    {
        var innerBuilder = new JoinBuilder(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return (TBuilder)this;
    }


    public override QueryStatement BuildStatement()
    {
        var deleteStatement = new DeleteStatement(
            TableExpression,
            OutputExpressions,
            FromExpressions,
            JoinExpressions,
            WhereExpressions,
            CommentExpressions);

        var statement = QueryGenerator.BuildDelete(deleteStatement);

        return new QueryStatement(statement, Parameters);
    }
}
