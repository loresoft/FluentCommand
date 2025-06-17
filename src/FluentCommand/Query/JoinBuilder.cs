using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL JOIN clauses with fluent, chainable methods.
/// </summary>
public class JoinBuilder : JoinBuilder<JoinBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JoinBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    public JoinBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }
}

/// <summary>
/// Provides a generic base class for building SQL JOIN clauses with fluent, chainable methods.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder for fluent chaining.</typeparam>
public class JoinBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : JoinBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JoinBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    public JoinBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Gets or sets the join expression that defines the JOIN clause.
    /// </summary>
    /// <value>
    /// The <see cref="JoinExpression"/> representing the JOIN clause.
    /// </value>
    protected JoinExpression JoinExpression { get; set; } = new();

    /// <summary>
    /// Specifies the left column to join on.
    /// </summary>
    /// <param name="columnName">The name of the column in the left table.</param>
    /// <param name="tableAlias">The alias of the left table.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder Left(
        string columnName,
        string tableAlias)
    {
        JoinExpression = JoinExpression with
        {
            LeftColumnName = columnName,
            LeftTableAlias = tableAlias
        };

        return (TBuilder)this;
    }

    /// <summary>
    /// Specifies the right column and table to join on.
    /// </summary>
    /// <param name="columnName">The name of the column in the right table.</param>
    /// <param name="tableName">The name of the right table.</param>
    /// <param name="tableSchema">The schema of the right table.</param>
    /// <param name="tableAlias">The alias of the right table. If <c>null</c>, the table name is used as the alias.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder Right(
        string columnName,
        string tableName,
        string tableSchema,
        string tableAlias)
    {
        JoinExpression = JoinExpression with
        {
            RightColumnName = columnName,
            RightTableName = tableName,
            RightTableSchema = tableSchema,
            RightTableAlias = tableAlias ?? tableName
        };

        return (TBuilder)this;
    }

    /// <summary>
    /// Specifies the type of SQL JOIN operation to use.
    /// </summary>
    /// <param name="joinType">The <see cref="JoinTypes"/> value indicating the type of join (e.g., Inner, Left, Right).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public TBuilder Type(JoinTypes joinType)
    {
        JoinExpression = JoinExpression with
        {
            JoinType = joinType
        };

        return (TBuilder)this;
    }

    /// <summary>
    /// Builds and returns the <see cref="JoinExpression"/> representing the configured JOIN clause.
    /// </summary>
    /// <returns>
    /// The <see cref="JoinExpression"/> for the JOIN clause.
    /// </returns>
    public virtual JoinExpression BuildExpression()
    {
        return JoinExpression;
    }

    /// <summary>
    /// Builds the SQL JOIN statement using the current configuration.
    /// </summary>
    /// <returns>
    /// A <see cref="QueryStatement"/> containing the SQL JOIN clause and its parameters.
    /// </returns>
    public override QueryStatement BuildStatement()
    {
        var joinClause = QueryGenerator.JoinExpression(JoinExpression);

        return new QueryStatement(joinClause, Parameters);
    }
}
