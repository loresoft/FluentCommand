using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace FluentCommand.Merge
{
    /// <summary>
    /// A fluent <see langword="interface" /> to a <see cref="DataMerge "/> operation.
    /// </summary>
    public interface IDataMerge
    {
        /// <summary>
        /// Sets a value indicating whether to insert data not found in <see cref="TargetTable"/>.
        /// </summary>
        /// <param name="value"><c>true</c> to insert data not found; otherwise, <c>false</c>.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge "/> operation.
        /// </returns>
        IDataMerge IncludeInsert(bool value = true);

        /// <summary>
        /// Sets a value indicating whether to update data found in <see cref="TargetTable"/>.
        /// </summary>
        /// <param name="value"><c>true</c> to update data found; otherwise, <c>false</c>.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge "/> operation.
        /// </returns>
        IDataMerge IncludeUpdate(bool value = true);

        /// <summary>
        /// Sets a value indicating whether to delete data from <see cref="TargetTable"/> not found in source data.
        /// </summary>
        /// <param name="value"><c>true</c> to delete target data not in source data; otherwise, <c>false</c>.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge "/> operation.
        /// </returns>
        IDataMerge IncludeDelete(bool value = true);

        /// <summary>
        /// Sets a value indicating whether to allow identity insert on the <see cref="TargetTable"/>.
        /// </summary>
        /// <param name="value"><c>true</c> to allow identity insert; otherwise, <c>false</c>.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge "/> operation.
        /// </returns>
        IDataMerge IdentityInsert(bool value = true);

        /// <summary>
        /// The name of target table to merge data into.
        /// </summary>
        /// <param name="value">The name of the target table.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge "/> operation.
        /// </returns>
        IDataMerge TargetTable(string value);

        /// <summary>
        /// Start mapping the columns to merge using the fluent <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The fluent data mapping builder.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge "/> operation.
        /// </returns>
        IDataMerge Map(Action<DataMergeMapping> builder);

        /// <summary>
        /// Start mapping the columns to merge using the strongly typed fluent <paramref name="builder" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="builder">The fluent data mapping builder.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge "/> operation.
        /// </returns>
        IDataMerge Map<TEntity>(Action<DataMergeMapping<TEntity>> builder);

        /// <summary>
        /// Sets the mode for how the merge will be processed.
        /// </summary>
        /// <param name="mergeMode">The merge mode.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge " /> operation.
        /// </returns>
        IDataMerge Mode(DataMergeMode mergeMode);


        /// <summary>
        /// Merges the specified <paramref name="data"/> into the <see cref="TargetTable"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="data">The data to be merged.</param>
        /// <returns>The number of rows processed.</returns>
        int Execute<TEntity>(IEnumerable<TEntity> data);

        /// <summary>
        /// Merges the specified <paramref name="tableData"/> into the <see cref="TargetTable"/>.
        /// </summary>
        /// <param name="tableData">The table data to be merged.</param>
        /// <returns>The number of rows processed.</returns>
        /// <exception cref="System.InvalidOperationException">Bulk-Copy only supported by SQL Server.  Make sure DataSession was create with a valid SqlConnection.</exception>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
        /// TargetTable is require for the merge definition.
        /// or
        /// At least one column is required for the merge definition.
        /// or
        /// At least one column is required to be marked as a key for the merge definition.
        /// or
        /// SourceColumn is require for column merge definition
        /// or
        /// NativeType is require for column merge definition
        /// </exception>
        int Execute(DataTable tableData);

        /// <summary>
        /// Merges the specified <paramref name="data"/> into the <see cref="DataMerge.TargetTable"/> asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="data">The data to be merged.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>The number of rows processed.</returns>
        Task<int> ExecuteAsync<TEntity>(IEnumerable<TEntity> data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Merges the specified <paramref name="tableData"/> into the <see cref="DataMerge.TargetTable"/> asynchronously.
        /// </summary>
        /// <param name="tableData">The table data to be merged.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>The number of rows processed.</returns>
        /// <exception cref="System.InvalidOperationException">Bulk-Copy only supported by SQL Server.  Make sure DataSession was create with a valid SqlConnection.</exception>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
        /// TargetTable is require for the merge definition.
        /// or
        /// At least one column is required for the merge definition.
        /// or
        /// At least one column is required to be marked as a key for the merge definition.
        /// or
        /// SourceColumn is require for column merge definition
        /// or
        /// NativeType is require for column merge definition
        /// </exception>
        Task<int> ExecuteAsync(DataTable tableData, CancellationToken cancellationToken = default);


        /// <summary>
        /// Merges the specified <paramref name="data"/> into the <see cref="TargetTable"/> capturing the changes in the result.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="data">The data to be merged.</param>
        /// <returns>A collection of <see cref="T:DataMergeOutput`1"/> instances.</returns>
        IEnumerable<DataMergeOutputRow> ExecuteOutput<TEntity>(IEnumerable<TEntity> data)
            where TEntity : class;

        /// <summary>
        /// Merges the specified <paramref name="table" /> into the <see cref="TargetTable" /> capturing the changes in the result.
        /// </summary>
        /// <param name="table">The table data to be merged.</param>
        /// <returns>A collection of the merge output in a dictionary.</returns>
        /// <exception cref="System.InvalidOperationException">Bulk-Copy only supported by SQL Server.  Make sure DataSession was create with a valid SqlConnection.</exception>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">TargetTable is require for the merge definition.
        /// or
        /// At least one column is required for the merge definition.
        /// or
        /// At least one column is required to be marked as a key for the merge definition.
        /// or
        /// SourceColumn is require for column merge definition
        /// or
        /// NativeType is require for column merge definition</exception>
        IEnumerable<DataMergeOutputRow> ExecuteOutput(DataTable table);
        
        /// <summary>
        /// Merges the specified <paramref name="data" /> into the <see cref="DataMerge.TargetTable" /> capturing the changes in the result asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="data">The data to be merged.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A collection of <see cref="T:DataMergeOutput`1" /> instances.
        /// </returns>
        Task<IEnumerable<DataMergeOutputRow>> ExecuteOutputAsync<TEntity>(IEnumerable<TEntity> data, CancellationToken cancellationToken = default)
            where TEntity : class;

        /// <summary>
        /// Merges the specified <paramref name="table" /> into the <see cref="DataMerge.TargetTable" /> capturing the changes in the result asynchronously.
        /// </summary>
        /// <param name="table">The table data to be merged.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A collection of the merge output in a dictionary.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Bulk-Copy only supported by SQL Server.  Make sure DataSession was create with a valid SqlConnection.</exception>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">TargetTable is require for the merge definition.
        /// or
        /// At least one column is required for the merge definition.
        /// or
        /// At least one column is required to be marked as a key for the merge definition.
        /// or
        /// SourceColumn is require for column merge definition
        /// or
        /// NativeType is require for column merge definition</exception>
        Task<IEnumerable<DataMergeOutputRow>> ExecuteOutputAsync(DataTable table, CancellationToken cancellationToken = default);
    }
}