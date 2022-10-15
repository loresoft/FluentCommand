using System.Text;

using FluentCommand.Query.Generators;

namespace FluentCommand.Query;


public class QueryBuilder : IStatementBuilder
{
    private readonly Queue<IStatementBuilder> _builderQueue = new();

    public QueryBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
    {
        QueryGenerator = queryGenerator ?? throw new ArgumentNullException(nameof(queryGenerator));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected IQueryGenerator QueryGenerator { get; }

    protected List<QueryParameter> Parameters { get; }


    public StatementBuilder Statement()
    {
        var builder = new StatementBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }

    public SelectEntityBuilder<TEntity> Select<TEntity>()
        where TEntity : class
    {
        var builder = new SelectEntityBuilder<TEntity>(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }

    public SelectBuilder Select()
    {
        var builder = new SelectBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;

    }

    public InsertEntityBuilder<TEntity> Insert<TEntity>()
        where TEntity : class
    {
        var builder = new InsertEntityBuilder<TEntity>(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }

    public InsertBuilder Insert()
    {
        var builder = new InsertBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;

    }

    public UpdateEntityBuilder<TEntity> Update<TEntity>()
        where TEntity : class
    {
        var builder = new UpdateEntityBuilder<TEntity>(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;

    }

    public UpdateBuilder Update()
    {
        var builder = new UpdateBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }

    public DeleteEntityBuilder<TEntity> Delete<TEntity>()
        where TEntity : class
    {
        var builder = new DeleteEntityBuilder<TEntity>(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;

    }

    public DeleteBuilder Delete()
    {
        var builder = new DeleteBuilder(QueryGenerator, Parameters);

        _builderQueue.Enqueue(builder);

        return builder;
    }


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
