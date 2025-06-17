using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL UPDATE statements with fluent, chainable methods.
/// </summary>
public class UpdateBuilder : UpdateBuilder<UpdateBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    /// <param name="logicalOperator">The logical operator (<see cref="LogicalOperators"/>) to combine WHERE expressions. Defaults to <see cref="LogicalOperators.And"/>.</param>
    public UpdateBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

/// <summary>
/// Provides a generic base class for building SQL UPDATE statements with fluent, chainable methods.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder for fluent chaining.</typeparam>
public abstract class UpdateBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : UpdateBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    /// <param name="logicalOperator">The logical operator (<see cref="LogicalOperators"/>) to combine WHERE expressions. Defaults to <see cref="LogicalOperators.And"/>.</param>
    protected UpdateBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    /// <summary>
    /// Gets the collection of update expressions for the UPDATE statement.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{UpdateExpression}"/> containing the update expressions.
    /// </value>
    protected HashSet<UpdateExpression> UpdateExpressions { get; } = new();

    /// <summary>
    /// Gets the collection of output column expressions for the UPDATE statement.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{ColumnExpression}"/> containing the output column expressions.
    /// </value>
    protected HashSet<ColumnExpression> OutputExpressions { get; } = new();

    /// <summary>
    /// Gets the collection of FROM table expressions for the UPDATE statement.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{TableExpression}"/> containing the FROM table expressions.
    /// </value>
    protected HashSet<TableExpression> FromExpressions { get; } = new();

    /// <summary>
    /// Gets the collection of JOIN expressions for the UPDATE statement.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{JoinExpression}"/> containing the JOIN expressions.
    /// </value>
    protected HashSet<JoinExpression> JoinExpressions { get; } = new();

    /// <summary>
    /// Gets the target table expression for the UPDATE statement.
    /// </summary>
    /// <value>
    /// The <see cref="TableExpression"/> representing the target table.
    /// </value>
    protected TableExpression TableExpression { get; private set; }

    /// <summary>
    /// Sets the target table to update.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="tableSchema">The schema of the table (optional).</param>
    /// <param name="tableAlias">The alias for the table (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Adds a value for the specified column name and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">The name of the column to update.</param>
    /// <param name="parameterValue">The value to set for the column.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder Value<TValue>(
        string columnName,
        TValue parameterValue)
    {
        return Value(columnName, parameterValue, typeof(TValue));
    }

    /// <summary>
    /// Adds a value for the specified column name, value, and type.
    /// </summary>
    /// <param name="columnName">The name of the column to update.</param>
    /// <param name="parameterValue">The value to set for the column.</param>
    /// <param name="parameterType">The type of the parameter value.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="columnName"/> is null or empty.</exception>
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
    /// Conditionally adds a value for the specified column name and value if the condition is met.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">The name of the column to update.</param>
    /// <param name="parameterValue">The value to set for the column.</param>
    /// <param name="condition">A function that determines whether to add the value, based on the column name and value. If <c>null</c>, the value is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Adds an OUTPUT clause for the specified column names.
    /// </summary>
    /// <param name="columnNames">The collection of column names to include in the OUTPUT clause.</param>
    /// <param name="tableAlias">The alias for the table (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="columnNames"/> is <c>null</c>.</exception>
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
    /// Adds an OUTPUT clause for the specified column name.
    /// </summary>
    /// <param name="columnName">The name of the column to include in the OUTPUT clause.</param>
    /// <param name="tableAlias">The alias for the table (optional).</param>
    /// <param name="columnAlias">The alias for the output column (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Conditionally adds an OUTPUT clause for the specified column name if the condition is met.
    /// </summary>
    /// <param name="columnName">The name of the column to include in the OUTPUT clause.</param>
    /// <param name="tableAlias">The alias for the table (optional).</param>
    /// <param name="columnAlias">The alias for the output column (optional).</param>
    /// <param name="condition">A function that determines whether to add the OUTPUT clause. If <c>null</c>, the clause is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Adds a FROM clause to the UPDATE statement.
    /// </summary>
    /// <param name="tableName">The name of the table to include in the FROM clause.</param>
    /// <param name="tableSchema">The schema of the table (optional).</param>
    /// <param name="tableAlias">The alias for the table (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Adds a raw FROM clause to the UPDATE statement.
    /// </summary>
    /// <param name="fromClause">The raw SQL FROM clause.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder FromRaw(string fromClause)
    {
        if (fromClause.HasValue())
            FromExpressions.Add(new TableExpression(fromClause, IsRaw: true));

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a JOIN clause to the UPDATE statement using the specified builder action.
    /// </summary>
    /// <param name="builder">An action that configures the join using a <see cref="JoinBuilder"/>.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder Join(Action<JoinBuilder> builder)
    {
        var innerBuilder = new JoinBuilder(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return (TBuilder)this;
    }

    /// <summary>
    /// Builds the SQL UPDATE statement using the current configuration.
    /// </summary>
    /// <returns>
    /// A <see cref="QueryStatement"/> containing the SQL UPDATE statement and its parameters.
    /// </returns>
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
