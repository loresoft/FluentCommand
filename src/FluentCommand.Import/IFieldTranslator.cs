namespace FluentCommand.Import;

/// <summary>
/// Defines a contract for translating or converting a field value during import operations.
/// </summary>
public interface IFieldTranslator
{
    /// <summary>
    /// Translates the specified original string value to a new value suitable for import.
    /// </summary>
    /// <param name="original">The original string value to be translated.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with the translated value as the result.
    /// </returns>
    Task<object> Translate(string original);
}
