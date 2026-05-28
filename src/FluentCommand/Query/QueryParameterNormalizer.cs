namespace FluentCommand.Query;

/// <summary>
/// Normalizes query parameter values and types before parameter creation.
/// </summary>
internal static class QueryParameterNormalizer
{
    /// <summary>
    /// Normalizes enum and nullable enum values to their numeric underlying value and type.
    /// </summary>
    /// <param name="value">The parameter value.</param>
    /// <param name="type">The declared parameter type.</param>
    /// <returns>
    /// A tuple containing the normalized value and type.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
    public static (object? Value, Type Type) Normalize(object? value, Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var enumType = Nullable.GetUnderlyingType(type) ?? type;
        if (!enumType.IsEnum)
            return (value, type);

        var underlyingType = Enum.GetUnderlyingType(enumType);
        if (value is null)
            return (null, underlyingType);

        return (Convert.ChangeType(value, underlyingType), underlyingType);
    }
}
