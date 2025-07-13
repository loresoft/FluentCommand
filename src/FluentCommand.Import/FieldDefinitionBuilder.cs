namespace FluentCommand.Import;

/// <summary>
/// Provides a fluent API to configure and build a <see cref="FieldDefinition"/> for import operations.
/// </summary>
public class FieldDefinitionBuilder
{
    private readonly FieldDefinition _fieldDefinition;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldDefinitionBuilder"/> class.
    /// </summary>
    /// <param name="fieldDefinition">The <see cref="FieldDefinition"/> to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldDefinition"/> is <c>null</c>.</exception>
    public FieldDefinitionBuilder(FieldDefinition fieldDefinition)
    {
        if (fieldDefinition == null)
            throw new ArgumentNullException(nameof(fieldDefinition));

        _fieldDefinition = fieldDefinition;
    }

    /// <summary>
    /// Sets the unique field name for the <see cref="FieldDefinition"/>.
    /// </summary>
    /// <param name="value">The field name value.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public FieldDefinitionBuilder FieldName(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _fieldDefinition.Name = value;

        return this;
    }

    /// <summary>
    /// Sets the display name for the <see cref="FieldDefinition"/>, used for UI or reporting purposes.
    /// </summary>
    /// <param name="value">The display name value.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public FieldDefinitionBuilder DisplayName(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _fieldDefinition.DisplayName = value;

        return this;
    }

    /// <summary>
    /// Sets the data type for the <see cref="FieldDefinition"/>.
    /// </summary>
    /// <param name="value">The <see cref="Type"/> representing the field's data type.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public FieldDefinitionBuilder DataType(Type value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _fieldDefinition.DataType = value;

        return this;
    }

    /// <summary>
    /// Sets the data type for the <see cref="FieldDefinition"/> using a generic type parameter.
    /// </summary>
    /// <typeparam name="T">The type to use as the field's data type.</typeparam>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for chaining.</returns>
    public FieldDefinitionBuilder DataType<T>()
    {
        _fieldDefinition.DataType = typeof(T);
        return this;
    }

    /// <summary>
    /// Sets whether the field is a key field.
    /// When set to <c>true</c>, the field is marked as required and cannot be updated.
    /// </summary>
    /// <param name="value"><c>true</c> to mark the field as a key; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for chaining.</returns>
    public FieldDefinitionBuilder IsKey(bool value = true)
    {
        _fieldDefinition.IsKey = value;

        if (!value)
            return this;

        // defaults for a key field
        _fieldDefinition.IsRequired = true;
        _fieldDefinition.CanUpdate = false;

        return this;
    }

    /// <summary>
    /// Sets whether the field can be inserted during import.
    /// </summary>
    /// <param name="value"><c>true</c> if the field can be inserted; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for chaining.</returns>
    public FieldDefinitionBuilder CanInsert(bool value = true)
    {
        _fieldDefinition.CanInsert = value;
        return this;
    }

    /// <summary>
    /// Sets whether the field can be updated during import.
    /// </summary>
    /// <param name="value"><c>true</c> if the field can be updated; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for chaining.</returns>
    public FieldDefinitionBuilder CanUpdate(bool value = true)
    {
        _fieldDefinition.CanUpdate = value;
        return this;
    }

    /// <summary>
    /// Sets whether the field can be mapped by users during import configuration.
    /// </summary>
    /// <param name="value"><c>true</c> if the field can be mapped; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for chaining.</returns>
    public FieldDefinitionBuilder CanMap(bool value = true)
    {
        _fieldDefinition.CanMap = value;
        return this;
    }

    /// <summary>
    /// Sets whether the field is required for import.
    /// </summary>
    /// <param name="value"><c>true</c> if the field is required; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for chaining.</returns>
    public FieldDefinitionBuilder Required(bool value = true)
    {
        _fieldDefinition.IsRequired = value;
        return this;
    }

    /// <summary>
    /// Sets the default value behavior for this field.
    /// </summary>
    /// <param name="value">The <see cref="FieldDefault"/> option specifying how the default value is determined.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for chaining.</returns>
    public FieldDefinitionBuilder Default(FieldDefault? value)
    {
        _fieldDefinition.Default = value;

        if (value.HasValue && value != FieldDefault.Static)
            _fieldDefinition.CanMap = false;

        return this;
    }

    /// <summary>
    /// Sets a static default value for this field.
    /// </summary>
    /// <param name="value">The static default value to use for the field.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for chaining.</returns>
    public FieldDefinitionBuilder Default(object value)
    {
        _fieldDefinition.DefaultValue = value;
        _fieldDefinition.Default = FieldDefault.Static;

        if (!Equals(value, null))
            _fieldDefinition.CanMap = false;

        return this;
    }

    /// <summary>
    /// Adds a match regular expression to the <see cref="FieldDefinition"/> for mapping or validation.
    /// </summary>
    /// <param name="value">The regular expression pattern.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public FieldDefinitionBuilder Expression(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _fieldDefinition.Expressions.Add(value);

        return this;
    }

    /// <summary>
    /// Adds multiple match regular expressions to the <see cref="FieldDefinition"/> for mapping or validation.
    /// </summary>
    /// <param name="values">A collection of regular expression patterns.</param>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
    public FieldDefinitionBuilder Expressions(IEnumerable<string> values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        _fieldDefinition.Expressions.AddRange(values);

        return this;
    }

    /// <summary>
    /// Sets the field translator type for the <see cref="FieldDefinition"/>, used to transform or convert field values during import.
    /// </summary>
    /// <typeparam name="T">The type of translator implementing <see cref="IFieldTranslator"/>.</typeparam>
    /// <returns>The current <see cref="FieldDefinitionBuilder"/> instance for chaining.</returns>
    public FieldDefinitionBuilder Translator<T>()
        where T : IFieldTranslator
    {
        _fieldDefinition.Translator = typeof(T);
        return this;
    }

}
