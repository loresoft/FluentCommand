using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Join clause builder
/// </summary>
public class JoinBuilder : JoinBuilder<JoinBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JoinBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    public JoinBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public class JoinBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : JoinBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JoinBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    public JoinBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Gets or sets the join expression.
    /// </summary>
    /// <value>
    /// The join expression.
    /// </value>
    protected JoinExpression JoinExpression { get; set; } = new();

    /// <summary>
    /// The left column to join on.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// The right column to join on.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="tableSchema">The table schema.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Specify the join type.
    /// </summary>
    /// <param name="joinType">Type of the join.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Builds the join expression.
    /// </summary>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public virtual JoinExpression BuildExpression()
    {
        return JoinExpression;
    }

    /// <inheritdoc />
    public override QueryStatement BuildStatement()
    {
        var joinClause = QueryGenerator.JoinExpression(JoinExpression);

        return new QueryStatement(joinClause, Parameters);
    }
}
