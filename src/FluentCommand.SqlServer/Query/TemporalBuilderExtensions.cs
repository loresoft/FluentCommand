namespace FluentCommand.Query;

/// <summary>
/// Provides extension methods for integrating SQL Server temporal table queries (<c>FOR SYSTEM_TIME</c>) into select query builders.
/// </summary>
public static class TemporalBuilderExtensions
{
    /// <summary>
    /// Adds a temporal table clause (<c>FOR SYSTEM_TIME</c>) to the select query using a custom <see cref="TemporalBuilder"/> configuration.
    /// </summary>
    /// <param name="selectBuilder">The <see cref="SelectBuilder"/> to extend with temporal table support.</param>
    /// <param name="builder">An action to configure the <see cref="TemporalBuilder"/> for the temporal query.</param>
    /// <returns>
    /// The same <see cref="SelectBuilder"/> instance for fluent chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="selectBuilder"/> or <paramref name="builder"/> is <c>null</c>.
    /// </exception>
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

    /// <summary>
    /// Adds a temporal table clause (<c>FOR SYSTEM_TIME</c>) to the entity select query using a custom <see cref="TemporalBuilder"/> configuration.
    /// The table and schema are preset from the entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type representing the table for the temporal query.</typeparam>
    /// <param name="selectBuilder">The <see cref="SelectEntityBuilder{TEntity}"/> to extend with temporal table support.</param>
    /// <param name="builder">An action to configure the <see cref="TemporalBuilder"/> for the temporal query.</param>
    /// <returns>
    /// The same <see cref="SelectEntityBuilder{TEntity}"/> instance for fluent chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="selectBuilder"/> or <paramref name="builder"/> is <c>null</c>.
    /// </exception>
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
