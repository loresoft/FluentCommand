using System.Data;

namespace FluentCommand.Import;

/// <summary>
/// Defines a contract for validating imported data rows during an import operation.
/// </summary>
public interface IImportValidator
{
    /// <summary>
    /// Validates the specified data row against the provided import definition.
    /// </summary>
    /// <param name="importDefinition">The <see cref="ImportDefinition"/> containing the import configuration and field definitions.</param>
    /// <param name="targetRow">The <see cref="DataRow"/> representing the data to validate.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous validation operation.
    /// Implementations should throw an exception or collect errors if validation fails.
    /// </returns>
    Task ValidateRow(ImportDefinition importDefinition, DataRow targetRow);
}
