using System.Globalization;

#nullable enable

namespace FluentCommand.Extensions;

/// <summary>
/// Provides extension methods for safely converting string values to various base data types, supporting nullable types and culture-specific formatting.
/// </summary>
public static class ConvertExtensions
{
    /// <summary>
    /// Converts the specified string representation of a logical value to its Boolean equivalent.
    /// Accepts common true/false representations such as "true", "false", "yes", "no", "1", "0", "on", "off", "y", "n", "t", "f", and is case-insensitive.
    /// </summary>
    /// <param name="value">A string containing the logical value to convert.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="value"/> represents a true value; otherwise, <c>false</c>.
    /// </returns>
    public static bool ToBoolean(this string? value)
    {
        if (value == null)
            return false;

        if (bool.TryParse(value, out var result))
            return result;

        string v = value.Trim();

        if (string.Equals(v, "t", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "yes", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "x", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "on", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 8-bit unsigned integer.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <returns>
    /// An 8-bit unsigned integer equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static byte? ToByte(this string? value)
    {
        if (value == null)
            return null;

        if (byte.TryParse(value, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 8-bit unsigned integer, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <param name="provider">An object supplying culture-specific formatting information.</param>
    /// <returns>
    /// An 8-bit unsigned integer equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static byte? ToByte(this string? value, IFormatProvider provider)
    {
        if (value == null)
            return null;

        if (byte.TryParse(value, NumberStyles.Integer, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a date and time to an equivalent <see cref="DateTime"/> value.
    /// Attempts multiple common date formats.
    /// </summary>
    /// <param name="value">The string representation of a date and time.</param>
    /// <returns>
    /// The <see cref="DateTime"/> equivalent of <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static DateTime? ToDateTime(this string? value)
    {
        if (value == null)
            return null;

        if (DateTime.TryParse(value, out var result))
            return result;

        if (DateTime.TryParseExact(value, "M/d/yyyy hh:mm:ss tt", CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
            return result;

        if (DateTime.TryParseExact(value, "M/d/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a date and time to an equivalent <see cref="DateTime"/> value, using the specified culture-specific formatting information.
    /// Attempts multiple common date formats.
    /// </summary>
    /// <param name="value">A string containing a date and time to convert.</param>
    /// <param name="provider">An object supplying culture-specific formatting information.</param>
    /// <returns>
    /// The <see cref="DateTime"/> equivalent of <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static DateTime? ToDateTime(this string? value, IFormatProvider provider)
    {
        if (value == null)
            return null;

        if (DateTime.TryParse(value, out var result))
            return result;

        if (DateTime.TryParseExact(value, "M/d/yyyy hh:mm:ss tt", provider, DateTimeStyles.None, out result))
            return result;

        if (DateTime.TryParseExact(value, "M/d/yyyy", provider, DateTimeStyles.None, out result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent decimal number.
    /// </summary>
    /// <param name="value">A string containing a number to convert.</param>
    /// <returns>
    /// A <see cref="decimal"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static decimal? ToDecimal(this string? value)
    {
        if (value == null)
            return null;

        if (decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent decimal number, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string containing a number to convert.</param>
    /// <param name="provider">An object supplying culture-specific formatting information.</param>
    /// <returns>
    /// A <see cref="decimal"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static decimal? ToDecimal(this string? value, IFormatProvider provider)
    {
        if (value == null)
            return null;

        if (decimal.TryParse(value, NumberStyles.Currency, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent double-precision floating-point number.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <returns>
    /// A <see cref="double"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static double? ToDouble(this string? value)
    {
        if (value == null)
            return null;

        if (double.TryParse(value, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent double-precision floating-point number, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <param name="provider">An object supplying culture-specific formatting information.</param>
    /// <returns>
    /// A <see cref="double"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static double? ToDouble(this string? value, IFormatProvider provider)
    {
        if (value == null)
            return null;

        if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 16-bit signed integer.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <returns>
    /// A <see cref="short"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static short? ToInt16(this string? value)
    {
        if (value == null)
            return null;

        if (short.TryParse(value, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 16-bit signed integer, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <param name="provider">An object supplying culture-specific formatting information.</param>
    /// <returns>
    /// A <see cref="short"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static short? ToInt16(this string? value, IFormatProvider provider)
    {
        if (value == null)
            return null;

        if (short.TryParse(value, NumberStyles.Integer, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 32-bit signed integer.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <returns>
    /// An <see cref="int"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static int? ToInt32(this string? value)
    {
        if (value == null)
            return null;

        if (int.TryParse(value, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 32-bit signed integer, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <param name="provider">An object supplying culture-specific formatting information.</param>
    /// <returns>
    /// An <see cref="int"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static int? ToInt32(this string? value, IFormatProvider provider)
    {
        if (value == null)
            return null;

        if (int.TryParse(value, NumberStyles.Integer, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 64-bit signed integer.
    /// </summary>
    /// <param name="value">A string containing a number to convert.</param>
    /// <returns>
    /// A <see cref="long"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static long? ToInt64(this string? value)
    {
        if (value == null)
            return null;

        if (long.TryParse(value, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 64-bit signed integer, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <param name="provider">An object supplying culture-specific formatting information.</param>
    /// <returns>
    /// A <see cref="long"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static long? ToInt64(this string? value, IFormatProvider provider)
    {
        if (value == null)
            return null;

        if (long.TryParse(value, NumberStyles.Integer, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent single-precision floating-point number.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <returns>
    /// A <see cref="float"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static float? ToSingle(this string? value)
    {
        if (value == null)
            return null;

        if (float.TryParse(value, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent single-precision floating-point number, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <param name="provider">An object supplying culture-specific formatting information.</param>
    /// <returns>
    /// A <see cref="float"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static float? ToSingle(this string? value, IFormatProvider provider)
    {
        if (value == null)
            return null;

        if (float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 16-bit unsigned integer.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <returns>
    /// A <see cref="ushort"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static ushort? ToUInt16(this string? value)
    {
        if (value == null)
            return null;

        if (ushort.TryParse(value, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 16-bit unsigned integer, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <param name="provider">An object supplying culture-specific formatting information.</param>
    /// <returns>
    /// A <see cref="ushort"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static ushort? ToUInt16(this string? value, IFormatProvider provider)
    {
        if (value == null)
            return null;

        if (ushort.TryParse(value, NumberStyles.Integer, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 32-bit unsigned integer.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <returns>
    /// A <see cref="uint"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static uint? ToUInt32(this string? value)
    {
        if (value == null)
            return null;

        if (uint.TryParse(value, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 32-bit unsigned integer, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <param name="provider">An object supplying culture-specific formatting information.</param>
    /// <returns>
    /// A <see cref="uint"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static uint? ToUInt32(this string? value, IFormatProvider provider)
    {
        if (value == null)
            return null;

        if (uint.TryParse(value, NumberStyles.Integer, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 64-bit unsigned integer.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <returns>
    /// A <see cref="ulong"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static ulong? ToUInt64(this string? value)
    {
        if (value == null)
            return null;

        if (ulong.TryParse(value, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 64-bit unsigned integer, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <param name="provider">An object supplying culture-specific formatting information.</param>
    /// <returns>
    /// A <see cref="ulong"/> equivalent to <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static ulong? ToUInt64(this string? value, IFormatProvider provider)
    {
        if (value == null)
            return null;

        if (ulong.TryParse(value, NumberStyles.Integer, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string to an equivalent <see cref="TimeSpan"/> value.
    /// </summary>
    /// <param name="value">The string representation of a <see cref="TimeSpan"/>.</param>
    /// <returns>
    /// The <see cref="TimeSpan"/> equivalent of <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static TimeSpan? ToTimeSpan(this string? value)
    {
        if (value == null)
            return null;

        if (TimeSpan.TryParse(value, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string to an equivalent <see cref="Guid"/> value.
    /// </summary>
    /// <param name="value">The string representation of a <see cref="Guid"/>.</param>
    /// <returns>
    /// The <see cref="Guid"/> equivalent of <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static Guid? ToGuid(this string? value)
    {
        if (value == null)
            return null;

        if (Guid.TryParse(value, out var result))
            return result;

        return null;
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Converts the specified string to an equivalent <see cref="DateOnly"/> value.
    /// Attempts to parse as <see cref="DateOnly"/>, <see cref="DateTime"/>, or <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="value">The string representation of a <see cref="DateOnly"/>.</param>
    /// <returns>
    /// The <see cref="DateOnly"/> equivalent of <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static DateOnly? ToDateOnly(this string? value)
    {
        if (value == null)
            return null;

        if (DateOnly.TryParse(value, out var dateOnly))
            return dateOnly;

        if (DateTime.TryParse(value, out var dateTime))
            return DateOnly.FromDateTime(dateTime);

        if (DateTimeOffset.TryParse(value, out var dateTimeOffset))
            return DateOnly.FromDateTime(dateTimeOffset.DateTime);

        return null;
    }

    /// <summary>
    /// Converts the specified string to an equivalent <see cref="TimeOnly"/> value.
    /// Attempts to parse as <see cref="TimeOnly"/>, <see cref="TimeSpan"/>, <see cref="DateTime"/>, or <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="value">The string representation of a <see cref="TimeOnly"/>.</param>
    /// <returns>
    /// The <see cref="TimeOnly"/> equivalent of <paramref name="value"/>, or <c>null</c> if conversion fails or <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static TimeOnly? ToTimeOnly(this string? value)
    {
        if (value == null)
            return null;

        if (TimeOnly.TryParse(value, out var timeOnly))
            return timeOnly;

        if (TimeSpan.TryParse(value, out var timeSpan))
            return TimeOnly.FromTimeSpan(timeSpan);

        if (DateTime.TryParse(value, out var dateTime))
            return TimeOnly.FromDateTime(dateTime);

        if (DateTimeOffset.TryParse(value, out var dateTimeOffset))
            return TimeOnly.FromDateTime(dateTimeOffset.DateTime);

        return null;
    }
#endif

    /// <summary>
    /// Safely converts the specified string input to the given <paramref name="type"/>.
    /// Supports all common base types, including nullable types, and returns <c>null</c> for empty or invalid input if the type is nullable.
    /// </summary>
    /// <param name="type">The target type to convert to.</param>
    /// <param name="input">The string input to convert.</param>
    /// <returns>
    /// The converted value as an object, or <c>null</c> if conversion fails and the type is nullable; otherwise, the default value for the type.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
    public static object? SafeConvert(Type type, string? input)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        // first try string
        if (type == typeof(string))
        {
            return input;
        }

        var isNullable = type.IsNullable();
        if ((input?.IsNullOrEmpty() != false) && isNullable)
        {
            return null;
        }

        input = input?.Trim();
        var underlyingType = type.GetUnderlyingType();

        // convert by type
        if (underlyingType == typeof(bool))
        {
            return input.ToBoolean();
        }
        if (underlyingType == typeof(byte))
        {
            var value = input.ToByte();
            return value ?? (isNullable ? null : 0);
        }
        if (underlyingType == typeof(DateTime))
        {
            var value = input.ToDateTime();
            return value ?? (isNullable ? null : DateTime.MinValue);
        }
        if (underlyingType == typeof(decimal))
        {
            var value = input.ToDecimal();
            return value ?? (isNullable ? null : 0);
        }
        if (underlyingType == typeof(double))
        {
            var value = input.ToDouble();
            return value ?? (isNullable ? null : 0);
        }
        if (underlyingType == typeof(short))
        {
            var value = input.ToInt16();
            return value ?? (isNullable ? null : 0);
        }
        if (underlyingType == typeof(int))
        {
            var value = input.ToInt32();
            return value ?? (isNullable ? null : 0);
        }
        if (underlyingType == typeof(long))
        {
            var value = input.ToInt64();
            return value ?? (isNullable ? null : 0);
        }
        if (underlyingType == typeof(float))
        {
            var value = input.ToSingle();
            return value ?? (isNullable ? null : 0);
        }
        if (underlyingType == typeof(ushort))
        {
            var value = input.ToUInt16();
            return value ?? (isNullable ? null : 0);
        }
        if (underlyingType == typeof(uint))
        {
            var value = input.ToUInt32();
            return value ?? (isNullable ? null : 0);
        }
        if (underlyingType == typeof(ulong))
        {
            var value = input.ToUInt64();
            return value ?? (isNullable ? null : 0);
        }
        if (underlyingType == typeof(TimeSpan))
        {
            var value = input.ToTimeSpan();
            return value ?? (isNullable ? null : TimeSpan.Zero);
        }
        if (underlyingType == typeof(Guid))
        {
            var value = input.ToGuid();
            return value ?? (isNullable ? null : Guid.Empty);
        }
#if NET6_0_OR_GREATER
        if (underlyingType == typeof(DateOnly))
        {
            var value = input.ToDateOnly();
            return value ?? (isNullable ? null : DateOnly.MinValue);
        }
        if (underlyingType == typeof(TimeOnly))
        {
            var value = input.ToTimeOnly();
            return value ?? (isNullable ? null : TimeOnly.MinValue);
        }
#endif
        return default;
    }

    /// <summary>
    /// Converts the specified result object to the given <typeparamref name="TValue"/> type.
    /// Supports custom conversion functions and safe conversion from string or other types.
    /// </summary>
    /// <typeparam name="TValue">The target type to convert to.</typeparam>
    /// <param name="result">The result object to convert.</param>
    /// <param name="convert">An optional custom conversion function.</param>
    /// <returns>
    /// The converted value as <typeparamref name="TValue"/>, or the default value if conversion fails.
    /// </returns>
    public static TValue? ConvertValue<TValue>(object? result, Func<object?, TValue>? convert = null)
    {
        if (result is null || result == DBNull.Value)
            return default;

        if (result is TValue valueType)
            return valueType;

        if (convert != null)
            return convert(result);

        if (result is string stringValue)
            return (TValue?)SafeConvert(typeof(TValue), stringValue);

        try
        {
            return (TValue)Convert.ChangeType(result, typeof(TValue));
        }
        catch
        {
            return default;
        }
    }
}
