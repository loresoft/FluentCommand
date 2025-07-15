namespace FluentCommand.Import;

/// <summary>
/// Provides a fluent API for configuring and building a <see cref="Import.FieldDefinition"/> instance for import operations.
/// Enables chaining of configuration methods for setting field metadata, data type, mapping, validation, and transformation options.
/// </summary>
public class FieldDefinitionBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FieldDefinitionBuilder"/> class.
    /// </summary>
    /// <param name="fieldDefinition">The <see cref="Import.FieldDefinition"/> to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldDefinition"/> is <c>null</c>.</exception>
    public FieldDefinitionBuilder(FieldDefinition fieldDefinition)
    {
        if (fieldDefinition == null)
            throw new ArgumentNullException(nameof(fieldDefinition));

        FieldDefinition = fieldDefinition;
    }

    protected FieldDefinition FieldDefinition { get; }

    /// <summary>
    /// Sets the unique field name for the <see cref="Import.FieldDefinition"/>.
    /// </summary>
    /// <param name="value">The unique name of the field.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public FieldDefinitionBuilder FieldName(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        FieldDefinition.Name = value;

        return this;
    }

    /// <summary>
    /// Sets the display name for the <see cref="Import.FieldDefinition"/>, typically used for user interfaces or reporting.
    /// </summary>
    /// <param name="value">The display name of the field.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public FieldDefinitionBuilder DisplayName(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        FieldDefinition.DisplayName = value;

        return this;
    }

    /// <summary>
    /// Sets the data type for the <see cref="Import.FieldDefinition"/>.
    /// </summary>
    /// <param name="value">The <see cref="Type"/> representing the field's data type.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public FieldDefinitionBuilder DataType(Type value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        FieldDefinition.DataType = value;

        return this;
    }

    /// <summary>
    /// Sets the data type for the <see cref="Import.FieldDefinition"/> using a generic type parameter.
    /// </summary>
    /// <typeparam name="T">The type to use as the field's data type.</typeparam>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    public FieldDefinitionBuilder DataType<T>()
    {
        FieldDefinition.DataType = typeof(T);
        return this;
    }

    /// <summary>
    /// Marks the field as a key field, which uniquely identifies records.
    /// When set to <c>true</c>, the field is also marked as required and cannot be updated.
    /// </summary>
    /// <param name="value"><c>true</c> to mark the field as a key; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    public FieldDefinitionBuilder IsKey(bool value = true)
    {
        FieldDefinition.IsKey = value;

        if (!value)
            return this;

        // defaults for a key field
        FieldDefinition.IsRequired = true;
        FieldDefinition.CanUpdate = false;

        return this;
    }

    /// <summary>
    /// Sets whether the field can be inserted during import operations.
    /// </summary>
    /// <param name="value"><c>true</c> if the field can be inserted; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    public FieldDefinitionBuilder CanInsert(bool value = true)
    {
        FieldDefinition.CanInsert = value;
        return this;
    }

    /// <summary>
    /// Sets whether the field can be updated during import operations.
    /// </summary>
    /// <param name="value"><c>true</c> if the field can be updated; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    public FieldDefinitionBuilder CanUpdate(bool value = true)
    {
        FieldDefinition.CanUpdate = value;
        return this;
    }

    /// <summary>
    /// Sets whether the field can be mapped by users during import configuration.
    /// </summary>
    /// <param name="value"><c>true</c> if the field can be mapped; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    public FieldDefinitionBuilder CanMap(bool value = true)
    {
        FieldDefinition.CanMap = value;
        return this;
    }

    /// <summary>
    /// Sets whether the field is required for import. Required fields must be provided in the import data.
    /// </summary>
    /// <param name="value"><c>true</c> if the field is required; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    public FieldDefinitionBuilder Required(bool value = true)
    {
        FieldDefinition.IsRequired = value;
        return this;
    }

    /// <summary>
    /// Sets the default value behavior for this field, as specified by <see cref="FieldDefault"/>.
    /// </summary>
    /// <param name="value">The <see cref="FieldDefault"/> option that determines how the default value is assigned.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    public FieldDefinitionBuilder Default(FieldDefault? value)
    {
        FieldDefinition.Default = value;

        if (value.HasValue && value != FieldDefault.Static)
            FieldDefinition.CanMap = false;

        return this;
    }

    /// <summary>
    /// Sets a static default value for this field, used when <see cref="FieldDefault.Static"/> is selected.
    /// </summary>
    /// <param name="value">The static default value to assign to the field if no value is provided during import.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    public FieldDefinitionBuilder Default(object value)
    {
        FieldDefinition.DefaultValue = value;
        FieldDefinition.Default = FieldDefault.Static;

        if (!Equals(value, null))
            FieldDefinition.CanMap = false;

        return this;
    }

    /// <summary>
    /// Adds a match or validation regular expression to the <see cref="Import.FieldDefinition"/>.
    /// These expressions can be used for mapping or validating field values during import.
    /// </summary>
    /// <param name="value">The regular expression pattern.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public FieldDefinitionBuilder Expression(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        FieldDefinition.Expressions.Add(value);

        return this;
    }

    /// <summary>
    /// Adds multiple match or validation regular expressions to the <see cref="Import.FieldDefinition"/>.
    /// These expressions can be used for mapping or validating field values during import.
    /// </summary>
    /// <param name="values">A collection of regular expression patterns.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
    public FieldDefinitionBuilder Expressions(IEnumerable<string> values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        FieldDefinition.Expressions.AddRange(values);

        return this;
    }

    /// <summary>
    /// Sets the field translator type for the <see cref="Import.FieldDefinition"/>, used to transform or convert field values during import.
    /// </summary>
    /// <typeparam name="T">The type of translator implementing <see cref="IFieldTranslator"/>.</typeparam>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    [Obsolete("Use TranslatorKey(string) instead. This will be removed in a future version.")]
    public FieldDefinitionBuilder Translator<T>()
        where T : IFieldTranslator
    {
        FieldDefinition.Translator = typeof(T);
        return this;
    }

    /// <summary>
    /// Sets the dependency injection service key for accessing the field translator, which is used to transform or convert field values during import.
    /// The service must be registered in the DI container with this key and implement <see cref="IFieldTranslator"/>.
    /// </summary>
    /// <param name="value">The service key for the field translator.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for method chaining.</returns>
    public FieldDefinitionBuilder TranslatorKey(string value)
    {
        FieldDefinition.TranslatorKey = value;
        return this;
    }
}
