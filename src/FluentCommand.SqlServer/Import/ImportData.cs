using System.Collections.Generic;

namespace FluentCommand.Import;

/// <summary>
/// The data and field mapping of an import
/// </summary>
public class ImportData
{
    /// <summary>
    /// Gets or sets the name of the import file.
    /// </summary>
    /// <value>
    /// The name of the file.
    /// </value>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the data to be imported.
    /// </summary>
    /// <value>
    /// The data to be imported.
    /// </value>
    public string[][] Data { get; set; }

    /// <summary>
    /// Gets or sets the list of field mappings.
    /// </summary>
    /// <value>
    /// The list of field mappings.
    /// </value>
    public List<FieldMap> Mappings { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether data has a header row.
    /// </summary>
    /// <value>
    ///   <c>true</c> if data has a header row; otherwise, <c>false</c>.
    /// </value>
    public bool HasHeader { get; set; } = true;
}