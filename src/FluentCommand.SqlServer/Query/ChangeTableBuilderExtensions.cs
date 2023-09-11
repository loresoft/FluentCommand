namespace FluentCommand.Query;

public static class ChangeTableBuilderExtensions
{
    public static SelectBuilder ChangeTable(this SelectBuilder selectBuilder, Action<ChangeTableBuilder> builder)
    {
        if (selectBuilder is null)
            throw new ArgumentNullException(nameof(selectBuilder));
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        var queryBuilder = selectBuilder as IQueryBuilder;

        var innerBuilder = new ChangeTableBuilder(queryBuilder.QueryGenerator, queryBuilder.Parameters);
        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();
        selectBuilder.FromRaw(statement.Statement);

        return selectBuilder;

    }

    public static SelectEntityBuilder<TEntity> ChangeTable<TEntity>(this SelectEntityBuilder<TEntity> selectBuilder, Action<ChangeTableBuilder> builder)
        where TEntity : class
    {
        if (selectBuilder is null)
            throw new ArgumentNullException(nameof(selectBuilder));
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        var queryBuilder = selectBuilder as IQueryBuilder;

        var innerBuilder = new ChangeTableBuilder(queryBuilder.QueryGenerator, queryBuilder.Parameters);

        // preset table and schema
        innerBuilder.From<TEntity>();

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();
        selectBuilder.FromRaw(statement.Statement);

        return selectBuilder;
    }
}
