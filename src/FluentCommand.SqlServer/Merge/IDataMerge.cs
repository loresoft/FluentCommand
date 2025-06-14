using System.Data;

namespace FluentCommand.Merge;

/// <summary>
/// A fluent interface for configuring and executing a <see cref="DataMerge"/> operation.
/// </summary>
public interface IDataMerge
{
    /// <summary>
    /// Specifies whether to insert data not found in the target table during the merge operation.
    /// </summary>
    /// <param name="value"><c>true</c> to insert data not found in the target table; otherwise, <c>false</c>.</param>
    /// <returns>The same <see cref="IDataMerge"/> instance for fluent chaining.</returns>
    IDataMerge IncludeInsert(bool value = true);

    /// <summary>
    /// Specifies whether to update data found in the target table during the merge operation.
    /// </summary>
    /// <param name="value"><c>true</c> to update data found in the target table; otherwise, <c>false</c>.</param>
    /// <returns>The same <see cref="IDataMerge"/> instance for fluent chaining.</returns>
    IDataMerge IncludeUpdate(bool value = true);

    /// <summary>
    /// Specifies whether to delete data from the target table that is not found in the source data during the merge operation.
    /// </summary>
    /// <param name="value"><c>true</c> to delete rows in the target table not present in the source data; otherwise, <c>false</c>.</param>
    /// <returns>The same <see cref="IDataMerge"/> instance for fluent chaining.</returns>
    IDataMerge IncludeDelete(bool value = true);

    /// <summary>
    /// Specifies whether to allow identity insert on the target table during the merge operation.
    /// </summary>
    /// <param name="value"><c>true</c> to allow identity insert on the target table; otherwise, <c>false</c>.</param>
    /// <returns>The same <see cref="IDataMerge"/> instance for fluent chaining.</returns>
    IDataMerge IdentityInsert(bool value = true);

    /// <summary>
    /// Sets the name of the target table to merge data into.
    /// </summary>
    /// <param name="value">The name of the target table.</param>
    /// <returns>The same <see cref="IDataMerge"/> instance for fluent chaining.</returns>
    IDataMerge TargetTable(string value);

    /// <summary>
    /// Begins mapping the columns to merge using the provided fluent mapping builder.
    /// </summary>
    /// <param name="builder">A delegate to configure the data column mappings using a <see cref="DataMergeMapping"/> instance.</param>
    /// <returns>The same <see cref="IDataMerge"/> instance for fluent chaining.</returns>
    IDataMerge Map(Action<DataMergeMapping> builder);

    /// <summary>
    /// Begins mapping the columns to merge using a strongly typed fluent mapping builder.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity being merged.</typeparam>
    /// <param name="builder">A delegate to configure the data column mappings using a <see cref="DataMergeMapping{TEntity}"/> instance.</param>
    /// <returns>The same <see cref="IDataMerge"/> instance for fluent chaining.</returns>
    IDataMerge Map<TEntity>(Action<DataMergeMapping<TEntity>> builder) where TEntity : class;

    /// <summary>
    /// Sets the mode for how the merge will be processed.
    /// </summary>
    /// <param name="mergeMode">The merge mode to use. See <see cref="DataMergeMode"/> for options.</param>
    /// <returns>The same <see cref="IDataMerge"/> instance for fluent chaining.</returns>
    IDataMerge Mode(DataMergeMode mergeMode);

    /// <summary>
    /// Sets the wait time (in seconds) before terminating the attempt to execute the command and generating an error.
    /// </summary>
    /// <param name="timeout">The time in seconds to wait for the command to execute.</param>
    /// <returns>The same <see cref="IDataMerge"/> instance for fluent chaining.</returns>
    IDataMerge CommandTimeout(int timeout);

    /// <summary>
    /// Merges the specified <paramref name="data"/> into the target table.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity being merged.</typeparam>
    /// <param name="data">The data to be merged.</param>
    /// <returns>The number of rows processed.</returns>
    int Execute<TEntity>(IEnumerable<TEntity> data) where TEntity : class;

