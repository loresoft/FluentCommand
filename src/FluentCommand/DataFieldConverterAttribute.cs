namespace FluentCommand;

/// <summary>
/// Attribute to enable source generation of data reader factory
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class DataFieldConverterAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataFieldConverterAttribute"/> class.
    /// </summary>
    /// <param name="converterType">Type of the converter.</param>
    public DataFieldConverterAttribute(Type converterType)
    {
        ConverterType = converterType;
    }

    /// <summary>
    /// Gets the type of the converter.
    /// </summary>
    /// <value>
    /// The type of the converter.
    /// </value>
    public Type ConverterType { get; }
}

#if NET7_0_OR_GREATER
/// <summary>
/// Attribute to enable source generation of data reader factory
/// </summary>
/// <typeparam name="TConverter">
/// The type of the converter
/// </typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class DataFieldConverterAttribute<TConverter> : DataFieldConverterAttribute
{
    public DataFieldConverterAttribute() : base(typeof(TConverter))
    {
    }
}
#endif
