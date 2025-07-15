namespace FluentCommand.Import;

/// <summary>
/// Provides a fluent API for configuring and building an <see cref="Import.ImportDefinition"/> instance.
/// Enables chaining of configuration methods for setting import metadata, target table, field definitions, and validation options.
/// </summary>
public class ImportDefinitionBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImportDefinitionBuilder"/> class.
    /// </summary>
    /// <param name="importDefinition">The <see cref="Import.ImportDefinition"/> to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="importDefinition"/> is <c>null</c>.</exception>
    public ImportDefinitionBuilder(ImportDefinition importDefinition)
    {
        ImportDefinition = importDefinition ?? throw new ArgumentNullException(nameof(importDefinition));
    }

    protected ImportDefinition ImportDefinition { get; }

    /// <summary>
    /// Sets the name of the import operation for the <see cref="Import.ImportDefinition"/>.
    /// </summary>
    /// <param name="name">The name of the import operation.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <c>null</c>.</exception>
    public ImportDefinitionBuilder Name(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        ImportDefinition.Name = name;
        return this;
    }

    /// <summary>
    /// Sets the target table name for the <see cref="Import.ImportDefinition"/>.
    /// </summary>
    /// <param name="name">The name of the target table into which data will be imported.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <c>null</c>.</exception>
    public ImportDefinitionBuilder TargetTable(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        ImportDefinition.TargetTable = name;
        return this;
    }

    /// <summary>
    /// Sets whether new data can be inserted into the target table during import.
    /// </summary>
    /// <param name="value"><c>true</c> to allow inserts; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for method chaining.</returns>
    public ImportDefinitionBuilder CanInsert(bool value = true)
    {
        ImportDefinition.CanInsert = value;
        return this;
    }

    /// <summary>
    /// Sets whether existing data in the target table can be updated during import.
    /// </summary>
    /// <param name="value"><c>true</c> to allow updates; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for method chaining.</returns>
    public ImportDefinitionBuilder CanUpdate(bool value = true)
    {
        ImportDefinition.CanUpdate = value;
        return this;
    }

    /// <summary>
    /// Adds a new field definition to the import configuration using the specified builder action.
    /// </summary>
    /// <param name="builder">An action to configure the <see cref="FieldDefinitionBuilder"/> for the new field.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for method chaining.</returns>
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

        ImportDefinition.Fields.Add(fieldMapping);

        return this;
    }

    /// <summary>
    /// Adds or updates a field definition for the specified field name using the provided builder action.
    /// If the field already exists, it is updated; otherwise, a new field is added.
    /// </summary>
    /// <param name="fieldName">The name of the field to configure.</param>
    /// <param name="builder">An action to configure the <see cref="FieldDefinitionBuilder"/> for the field.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName"/> is <c>null</c> or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    public ImportDefinitionBuilder Field(string fieldName, Action<FieldDefinitionBuilder> builder)
    {
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentException($"'{nameof(fieldName)}' cannot be null or empty.", nameof(fieldName));

        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        var fieldMapping = ImportDefinition.Fields.FirstOrDefault(m => m.Name == fieldName);
        if (fieldMapping == null)
        {
            fieldMapping = new FieldDefinition { Name = fieldName };
            ImportDefinition.Fields.Add(fieldMapping);
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
    /// If the field does not exist, it is created and added to the import definition.
    /// </summary>
    /// <param name="fieldName">The name of the field to configure.</param>
    /// <returns>A <see cref="FieldDefinitionBuilder"/> for the specified field.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName"/> is <c>null</c> or empty.</exception>
    public FieldDefinitionBuilder Field(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentException($"'{nameof(fieldName)}' cannot be null or empty.", nameof(fieldName));

        var fieldMapping = ImportDefinition.Fields.FirstOrDefault(m => m.Name == fieldName);
        if (fieldMapping == null)
        {
            fieldMapping = new FieldDefinition { Name = fieldName };
            ImportDefinition.Fields.Add(fieldMapping);
        }

        return new FieldDefinitionBuilder(fieldMapping);
    }

    /// <summary>
    /// Sets the maximum number of errors allowed before the import operation is aborted.
    /// </summary>
    /// <param name="value">The maximum number of errors permitted during import.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for method chaining.</returns>
    public ImportDefinitionBuilder MaxErrors(int value)
    {
        ImportDefinition.MaxErrors = value;
        return this;
    }

    /// <summary>
    /// Sets the import validator type for the <see cref="Import.ImportDefinition"/> using a generic type parameter.
    /// The validator type must implement <see cref="IImportValidator"/>.
    /// </summary>
    /// <typeparam name="T">The type of validator implementing <see cref="IImportValidator"/>.</typeparam>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for method chaining.</returns>
    [Obsolete("Use ValidatorKey(string) instead.")]
    public ImportDefinitionBuilder Validator<T>()
        where T : IImportValidator
    {
        ImportDefinition.Validator = typeof(T);
        return this;
    }

    /// <summary>
    /// Sets the import validator type for the <see cref="Import.ImportDefinition"/> using a <see cref="Type"/> instance.
    /// The validator type must implement <see cref="IImportValidator"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> of the validator.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for method chaining.</returns>
    [Obsolete("Use ValidatorKey(string) instead.")]
    public ImportDefinitionBuilder Validator(Type type)
    {
        ImportDefinition.Validator = type;
        return this;
    }

    /// <summary>
    /// Sets the service key used to resolve the <see cref="IImportValidator"/> service from the dependency injection container.
    /// The service must be registered in the DI container with this key and implement <see cref="IImportValidator"/>.
    /// </summary>
    /// <param name="key">The service key for the validator.</param>
    /// <returns>The current <see cref="ImportDefinitionBuilder"/> instance for method chaining.</returns>
    public ImportDefinitionBuilder ValidatorKey(string key)
    {
        ImportDefinition.ValidatorKey = key;
        return this;
    }
}
