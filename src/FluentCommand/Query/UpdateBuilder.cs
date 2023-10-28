using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Update query statement builder
/// </summary>
public class UpdateBuilder : UpdateBuilder<UpdateBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="logicalOperator">The logical operator.</param>
    public UpdateBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

/// <summary>
/// Insert query statement builder
/// </summary>
public abstract class UpdateBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : UpdateBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="logicalOperator">The logical operator.</param>
    protected UpdateBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }


    /// <summary>
    /// Gets the update expressions.
    /// </summary>
    /// <value>
    /// The update expressions.
    /// </value>
    protected HashSet<UpdateExpression> UpdateExpressions { get; } = new();

    /// <summary>
    /// Gets the output expressions.
    /// </summary>
    /// <value>
    /// The output expressions.
    /// </value>
    protected HashSet<ColumnExpression> OutputExpressions { get; } = new();

    /// <summary>
    /// Gets from expressions.
    /// </summary>
    /// <value>
    /// From expressions.
    /// </value>
    protected HashSet<TableExpression> FromExpressions { get; } = new();

    /// <summary>
    /// Gets the join expressions.
    /// </summary>
    /// <value>
    /// The join expressions.
    /// </value>
    protected HashSet<JoinExpression> JoinExpressions { get; } = new();

    /// <summary>
    /// Gets the table expression.
    /// </summary>
    /// <value>
    /// The table expression.
    /// </value>
    protected TableExpression TableExpression { get; private set; }


    /// <summary>
    /// Set the target table to update.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="tableSchema">The table schema.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder Table(
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
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
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

        var paramterName = NextParameter();
        var updateClause = new UpdateExpression(columnName, paramterName);

        UpdateExpressions.Add(updateClause);
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
        string tableAlias = null)
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
        string tableAlias = null,
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
        string tableAlias = null,
        string columnAlias = null,
        Func<string, bool> condition = null)
    {
        if (condition != null && !condition(columnName))
            return (TBuilder)this;

        return Output(columnName, tableAlias, columnAlias);
    }


    /// <summary>
    /// Add a from clause to the query.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="tableSchema">The table schema.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public virtual TBuilder From(
        string tableName,
        string tableSchema = null,
        string tableAlias = null)
    {
        var fromClause = new TableExpression(tableName, tableSchema, tableAlias);

        FromExpressions.Add(fromClause);

        return (TBuilder)this;
    }

    /// <summary>
    /// Add a raw from clause to the query.
    /// </summary>
    /// <param name="fromClause">From clause.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder FromRaw(string fromClause)
    {
        if (fromClause.HasValue())
            FromExpressions.Add(new TableExpression(fromClause, IsRaw: true));

        return (TBuilder)this;
    }

    /// <summary>
    /// Add a join clause using the specified builder action
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder Join(Action<JoinBuilder> builder)
    {
        var innerBuilder = new JoinBuilder(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return (TBuilder)this;
    }

    /// <inheritdoc />
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
