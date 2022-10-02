using FluentCommand.Query;
using FluentCommand.Query.Generators;

namespace FluentCommand;


public class QueryBuilder : IStatementBuilder
{
    private IStatementBuilder _currentBuilder;

    public QueryBuilder(IQueryGenerator queryGenerator)
    {
        QueryGenerator = queryGenerator;
    }

    protected IQueryGenerator QueryGenerator { get; }


    public SelectEntityBuilder<TEntity> Select<TEntity>()
        where TEntity : class
    {
        var parameters = new List<QueryParameter>();
        var builder = new SelectEntityBuilder<TEntity>(QueryGenerator, parameters);

        // track current
        _currentBuilder = builder;

        return builder;
    }

    public SelectBuilder Select()
    {
        var parameters = new List<QueryParameter>();
        var builder = new SelectBuilder(QueryGenerator, parameters);

        // track current
        _currentBuilder = builder;

        return builder;

    }

    public InsertEntityBuilder<TEntity> Insert<TEntity>()
        where TEntity : class
    {
        var parameters = new List<QueryParameter>();
        var builder = new InsertEntityBuilder<TEntity>(QueryGenerator, parameters);

        // track current
        _currentBuilder = builder;

        return builder;
    }

    public InsertBuilder Insert()
    {
        var parameters = new List<QueryParameter>();
        var builder = new InsertBuilder(QueryGenerator, parameters);

        // track current
        _currentBuilder = builder;

        return builder;

    }

    public UpdateEntityBuilder<TEntity> Update<TEntity>()
        where TEntity : class
    {
        var parameters = new List<QueryParameter>();
        var builder = new UpdateEntityBuilder<TEntity>(QueryGenerator, parameters);

        // track current
        _currentBuilder = builder;

        return builder;

    }

    public UpdateBuilder Update()
    {
        var parameters = new List<QueryParameter>();
        var builder = new UpdateBuilder(QueryGenerator, parameters);

        // track current
        _currentBuilder = builder;

        return builder;
    }

    public DeleteEntityBuilder<TEntity> Delete<TEntity>()
        where TEntity : class
    {
        var parameters = new List<QueryParameter>();
        var builder = new DeleteEntityBuilder<TEntity>(QueryGenerator, parameters);

        // track current
        _currentBuilder = builder;

        return builder;

    }

    public DeleteBuilder Delete()
    {
        var parameters = new List<QueryParameter>();
        var builder = new DeleteBuilder(QueryGenerator, parameters);

        // track current
        _currentBuilder = builder;

        return builder;
    }


    public QueryStatement BuildStatement()
    {
        return _currentBuilder?.BuildStatement();
    }
}
