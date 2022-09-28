namespace FluentCommand.Import;

/// <summary>
/// Import field mapping
/// </summary>
public class FieldMap
{
    /// <summary>
    /// Gets or sets the field index.
    /// </summary>
    /// <value>
    /// The field index.
    /// </value>
    public int? Index { get; set; }

    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    /// <value>
    /// The name of the field.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"Name: {Name}, Index: {Index}";
    }
}