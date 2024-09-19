namespace FluentCommand.Merge;

/// <summary>
/// <see cref="IDataSession"/> extension methods
/// </summary>
public static class DataMergeExtensions
{
    /// <summary>
    /// Starts a data merge operation with the specified destination table name.
    /// </summary>
    /// <param name="session">The session to use for the merge.</param>
    /// <param name="destinationTable">Name of the destination table on the server.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="DataMerge " /> operation.
    /// </returns>
    public static IDataMerge MergeData(this IDataSession session, string destinationTable)
    {
        var definition = new DataMergeDefinition();
        definition.TargetTable = destinationTable;

        return MergeData(session, definition);
    }

    /// <summary>
    /// Starts a data merge operation with the specified <typeparamref name="TEntity"/>
    /// </summary>
    /// <param name="session">The session to use for the merge.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="DataMerge " /> operation.
    /// </returns>
    public static IDataMerge MergeData<TEntity>(this IDataSession session)
    {
        var definition = DataMergeDefinition.Create<TEntity>();
        return MergeData(session, definition);
    }

    /// <summary>
    /// Starts a data merge operation with the specified <paramref name="mergeDefinition"/>.
    /// </summary>
    /// <param name="session">The session to use for the merge.</param>
    /// <param name="mergeDefinition">The data merge definition.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="DataMerge " /> operation.
    /// </returns>
    public static IDataMerge MergeData(this IDataSession session, DataMergeDefinition mergeDefinition)
    {
        return new DataMerge(session, mergeDefinition);
    }


    /// <summary>
    /// Sets the wait time before terminating the attempt to execute a command and generating an error.
    /// </summary>
    /// <param name="dataMerge">The <see cref="IDataMerge"/> for this extension method.</param>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to wait for the command to execute.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to the data command.
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
