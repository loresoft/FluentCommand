using System.Text;

using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Provides a high-level builder for constructing and composing multiple SQL query statements.
/// </summary>
/// <seealso cref="FluentCommand.Query.IStatementBuilder" />
public class QueryBuilder : IStatementBuilder
{
    private readonly Queue<IStatementBuilder> _builderQueue = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions and statements.</param>
    /// <param name="parameters">The initial list of <see cref="QueryParameter"/> objects for the query.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown if <paramref name="queryGenerator"/> or <paramref name="parameters"/> is <c>null</c>.
    /// </exception>
    public QueryBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
    {
        QueryGenerator = queryGenerator ?? throw new ArgumentNullException(nameof(queryGenerator));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    /// <summary>
    /// Gets the <see cref="IQueryGenerator"/> used to generate SQL expressions and statements.
    /// </summary>
    /// <value>
    /// The <see cref="IQueryGenerator"/> instance.
    /// </value>
    protected IQueryGenerator QueryGenerator { get; }

    /// <summary>
    /// Gets the list of <see cref="QueryParameter"/> objects used in the query.
    /// </summary>
    /// <value>
    /// The list of query parameters.
    /// </value>
    protected List<QueryParameter> Parameters { get; }


    /// <summary>
    /// Starts a new raw SQL statement builder and adds it to the query.
    /// </summary>
    /// <returns>
    /// A new <see cref="StatementBuilder"/> instance for building a raw SQL statement.
    /// </returns>
    public StatementBuilder Statement()
    {
        var builder = new StatementBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }

    /// <summary>
    /// Starts a new SELECT statement builder for a specific entity type and adds it to the query.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to select.</typeparam>
    /// <returns>
    /// A new <see cref="SelectEntityBuilder{TEntity}"/> instance for building a SELECT statement.
    /// </returns>
    public SelectEntityBuilder<TEntity> Select<TEntity>()
        where TEntity : class
    {
        var builder = new SelectEntityBuilder<TEntity>(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }

    /// <summary>
    /// Starts a new SELECT statement builder and adds it to the query.
    /// </summary>
    /// <returns>
    /// A new <see cref="SelectBuilder"/> instance for building a SELECT statement.
    /// </returns>
    public SelectBuilder Select()
    {
        var builder = new SelectBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;

    }

    /// <summary>
    /// Starts a new INSERT statement builder for a specific entity type and adds it to the query.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to insert.</typeparam>
    /// <returns>
    /// A new <see cref="InsertEntityBuilder{TEntity}"/> instance for building an INSERT statement.
    /// </returns>
    public InsertEntityBuilder<TEntity> Insert<TEntity>()
        where TEntity : class
    {
        var builder = new InsertEntityBuilder<TEntity>(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }

    /// <summary>
    /// Starts a new INSERT statement builder and adds it to the query.
    /// </summary>
    /// <returns>
    /// A new <see cref="InsertBuilder"/> instance for building an INSERT statement.
    /// </returns>
    public InsertBuilder Insert()
    {
        var builder = new InsertBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;

    }

    /// <summary>
    /// Starts a new UPDATE statement builder for a specific entity type and adds it to the query.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to update.</typeparam>
    /// <returns>
    /// A new <see cref="UpdateEntityBuilder{TEntity}"/> instance for building an UPDATE statement.
    /// </returns>
    public UpdateEntityBuilder<TEntity> Update<TEntity>()
        where TEntity : class
    {
        var builder = new UpdateEntityBuilder<TEntity>(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;

    }

    /// <summary>
    /// Starts a new UPDATE statement builder and adds it to the query.
    /// </summary>
    /// <returns>
    /// A new <see cref="UpdateBuilder"/> instance for building an UPDATE statement.
    /// </returns>
    public UpdateBuilder Update()
    {
        var builder = new UpdateBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }

    /// <summary>
    /// Starts a new DELETE statement builder for a specific entity type and adds it to the query.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to delete.</typeparam>
    /// <returns>
    /// A new <see cref="DeleteEntityBuilder{TEntity}"/> instance for building a DELETE statement.
    /// </returns>
    public DeleteEntityBuilder<TEntity> Delete<TEntity>()
        where TEntity : class
    {
        var builder = new DeleteEntityBuilder<TEntity>(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;

    }

    /// <summary>
    /// Starts a new DELETE statement builder and adds it to the query.
    /// </summary>
    /// <returns>
    /// A new <see cref="DeleteBuilder"/> instance for building a DELETE statement.
    /// </returns>
    public DeleteBuilder Delete()
    {
        var builder = new DeleteBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }

    /// <summary>
    /// Builds and returns a <see cref="QueryStatement"/> representing the composed SQL query and its parameters.
    /// </summary>
    /// <returns>
    /// A <see cref="QueryStatement"/> containing the SQL statement and associated <see cref="QueryParameter"/> values.
    /// </returns>
    public QueryStatement BuildStatement()
    {
        // optimize for when only 1 builder
        if (_builderQueue.Count == 1)
        {
            var builder = _builderQueue.Dequeue();
            return builder.BuildStatement();
        }

        // merge all queued builders together
        var query = new StringBuilder();

        while (_builderQueue.Count > 0)
        {
            var builder = _builderQueue.Dequeue();
            var statement = builder.BuildStatement();

            query.AppendLine(statement.Statement);
        }

        return new QueryStatement(query.ToString(), Parameters);
    }
}
