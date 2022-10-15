using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

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

    protected HashSet<ColumnExpression> ColumnExpressions { get; } = new();

    protected HashSet<ColumnExpression> OutputExpressions { get; } = new();

    protected HashSet<string> ValueExpressions { get; } = new();

    protected TableExpression TableExpression { get; private set; }


    public TBuilder Into(
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

        if (parameterType is null)
            throw new ArgumentNullException(nameof(parameterType));

        var paramterName = NextParameter();

        var columnExpression = new ColumnExpression(columnName);

        ColumnExpressions.Add(columnExpression);
        ValueExpressions.Add(paramterName);

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


    public override QueryStatement BuildStatement()
    {
        var insertStatement = new InsertStatement(
            TableExpression,
            ColumnExpressions,
            OutputExpressions,
            ValueExpressions,
            CommentExpressions);

        var statement = QueryGenerator.BuildInsert(insertStatement);

        return new QueryStatement(statement, Parameters);
    }

}
