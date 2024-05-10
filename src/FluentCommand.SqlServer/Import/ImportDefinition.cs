namespace FluentCommand.Import;

/// <summary>
/// Import definition
/// </summary>
public class ImportDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImportDefinition"/> class.
    /// </summary>
    public ImportDefinition()
    {
        CanInsert = true;
        CanUpdate = true;
        Fields = new List<FieldDefinition>();
    }

    /// <summary>
    /// Gets or sets the name of the import.
    /// </summary>
    /// <value>
    /// The name of the import.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the target table to merge the uploaded data into.
    /// </summary>
    /// <value>
    /// The target table name.
    /// </value>
    public string TargetTable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether data can be inserted that doesn't exists in the <see cref="TargetTable"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> if data can be inserted; otherwise, <c>false</c>.
    /// </value>
    public bool CanInsert { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether data can be updated when it exists in the <see cref="TargetTable"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> if data can be updated; otherwise, <c>false</c>.
    /// </value>
    public bool CanUpdate { get; set; }

    /// <summary>
    /// Gets or sets the list of field definitions.
    /// </summary>
    /// <value>
    /// The list of field definitions.
    /// </value>
    public List<FieldDefinition> Fields { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of errors.
    /// </summary>
    /// <value>
    /// The maximum number of errors.
    /// </value>
    public int MaxErrors { get; set; }

    /// <summary>
    /// Gets or sets the data row validator type.
    /// </summary>
    /// <value>
    /// The data row validator type.
    /// </value>
    public Type Validator { get; set; }

    /// <summary>
    /// Builds an <see cref="ImportDefinition"/> using the specified builder action.
    /// </summary>
    /// <param name="builder">The builder action delegate.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">builder is null</exception>
    public static ImportDefinition Build(Action<ImportDefinitionBuilder> builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        var importDefinition = new ImportDefinition();

        var importBuilder = new ImportDefinitionBuilder(importDefinition);
        builder(importBuilder);

        return importDefinition;
    }

    public override string ToString()
    {
        return $"Name: {Name}, Table: {TargetTable}, Insert: {CanInsert}, Update: {CanUpdate}";
    }
}
