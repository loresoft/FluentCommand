namespace FluentCommand.Import;

/// <summary>
/// Fluent builder for <see cref="FieldDefinition"/>
/// </summary>
public class FieldDefinitionBuilder
{
    private readonly FieldDefinition _fieldDefinition;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldDefinition"/> class.
    /// </summary>
    /// <param name="fieldDefinition">The field definition.</param>
    /// <exception cref="ArgumentNullException">fieldDefinition</exception>
    public FieldDefinitionBuilder(FieldDefinition fieldDefinition)
    {
        if (fieldDefinition == null)
            throw new ArgumentNullException(nameof(fieldDefinition));

        _fieldDefinition = fieldDefinition;
    }

    /// <summary>
    /// Sets the field name for the <see cref="FieldDefinition"/>
    /// </summary>
    /// <param name="value">The field name value.</param>
    /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
    /// <exception cref="ArgumentNullException">when value is null</exception>
    public FieldDefinitionBuilder FieldName(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _fieldDefinition.Name = value;

        return this;
    }

    /// <summary>
    /// Sets the display name for the <see cref="FieldDefinition"/>
    /// </summary>
    /// <param name="value">The display name value.</param>
    /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
    /// <exception cref="ArgumentNullException">when value is null</exception>
    public FieldDefinitionBuilder DisplayName(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _fieldDefinition.DisplayName = value;

        return this;
    }

    /// <summary>
    /// Sets the data type for the <see cref="FieldDefinition"/>
    /// </summary>
    /// <param name="value">The data type value.</param>
    /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
    /// <exception cref="ArgumentNullException">when value is null</exception>
    public FieldDefinitionBuilder DataType(Type value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _fieldDefinition.DataType = value;

        return this;
    }

    /// <summary>
    /// Sets the data type for the <see cref="FieldDefinition" />
    /// </summary>
    /// <typeparam name="T">The field type</typeparam>
    /// <returns>
    /// Fluent builder for <see cref="FieldDefinition" />
    /// </returns>
    public FieldDefinitionBuilder DataType<T>()
    {
        _fieldDefinition.DataType = typeof(T);
        return this;
    }

    /// <summary>
    /// Sets whether the field is a key.
    /// </summary>
    /// <param name="value">if set to <c>true</c> field is key.</param>
    /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
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
    /// Set whether the field can insert a value.
    /// </summary>
    /// <param name="value">if set to <c>true</c> field can insert.</param>
    /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
    public FieldDefinitionBuilder CanInsert(bool value = true)
    {
        _fieldDefinition.CanInsert = value;
        return this;
    }

    /// <summary>
    /// Set whether the field can update a value.
    /// </summary>
    /// <param name="value">if set to <c>true</c> field can update.</param>
    /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
    public FieldDefinitionBuilder CanUpdate(bool value = true)
    {
        _fieldDefinition.CanUpdate = value;
        return this;
    }

    /// <summary>
    /// Set whether the field can be mapped.
    /// </summary>
    /// <param name="value">if set to <c>true</c> field can be mapped.</param>
    /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
    public FieldDefinitionBuilder CanMap(bool value = true)
    {
        _fieldDefinition.CanMap = value;
        return this;
    }

    /// <summary>
    /// Sets the field as required for the <see cref="FieldDefinition"/>
    /// </summary>
    /// <param name="value">The required field value.</param>
    /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
    public FieldDefinitionBuilder Required(bool value = true)
    {
        _fieldDefinition.IsRequired = value;
        return this;
    }

    /// <summary>
    /// Sets the default value for this field.
    /// </summary>
    /// <param name="value">The type of field default to use.</param>
    /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
    public FieldDefinitionBuilder Default(FieldDefault? value)
    {
        _fieldDefinition.Default = value;

        if (value.HasValue && value != FieldDefault.Static)
            _fieldDefinition.CanMap = false;

        return this;
    }

    /// <summary>
    /// Sets the default value for this field.
    /// </summary>
    /// <param name="value">The default value to use.</param>
    /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
    public FieldDefinitionBuilder Default(object value)
    {
        _fieldDefinition.DefaultValue = value;
        _fieldDefinition.Default = FieldDefault.Static;

        if (!Equals(value, null))
            _fieldDefinition.CanMap = false;

        return this;
    }

    /// <summary>
    /// Sets the match regular expression for the <see cref="FieldDefinition"/>
    /// </summary>
    /// <param name="value">The regular expression value.</param>
    /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
    /// <exception cref="ArgumentNullException">when value is null</exception>
    public FieldDefinitionBuilder Expression(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _fieldDefinition.Expressions.Add(value);

        return this;
    }

    /// <summary>
    /// Sets the match regular expression for the <see cref="FieldDefinition"/>
    /// </summary>
    /// <param name="values">The regular expression values.</param>
    /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
    /// <exception cref="ArgumentNullException">when values is null</exception>
    public FieldDefinitionBuilder Expressions(IEnumerable<string> values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        _fieldDefinition.Expressions.AddRange(values);

        return this;
    }

    /// <summary>
    /// Sets the field translator type for the <see cref="FieldDefinition" />
    /// </summary>
    /// <typeparam name="T">The type of translator</typeparam>
    /// <returns>
    /// Fluent builder for <see cref="FieldDefinition" />
    /// </returns>
    public FieldDefinitionBuilder Translator<T>()
        where T : IFieldTranslator
    {
        _fieldDefinition.Translator = typeof(T);
        return this;
    }

}