    /// <summary>
    /// Merges the specified <paramref name="tableData"/> into the target table.
    /// </summary>
    /// <param name="tableData">The <see cref="DataTable"/> containing the data to be merged.</param>
    /// <returns>The number of rows processed.</returns>
    /// <exception cref="System.InvalidOperationException">Bulk-Copy is only supported by SQL Server. Ensure DataSession was created with a valid SqlConnection.</exception>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    /// Thrown if the merge definition is invalid:
    /// <list type="bullet">
    /// <item><description>TargetTable is required.</description></item>
    /// <item><description>At least one column is required.</description></item>
    /// <item><description>At least one column must be marked as a key.</description></item>
    /// <item><description>SourceColumn is required for column mapping.</description></item>
    /// <item><description>NativeType is required for column mapping.</description></item>
    /// </list>
    /// </exception>
    int Execute(DataTable tableData);

    /// <summary>
    /// Asynchronously merges the specified <paramref name="data"/> into the target table.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity being merged.</typeparam>
    /// <param name="data">The data to be merged.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the number of rows processed.</returns>
    Task<int> ExecuteAsync<TEntity>(IEnumerable<TEntity> data, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Asynchronously merges the specified <paramref name="tableData"/> into the target table.
    /// </summary>
    /// <param name="tableData">The <see cref="DataTable"/> containing the data to be merged.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the number of rows processed.</returns>
    /// <exception cref="System.InvalidOperationException">Bulk-Copy is only supported by SQL Server. Ensure DataSession was created with a valid SqlConnection.</exception>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    /// Thrown if the merge definition is invalid:
    /// <list type="bullet">
    /// <item><description>TargetTable is required.</description></item>
    /// <item><description>At least one column is required.</description></item>
    /// <item><description>At least one column must be marked as a key.</description></item>
    /// <item><description>SourceColumn is required for column mapping.</description></item>
    /// <item><description>NativeType is required for column mapping.</description></item>
    /// </list>
    /// </exception>
    Task<int> ExecuteAsync(DataTable tableData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Merges the specified <paramref name="data"/> into the target table and returns the output rows describing the changes.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity being merged.</typeparam>
    /// <param name="data">The data to be merged.</param>
    /// <returns>A collection of <see cref="DataMergeOutputRow"/> instances describing the merge results.</returns>
    IEnumerable<DataMergeOutputRow> ExecuteOutput<TEntity>(IEnumerable<TEntity> data) where TEntity : class;

    /// <summary>
    /// Merges the specified <paramref name="table"/> into the target table and returns the output rows describing the changes.
    /// </summary>
    /// <param name="table">The <see cref="DataTable"/> containing the data to be merged.</param>
    /// <returns>A collection of <see cref="DataMergeOutputRow"/> instances describing the merge results.</returns>
    /// <exception cref="System.InvalidOperationException">Bulk-Copy is only supported by SQL Server. Ensure DataSession was created with a valid SqlConnection.</exception>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    /// Thrown if the merge definition is invalid:
    /// <list type="bullet">
    /// <item><description>TargetTable is required.</description></item>
    /// <item><description>At least one column is required.</description></item>
    /// <item><description>At least one column must be marked as a key.</description></item>
    /// <item><description>SourceColumn is required for column mapping.</description></item>
    /// <item><description>NativeType is required for column mapping.</description></item>
    /// </list>
    /// </exception>
    IEnumerable<DataMergeOutputRow> ExecuteOutput(DataTable table);

    /// <summary>
    /// Asynchronously merges the specified <paramref name="data"/> into the target table and returns the output rows describing the changes.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity being merged.</typeparam>
    /// <param name="data">The data to be merged.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains a collection of <see cref="DataMergeOutputRow"/> instances describing the merge results.
    /// </returns>
    Task<IEnumerable<DataMergeOutputRow>> ExecuteOutputAsync<TEntity>(IEnumerable<TEntity> data, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Asynchronously merges the specified <paramref name="table"/> into the target table and returns the output rows describing the changes.
    /// </summary>
    /// <param name="table">The <see cref="DataTable"/> containing the data to be merged.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains a collection of <see cref="DataMergeOutputRow"/> instances describing the merge results.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">Bulk-Copy is only supported by SQL Server. Ensure DataSession was created with a valid SqlConnection.</exception>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    /// Thrown if the merge definition is invalid:
    /// <list type="bullet">
    /// <item><description>TargetTable is required.</description></item>
    /// <item><description>At least one column is required.</description></item>
    /// <item><description>At least one column must be marked as a key.</description></item>
    /// <item><description>SourceColumn is required for column mapping.</description></item>
    /// <item><description>NativeType is required for column mapping.</description></item>
    /// </list>
    /// </exception>
    Task<IEnumerable<DataMergeOutputRow>> ExecuteOutputAsync(DataTable table, CancellationToken cancellationToken = default);
}
