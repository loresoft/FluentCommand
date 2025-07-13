namespace FluentCommand.Import;

/// <summary>
/// Provides a fluent API to configure and build an <see cref="ImportDefinition"/>.
/// </summary>
public class ImportDefinitionBuilder
{
    private readonly ImportDefinition _importDefinition;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportDefinitionBuilder"/> class.
    /// </summary>
    /// <param name="importDefinition">The <see cref="ImportDefinition"/> to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="importDefinition"/> is <c>null</c>.</exception>
    public ImportDefinitionBuilder(ImportDefinition importDefinition)
    {
        _importDefinition = importDefinition ?? throw new ArgumentNullException(nameof(importDefinition));
    }

    /// <summary>
    /// Sets the import name for the <see cref="ImportDefinition"/>.
    /// </summary>
    /// <param name="name">The import name value.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <c>null</c>.</exception>
    public ImportDefinitionBuilder Name(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        _importDefinition.Name = name;
        return this;
    }

    /// <summary>
    /// Sets the target table for the <see cref="ImportDefinition"/>.
    /// </summary>
    /// <param name="name">The target table name.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <c>null</c>.</exception>
    public ImportDefinitionBuilder TargetTable(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        _importDefinition.TargetTable = name;
        return this;
    }

    /// <summary>
    /// Sets whether data can be inserted into the target table for the <see cref="ImportDefinition"/>.
    /// </summary>
    /// <param name="value"><c>true</c> to allow inserts; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for chaining.</returns>
    public ImportDefinitionBuilder CanInsert(bool value = true)
    {
        _importDefinition.CanInsert = value;
        return this;
    }

    /// <summary>
    /// Sets whether data can be updated in the target table for the <see cref="ImportDefinition"/>.
    /// </summary>
    /// <param name="value"><c>true</c> to allow updates; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for chaining.</returns>
    public ImportDefinitionBuilder CanUpdate(bool value = true)
    {
        _importDefinition.CanUpdate = value;
        return this;
    }

    /// <summary>
    /// Adds a new field mapping to the import definition using the specified builder action.
    /// </summary>
    /// <param name="builder">The action to configure the <see cref="FieldDefinitionBuilder"/>.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    public ImportDefinitionBuilder Field(Action<FieldDefinitionBuilder> builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        var fieldMapping = new FieldDefinition();
        var fieldBuilder = new FieldDefinitionBuilder(fieldMapping);
        builder(fieldBuilder);

        if (fieldMapping.DataType == null)
            fieldMapping.DataType = typeof(string); // default type

        _importDefinition.Fields.Add(fieldMapping);

        return this;
    }

    /// <summary>
    /// Adds or updates a field mapping for the specified field name using the provided builder action.
    /// </summary>
    /// <param name="fieldName">The name of the field to map.</param>
    /// <param name="builder">The action to configure the <see cref="FieldDefinitionBuilder"/>.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName"/> is <c>null</c> or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    public ImportDefinitionBuilder Field(string fieldName, Action<FieldDefinitionBuilder> builder)
    {
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentException($"'{nameof(fieldName)}' cannot be null or empty.", nameof(fieldName));

        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        var fieldMapping = _importDefinition.Fields.FirstOrDefault(m => m.Name == fieldName);
        if (fieldMapping == null)
        {
            fieldMapping = new FieldDefinition { Name = fieldName };
            _importDefinition.Fields.Add(fieldMapping);
        }

        var fieldBuilder = new FieldDefinitionBuilder(fieldMapping);
        builder(fieldBuilder);

        // default type
        if (fieldMapping.DataType == null)
            fieldMapping.DataType = typeof(string);

        return this;
    }

    /// <summary>
    /// Gets or creates a <see cref="FieldDefinitionBuilder"/> for the specified field name.
    /// </summary>
    /// <param name="fieldName">The name of the field to map.</param>
    /// <returns>A <see cref="FieldDefinitionBuilder"/> for the specified field.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName"/> is <c>null</c> or empty.</exception>
    public FieldDefinitionBuilder Field(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentException($"'{nameof(fieldName)}' cannot be null or empty.", nameof(fieldName));

        var fieldMapping = _importDefinition.Fields.FirstOrDefault(m => m.Name == fieldName);
        if (fieldMapping == null)
        {
            fieldMapping = new FieldDefinition { Name = fieldName };
            _importDefinition.Fields.Add(fieldMapping);
        }

        return new FieldDefinitionBuilder(fieldMapping);
    }

    /// <summary>
    /// Sets the maximum number of errors allowed for the <see cref="ImportDefinition"/>.
    /// </summary>
    /// <param name="value">The maximum number of errors.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for chaining.</returns>
    public ImportDefinitionBuilder MaxErrors(int value)
    {
        _importDefinition.MaxErrors = value;
        return this;
    }

    /// <summary>
    /// Sets the import validator type for the <see cref="ImportDefinition"/>.
    /// </summary>
    /// <typeparam name="T">The type of validator implementing <see cref="IImportValidator"/>.</typeparam>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for chaining.</returns>
    public ImportDefinitionBuilder Validator<T>()
        where T : IImportValidator
    {
        _importDefinition.Validator = typeof(T);
        return this;
    }

    /// <summary>
    /// Sets the import validator type for the <see cref="ImportDefinition"/>.
    /// </summary>
    /// <param name="type">The validator <see cref="Type"/>.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for chaining.</returns>
    public ImportDefinitionBuilder Validator(Type type)
    {
        _importDefinition.Validator = type;
        return this;
    }

}
