namespace FluentCommand.Import;

/// <summary>
/// Provides shared data and context for the current import process, including import definitions, data, user information, errors, and mapped fields.
/// </summary>
public class ImportProcessContext
{
    private readonly Lazy<IReadOnlyList<ImportFieldMapping>> _mappedFields;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportProcessContext"/> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> used for dependency resolution during import.</param>
    /// <param name="definition">The <see cref="ImportDefinition"/> describing the import configuration and rules.</param>
    /// <param name="importData">The <see cref="ImportData"/> containing the data and field mappings to be imported.</param>
    /// <param name="userName">The name of the user performing the import operation.</param>
    public ImportProcessContext(IServiceProvider serviceProvider, ImportDefinition definition, ImportData importData, string userName)
    {
        Services = serviceProvider;
        Definition = definition;
        ImportData = importData;
        UserName = userName;
        Errors = new List<Exception>();

        _mappedFields = new Lazy<IReadOnlyList<ImportFieldMapping>>(GetMappedFields);
    }

    /// <summary>
    /// Gets the name of the user performing the import operation.
    /// </summary>
    /// <value>
    /// The user name.
    /// </value>
    public string UserName { get; }

    /// <summary>
    /// Gets the <see cref="ImportDefinition"/> describing the import configuration and rules.
    /// </summary>
    /// <value>
    /// The import definition.
    /// </value>
    public ImportDefinition Definition { get; }

    /// <summary>
    /// Gets the <see cref="ImportData"/> containing the data and field mappings to be imported.
    /// </summary>
    /// <value>
    /// The import data.
    /// </value>
    public ImportData ImportData { get; }

    /// <summary>
    /// Gets the list of mapped fields for the import operation.
    /// Each <see cref="ImportFieldMapping"/> associates a field definition with its mapping information.
    /// </summary>
    /// <value>
    /// The mapped fields for the import process.
    /// </value>
    public IReadOnlyList<ImportFieldMapping> MappedFields => _mappedFields.Value;

    /// <summary>
    /// Gets or sets the list of errors that occurred during the import process.
    /// </summary>
    /// <value>
    /// A list of <see cref="Exception"/> instances representing errors encountered.
    /// </value>
    public List<Exception> Errors { get; set; }

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> used for resolving services during the import process.
    /// </summary>
    /// <value>
    /// The service provider.
    /// </value>
    public IServiceProvider Services { get; }

    private List<ImportFieldMapping> GetMappedFields()
    {
        var list = new List<ImportFieldMapping>();

        var firstRow = ImportData.Data.FirstOrDefault();
        var columns = firstRow?.Length ?? 0;

        foreach (var field in Definition.Fields)
        {
            // if default value, include
            if (field.Default.HasValue)
            {
                list.Add(new ImportFieldMapping(field));
                continue;
            }

            var name = field.Name;

            // if mapped to an index, include
            var mapping = ImportData.Mappings.FirstOrDefault(f => f.Name == name);
            if (mapping?.Index == null)
            {
                if (field.IsRequired)
                    throw new InvalidOperationException($"Missing required field mapping for '{name}'");

                continue;
            }

            if (mapping.Index.Value >= columns)
            {
                throw new IndexOutOfRangeException(
                    $"The mapped index {mapping.Index.Value} for field '{name}' is out of range of {columns}");
            }

            list.Add(new ImportFieldMapping(field, mapping));
        }

        return list;
    }
}
