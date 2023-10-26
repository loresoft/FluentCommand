using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Insert query statement builder
/// </summary>
public class InsertBuilder : InsertBuilder<InsertBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InsertBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The parameters.</param>
    public InsertBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    { }
}

/// <summary>
/// Insert query statement builder
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public abstract class InsertBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : InsertBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InsertBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    protected InsertBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Gets the column expressions.
    /// </summary>
    /// <value>
    /// The column expressions.
    /// </value>
    protected HashSet<ColumnExpression> ColumnExpressions { get; } = new();

    /// <summary>
    /// Gets the output expressions.
    /// </summary>
    /// <value>
    /// The output expressions.
    /// </value>
    protected HashSet<ColumnExpression> OutputExpressions { get; } = new();

    /// <summary>
    /// Gets the value expressions.
    /// </summary>
    /// <value>
    /// The value expressions.
    /// </value>
    protected HashSet<string> ValueExpressions { get; } = new();

    /// <summary>
    /// Gets the table expression.
    /// </summary>
    /// <value>
    /// The table expression.
    /// </value>
    protected TableExpression TableExpression { get; private set; }


    /// <summary>
    /// Set the target table to insert into.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="tableSchema">The table schema.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder Into(
        string tableName,
        string tableSchema = null,
        string tableAlias = null)
    {
        TableExpression = new TableExpression(tableName, tableSchema, tableAlias);

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a value with specified column name and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="parameterValue">The parameter value.</param>
    public TBuilder Value<TValue>(
        string columnName,
        TValue parameterValue)
    {
        return Value(columnName, parameterValue, typeof(TValue));
    }

    /// <summary>
    /// Adds a value with specified column name and value.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="parameterType">Type of the parameter.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
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

    /// <summary>
    /// Conditionally adds a value with specified column name and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder ValueIf<TValue>(
        string columnName,
        TValue parameterValue,
        Func<string, TValue, bool> condition)
    {
        if (condition != null && !condition(columnName, parameterValue))
            return (TBuilder)this;

        return Value(columnName, parameterValue);
    }


    /// <summary>
    /// Add an output clause for the specified column names.
    /// </summary>
    /// <param name="columnNames">The column names.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
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

    /// <summary>
    /// Add an output clause for the specified column name.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder Output(
        string columnName,
        string tableAlias = "INSERTED",
        string columnAlias = null)
    {
        var outputClause = new ColumnExpression(columnName, tableAlias, columnAlias);

        OutputExpressions.Add(outputClause);

        return (TBuilder)this;
    }

    /// <summary>
    /// Conditionally add an output clause for the specified column name.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
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


    /// <inheritdoc />
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
