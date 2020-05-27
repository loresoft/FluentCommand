using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace FluentCommand.Import
{
    /// <summary>
    /// A data import processor
    /// </summary>
    public interface IImportProcessor
    {
        /// <summary>
        /// Create a <see cref="DataTable"/> instance using the specified <paramref name="importDefinition" />.
        /// </summary>
        /// <param name="importDefinition">The import definition to create DataTable from.</param>
        /// <returns>An instance of <see cref="DataTable"/>.</returns>
        DataTable CreateTable(ImportDefinition importDefinition);

        /// <summary>
        /// Create and populates a <see cref="DataTable" /> instance using the specified <paramref name="importDefinition" /> and <paramref name="importData" />.
        /// </summary>
        /// <param name="importDefinition">The import definition to create DataTable from.</param>
        /// <param name="importData">The import data.</param>
        /// <returns>An instance of <see cref="DataTable" />.</returns>
        DataTable CreateTable(ImportDefinition importDefinition, ImportData importData);

        /// <summary>
        /// Merges the specified <paramref name="dataTable" /> using the <paramref name="importDefinition" />.
        /// </summary>
        /// <param name="dataTable">The data table to merge.</param>
        /// <param name="importDefinition">The import definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An instance of <see cref="ImportResult" /> indicating the number of rows processed.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dataTable" /> or <paramref name="importDefinition" /> is null 
        /// </exception>
        Task<ImportResult> MergeDataAsync(DataTable dataTable, ImportDefinition importDefinition, CancellationToken cancellationToken = default);

        /// <summary>
        /// Merge data using the specified <paramref name="importDefinition" /> and <paramref name="importData"/>.
        /// </summary>
        /// <param name="importDefinition">The import definition.</param>
        /// <param name="importData">The import data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="importData" /> or <paramref name="importDefinition" /> is null 
        /// </exception>
        Task<ImportResult> MergeDataAsync(ImportDefinition importDefinition, ImportData importData, CancellationToken cancellationToken = default);
    }
}