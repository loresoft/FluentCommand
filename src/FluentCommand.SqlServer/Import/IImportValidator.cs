using System.Data;
using System.Threading.Tasks;

namespace FluentCommand.Import;

/// <summary>
/// An import data validator
/// </summary>
public interface IImportValidator
{
    /// <summary>
    /// Validates the specified <paramref name="targetRow" />.
    /// </summary>
    /// <param name="importDefinition">The import definition.</param>
    /// <param name="targetRow">The target row.</param>
    /// <returns></returns>
    Task ValidateRow(ImportDefinition importDefinition, DataRow targetRow);
}
