using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL DELETE statements with fluent, chainable methods.
/// </summary>
public class DeleteBuilder : DeleteBuilder<DeleteBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    /// <param name="logicalOperator">The logical operator (<see cref="LogicalOperators"/>) to combine WHERE expressions. Defaults to <see cref="LogicalOperators.And"/>.</param>
    public DeleteBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }
}

/// <summary>
/// Provides a generic base class for building SQL DELETE statements with fluent, chainable methods.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder for fluent chaining.</typeparam>
public abstract class DeleteBuilder<TBuilder> : WhereBuilder<TBuilder>
    where TBuilder : DeleteBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    /// <param name="logicalOperator">The logical operator (<see cref="LogicalOperators"/>) to combine WHERE expressions. Defaults to <see cref="LogicalOperators.And"/>.</param>
    protected DeleteBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    /// <summary>
    /// Gets the collection of output column expressions for the DELETE statement.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{ColumnExpression}"/> containing the output column expressions.
    /// </value>
    protected HashSet<ColumnExpression> OutputExpressions { get; } = new();

    /// <summary>
    /// Gets the collection of FROM table expressions for the DELETE statement.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{TableExpression}"/> containing the FROM table expressions.
    /// </value>
    protected HashSet<TableExpression> FromExpressions { get; } = new();

    /// <summary>
    /// Gets the collection of JOIN expressions for the DELETE statement.
    /// </summary>
    /// <value>
    /// A <see cref="HashSet{JoinExpression}"/> containing the JOIN expressions.
    /// </value>
    protected HashSet<JoinExpression> JoinExpressions { get; } = new();

    /// <summary>
    /// Gets the target table expression for the DELETE statement.
    /// </summary>
    /// <value>
    /// The <see cref="TableExpression"/> representing the target table.
    /// </value>
    protected TableExpression TableExpression { get; private set; }

    /// <summary>
    /// Sets the target table to delete from.
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
    /// <param name="columnAlias">The alias for the column (optional).</param>
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
    /// <param name="columnAlias">The alias for the column (optional).</param>
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
    /// Adds a FROM clause to the DELETE statement.
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
    /// Adds a raw FROM clause to the DELETE statement.
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
    /// Adds a JOIN clause to the DELETE statement using the specified builder action.
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
    /// Builds the SQL DELETE statement using the current configuration.
    /// </summary>
    /// <returns>
    /// A <see cref="QueryStatement"/> containing the SQL DELETE statement and its parameters.
    /// </returns>
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
