namespace FluentCommand.Query;

/// <summary>
/// Provides extension methods for integrating SQL Server <c>CHANGETABLE (CHANGES ...)</c> queries into select query builders.
/// </summary>
public static class ChangeTableBuilderExtensions
{
    /// <summary>
    /// Adds a <c>CHANGETABLE (CHANGES ...)</c> clause to the select query using a custom <see cref="ChangeTableBuilder"/> configuration.
    /// </summary>
    /// <param name="selectBuilder">The <see cref="SelectBuilder"/> to extend with change tracking support.</param>
    /// <param name="builder">An action to configure the <see cref="ChangeTableBuilder"/> for the change tracking query.</param>
    /// <returns>
    /// The same <see cref="SelectBuilder"/> instance for fluent chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="selectBuilder"/> or <paramref name="builder"/> is <c>null</c>.
    /// </exception>
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

    /// <summary>
    /// Adds a <c>CHANGETABLE (CHANGES ...)</c> clause to the entity select query using a custom <see cref="ChangeTableBuilder"/> configuration.
    /// The table and schema are preset from the entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type representing the table for change tracking.</typeparam>
    /// <param name="selectBuilder">The <see cref="SelectEntityBuilder{TEntity}"/> to extend with change tracking support.</param>
    /// <param name="builder">An action to configure the <see cref="ChangeTableBuilder"/> for the change tracking query.</param>
    /// <returns>
    /// The same <see cref="SelectEntityBuilder{TEntity}"/> instance for fluent chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="selectBuilder"/> or <paramref name="builder"/> is <c>null</c>.
    /// </exception>
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
