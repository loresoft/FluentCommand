namespace FluentCommand.Import;

/// <summary>
/// The result of the import
/// </summary>
public class ImportResult
{
    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    /// <value>
    /// The message.
    /// </value>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the number of rows processed.
    /// </summary>
    /// <value>
    /// The number of rows processed.
    /// </value>
    public int Processed { get; set; }

    /// <summary>
    /// Gets or sets the list of errors.
    /// </summary>
    /// <value>
    /// The list of errors.
    /// </value>
    public IReadOnlyCollection<Exception> Errors { get; set; }
}
