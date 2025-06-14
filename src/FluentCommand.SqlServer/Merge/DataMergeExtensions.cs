namespace FluentCommand.Merge;

/// <summary>
/// Provides extension methods for <see cref="IDataSession"/> to configure and execute data merge operations.
/// </summary>
public static class DataMergeExtensions
{
    /// <summary>
    /// Starts a data merge operation using the specified destination table name.
    /// </summary>
    /// <param name="session">The <see cref="IDataSession"/> to use for the merge operation.</param>
    /// <param name="destinationTable">The name of the destination table on the server.</param>
    /// <returns>
    /// An <see cref="IDataMerge"/> instance for configuring and executing the merge operation.
    /// </returns>
    public static IDataMerge MergeData(this IDataSession session, string destinationTable)
    {
        var definition = new DataMergeDefinition();
        definition.TargetTable = destinationTable;

        return MergeData(session, definition);
    }

    /// <summary>
    /// Starts a data merge operation using the specified entity type <typeparamref name="TEntity"/>.
    /// The target table and columns are automatically mapped from the entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to merge.</typeparam>
    /// <param name="session">The <see cref="IDataSession"/> to use for the merge operation.</param>
    /// <returns>
    /// An <see cref="IDataMerge"/> instance for configuring and executing the merge operation.
    /// </returns>
    public static IDataMerge MergeData<TEntity>(this IDataSession session)
    {
        var definition = DataMergeDefinition.Create<TEntity>();
        return MergeData(session, definition);
    }

    /// <summary>
    /// Starts a data merge operation using the specified <see cref="DataMergeDefinition"/>.
    /// </summary>
    /// <param name="session">The <see cref="IDataSession"/> to use for the merge operation.</param>
    /// <param name="mergeDefinition">The <see cref="DataMergeDefinition"/> that defines the merge configuration.</param>
    /// <returns>
    /// An <see cref="IDataMerge"/> instance for configuring and executing the merge operation.
    /// </returns>
    public static IDataMerge MergeData(this IDataSession session, DataMergeDefinition mergeDefinition)
    {
        return new DataMerge(session, mergeDefinition);
    }

    /// <summary>
    /// Sets the wait time before terminating the attempt to execute a command and generating an error.
    /// </summary>
    /// <param name="dataMerge">The <see cref="IDataMerge"/> instance to configure.</param>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to wait for the command to execute before timing out.</param>
    /// <returns>
    /// The same <see cref="IDataMerge"/> instance for fluent chaining.
    /// </returns>
    public static IDataMerge CommandTimeout(
        this IDataMerge dataMerge,
        TimeSpan timeSpan)
    {
        var timeout = Convert.ToInt32(timeSpan.TotalSeconds);
        dataMerge.CommandTimeout(timeout);
        return dataMerge;
    }

}
