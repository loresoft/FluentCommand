using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

public class UpdateBuilder : UpdateBuilder<UpdateBuilder>
{
    public UpdateBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

public abstract class UpdateBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : UpdateBuilder<TBuilder>
{
    protected UpdateBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }


    protected HashSet<UpdateExpression> UpdateExpressions { get; } = new();

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

        var paramterName = NextParameter();
        var updateClause = new UpdateExpression(columnName, paramterName);

        UpdateExpressions.Add(updateClause);
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
        string tableAlias = "INSERTED")
    {
        if (columnNames is null)
            throw new ArgumentNullException(nameof(columnNames));

        foreach (var column in columnNames)
            Output(column, tableAlias);

        return (TBuilder)this;
    }

    public TBuilder Output(
        string columnName,
        string tableAlias = "INSERTED",
        string columnAlias = null)
    {
        var outputClause = new ColumnExpression(columnName, tableAlias, columnAlias);

        OutputExpressions.Add(outputClause);

        return (TBuilder)this;
    }

    public TBuilder OutputIf(
        string columnName,
        string tableAlias = "INSERTED",
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
        var updateStatement = new UpdateStatement(
            TableExpression,
            UpdateExpressions,
            OutputExpressions,
            FromExpressions,
            JoinExpressions,
            WhereExpressions,
            CommentExpressions);

        var statement = QueryGenerator.BuildUpdate(updateStatement);

        return new QueryStatement(statement, Parameters);
    }

}
