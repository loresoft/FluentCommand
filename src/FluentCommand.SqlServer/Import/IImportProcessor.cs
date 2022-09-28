using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace FluentCommand.Import;

/// <summary>
/// A data import processor
/// </summary>
public interface IImportProcessor
{
    /// <summary>
    /// Import data using the specified <paramref name="importDefinition" /> and <paramref name="importData" />.
    /// </summary>
    /// <param name="importDefinition">The import definition.</param>
    /// <param name="importData">The import data.</param>
    /// <param name="username">The name of the user importing the data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The results of the import</returns>
    /// <exception cref="ArgumentNullException"><paramref name="importData" /> or <paramref name="importDefinition" /> is null</exception>
    Task<ImportResult> ImportAsync(ImportDefinition importDefinition, ImportData importData, string username, CancellationToken cancellationToken = default);
}