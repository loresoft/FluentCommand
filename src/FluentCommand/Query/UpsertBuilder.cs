using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

public class UpsertBuilder : UpsertBuilder<UpsertBuilder>
{
    public UpsertBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

public abstract class UpsertBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : UpsertBuilder<TBuilder>
{
    protected UpsertBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }


    protected HashSet<UpsertExpression> UpsertExpressions { get; } = new();

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
        TValue parameterValue,
        UpsertType upsertType = UpsertType.InsertUpdate)
    {
        return Value(columnName, parameterValue, typeof(TValue), upsertType);
    }

    public TBuilder Value(
        string columnName,
        object parameterValue,
        Type parameterType,
        UpsertType upsertType = UpsertType.InsertUpdate)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException($"'{nameof(columnName)}' cannot be null or empty.", nameof(columnName));

        var paramterName = NextParameter();
        var updateClause = new UpsertExpression(columnName, paramterName, UpsertType: upsertType);

        UpsertExpressions.Add(updateClause);
        Parameters.Add(new QueryParameter(paramterName, parameterValue, parameterType));

        return (TBuilder)this;
    }

    public TBuilder ValueIf<TValue>(
        string columnName,
        TValue parameterValue,
        Func<string, TValue, bool> condition,
        UpsertType upsertType = UpsertType.InsertUpdate)
    {
        if (condition != null && !condition(columnName, parameterValue))
            return (TBuilder)this;

        return Value(columnName, parameterValue, upsertType);
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


    public override QueryStatement BuildStatement()
    {
        var updateStatement = new UpsertStatement(
            TableExpression,
            UpsertExpressions,
            OutputExpressions,
            FromExpressions,
            JoinExpressions,
            WhereExpressions,
            CommentExpressions);

        var statement = QueryGenerator.BuildUpsert(updateStatement);

        return new QueryStatement(statement, Parameters);
    }

}
