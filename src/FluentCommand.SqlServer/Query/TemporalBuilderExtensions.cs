namespace FluentCommand.Query;

public static class TemporalBuilderExtensions
{
    public static SelectBuilder Temporal(this SelectBuilder selectBuilder, Action<TemporalBuilder> builder)
    {
        if (selectBuilder is null)
            throw new ArgumentNullException(nameof(selectBuilder));
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        var queryBuilder = selectBuilder as IQueryBuilder;

        var innerBuilder = new TemporalBuilder(queryBuilder.QueryGenerator, queryBuilder.Parameters);
        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();
        selectBuilder.FromRaw(statement.Statement);

        return selectBuilder;

    }

    public static SelectEntityBuilder<TEntity> Temporal<TEntity>(this SelectEntityBuilder<TEntity> selectBuilder, Action<TemporalBuilder> builder)
        where TEntity : class
    {
        if (selectBuilder is null)
            throw new ArgumentNullException(nameof(selectBuilder));
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        var queryBuilder = selectBuilder as IQueryBuilder;

        var innerBuilder = new TemporalBuilder(queryBuilder.QueryGenerator, queryBuilder.Parameters);

        // preset table and schema
        innerBuilder.From<TEntity>();

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();
        selectBuilder.FromRaw(statement.Statement);

        return selectBuilder;
    }
}
