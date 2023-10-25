using System.Text;

using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// High level query builder
/// </summary>
/// <seealso cref="FluentCommand.Query.IStatementBuilder" />
public class QueryBuilder : IStatementBuilder
{
    private readonly Queue<IStatementBuilder> _builderQueue = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The parameters.</param>
    /// <exception cref="System.ArgumentNullException">queryGenerator or parameters</exception>
    public QueryBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
    {
        QueryGenerator = queryGenerator ?? throw new ArgumentNullException(nameof(queryGenerator));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    /// <summary>
    /// Gets the query generator.
    /// </summary>
    /// <value>
    /// The query generator.
    /// </value>
    protected IQueryGenerator QueryGenerator { get; }

    /// <summary>
    /// Gets the query parameters.
    /// </summary>
    /// <value>
    /// The query parameters.
    /// </value>
    protected List<QueryParameter> Parameters { get; }


    /// <summary>
    /// Starts a statement builder
    /// </summary>
    /// <returns>A new statement builder</returns>
    public StatementBuilder Statement()
    {
        var builder = new StatementBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }

    /// <summary>
    /// Starts a select statement builder
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>A new select statement builder</returns>
    public SelectEntityBuilder<TEntity> Select<TEntity>()
        where TEntity : class
    {
        var builder = new SelectEntityBuilder<TEntity>(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }

    /// <summary>
    /// Starts a select statement builder
    /// </summary>
    /// <returns>A new select statement builder</returns>
    public SelectBuilder Select()
    {
        var builder = new SelectBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;

    }

    /// <summary>
    /// Starts an insert statement builder
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>A new insert statement builder</returns>
    public InsertEntityBuilder<TEntity> Insert<TEntity>()
        where TEntity : class
    {
        var builder = new InsertEntityBuilder<TEntity>(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }

    /// <summary>
    /// Starts an insert statement builder
    /// </summary>
    /// <returns>A new insert statement builder</returns>
    public InsertBuilder Insert()
    {
        var builder = new InsertBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;

    }

    /// <summary>
    /// Starts an update statement builder
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>A new update statement builder</returns>
    public UpdateEntityBuilder<TEntity> Update<TEntity>()
        where TEntity : class
    {
        var builder = new UpdateEntityBuilder<TEntity>(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;

    }

    /// <summary>
    /// Starts an update statement builder
    /// </summary>
    /// <returns>A new update statement builder</returns>
    public UpdateBuilder Update()
    {
        var builder = new UpdateBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }

    /// <summary>
    /// Starts a delete statement builder
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>A new delete statement builder</returns>
    public DeleteEntityBuilder<TEntity> Delete<TEntity>()
        where TEntity : class
    {
        var builder = new DeleteEntityBuilder<TEntity>(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;

    }

    /// <summary>
    /// Starts a delete statement builder
    /// </summary>
    /// <returns>A new insert statement builder</returns>
    public DeleteBuilder Delete()
    {
        var builder = new DeleteBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }

    /// <inheritdoc />
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
