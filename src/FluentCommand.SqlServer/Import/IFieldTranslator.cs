using System.Threading.Tasks;

namespace FluentCommand.Import;

/// <summary>
/// An interface for translating a field value
/// </summary>
public interface IFieldTranslator
{
    /// <summary>
    /// Translates the specified original value.
    /// </summary>
    /// <param name="original">The original value.</param>
    /// <returns>The translated value</returns>
    Task<object> Translate(string original);
}