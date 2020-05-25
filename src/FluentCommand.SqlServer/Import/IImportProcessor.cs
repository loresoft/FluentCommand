using System.Data;

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
        /// Populates the <see cref="DataTable"/> with the specified <paramref name="importData" />.
        /// </summary>
        /// <param name="dataTable">The data table to populate.</param>
        /// <param name="importDefinition">The import definition.</param>
        /// <param name="importData">The import data.</param>
        /// <returns></returns>
        DataTable PopulateTable(DataTable dataTable, ImportDefinition importDefinition, ImportData importData);
    }
}