namespace FluentCommand.Internal;

/// <summary>
/// Represents an immutable structure for combining and generating hash codes.
/// </summary>
/// <remarks>
/// Implements the Jon Skeet recommended pattern for <see cref="object.GetHashCode()"/>.
/// Provides deterministic and composable hash code generation for complex objects.
/// </remarks>
public readonly struct HashCode : IFormattable, IEquatable<HashCode>
{
    /// <summary>
    /// The prime multiplier used to combine hash codes.
    /// </summary>
    public const int Multiplier = 31;

    /// <summary>
    /// The default seed value used for initializing random number generators or hash functions.
    /// </summary>
    public const int DefaultSeed = 17;

    private readonly int _hashCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="HashCode"/> struct with the specified hash code value.
    /// </summary>
    /// <param name="hashCode">The initial hash code value.</param>
    public HashCode(int hashCode)
    {
        _hashCode = hashCode;
    }

    /// <summary>
    /// Gets the initial seed value for combining hash codes.
    /// </summary>
    /// <value>
    /// The initial hash code seed value.
    /// </value>
    public static HashCode Seed => new(DefaultSeed);

    /// <summary>
    /// Combines this hash code with the hash code of the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to combine.</typeparam>
    /// <param name="value">The value whose hash code will be combined.</param>
    /// <returns>
    /// A new <see cref="HashCode"/> instance representing the combined hash code.
    /// </returns>
    public HashCode Combine<TValue>(TValue value)
    {
        var hashCode = value is null ? 0 : EqualityComparer<TValue>.Default.GetHashCode(value);
        unchecked
        {
            hashCode = (_hashCode * Multiplier) + hashCode;
        }

        return new HashCode(hashCode);
    }

    /// <summary>
    /// Combines this hash code with the deterministic hash code of the specified string value.
    /// </summary>
    /// <param name="value">The string value whose hash code will be combined.</param>
    /// <returns>
    /// A new <see cref="HashCode"/> instance representing the combined hash code.
    /// </returns>
    public HashCode Combine(string value)
    {
        // need to handle string values deterministically
        var hashCode = HashString(value);
        unchecked
        {
            hashCode = (_hashCode * Multiplier) + hashCode;
        }

        return new HashCode(hashCode);
    }

    /// <summary>
    /// Combines this hash code with the hash code of the specified object.
    /// If the object is a string, uses a deterministic string hash; otherwise, uses the object's hash code.
    /// </summary>
    /// <param name="value">The object whose hash code will be combined.</param>
    /// <returns>
    /// A new <see cref="HashCode"/> instance representing the combined hash code.
    /// </returns>
    public HashCode Combine(object value)
    {
        // need to handle string values deterministically
        return value switch
        {
            string text => Combine(text),
            _ => Combine(value?.GetHashCode() ?? 0),
        };
    }

    /// <summary>
    /// Combines this hash code with the hash codes of all items in the specified sequence.
    /// </summary>
    /// <typeparam name="TValue">The type of the values to combine.</typeparam>
    /// <param name="values">The sequence of values whose hash codes will be combined.</param>
    /// <returns>
    /// A new <see cref="HashCode"/> instance representing the combined hash code.
    /// </returns>
    public HashCode CombineAll<TValue>(IEnumerable<TValue> values)
    {
        if (values == null)
            return this;

        var comparer = EqualityComparer<TValue>.Default;
        var current = _hashCode;

        foreach (var value in values)
        {
            var hashCode = value switch
            {
                string text => HashString(text),
                TValue instance => comparer.GetHashCode(instance),
                _ => 0
            };

            unchecked
            {
                hashCode = (current * Multiplier) + hashCode;
            }

            current = hashCode;
        }

        return new HashCode(current);
    }

    /// <summary>
    /// Returns the hash code value for this instance.
    /// </summary>
    /// <returns>
    /// The hash code value for this instance.
    /// </returns>
    public override int GetHashCode() => _hashCode;

    /// <summary>
    /// Converts the numeric value of this instance to its equivalent string representation.
    /// </summary>
    /// <returns>
    /// The string representation of the hash code value.
    /// </returns>
    public override string ToString() => _hashCode.ToString();

    /// <summary>
    /// Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>
    /// The string representation of the hash code value as specified by <paramref name="provider"/>.
    /// </returns>
    public string ToString(IFormatProvider provider) => _hashCode.ToString(provider);

    /// <summary>
    /// Converts the numeric value of this instance to its equivalent string representation using the specified format.
    /// </summary>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <returns>
    /// The string representation of the hash code value as specified by <paramref name="format"/>.
    /// </returns>
    public string ToString(string format) => _hashCode.ToString(format);

    /// <summary>
    /// Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.
    /// </summary>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>
    /// The string representation of the hash code value as specified by <paramref name="format"/> and <paramref name="provider"/>.
    /// </returns>
    public string ToString(string format, IFormatProvider provider) => _hashCode.ToString(format, provider);

    /// <summary>
    /// Determines whether the specified object is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified object is a <see cref="HashCode"/> and is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj) => obj is HashCode code && Equals(code);

    /// <summary>
    /// Indicates whether the current object is equal to another <see cref="HashCode"/> instance.
    /// </summary>
    /// <param name="other">A <see cref="HashCode"/> instance to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(HashCode other) => _hashCode == other._hashCode;

    /// <summary>
    /// Performs an implicit conversion from <see cref="HashCode"/> to <see cref="int"/>.
    /// </summary>
    /// <param name="hashCode">The <see cref="HashCode"/> instance to convert.</param>
    /// <returns>
    /// The integer value of the hash code.
    /// </returns>
    public static implicit operator int(HashCode hashCode) => hashCode._hashCode;

    /// <summary>
    /// Determines whether two <see cref="HashCode"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="HashCode"/> to compare.</param>
    /// <param name="right">The second <see cref="HashCode"/> to compare.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="left"/> is equal to <paramref name="right"/>; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator ==(HashCode left, HashCode right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="HashCode"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="HashCode"/> to compare.</param>
    /// <param name="right">The second <see cref="HashCode"/> to compare.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="left"/> is not equal to <paramref name="right"/>; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator !=(HashCode left, HashCode right) => !(left == right);

    /// <summary>
    /// Computes a deterministic hash code for the specified string.
    /// </summary>
    /// <param name="text">The string to hash.</param>
    /// <returns>
    /// A 32-bit signed integer hash code for the string, or 0 if the string is null or empty.
    /// </returns>
    public static int HashString(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        int hash = DefaultSeed;

        unchecked
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < text.Length; index++)
                hash = (hash * Multiplier) + text[index];

        }

        return hash;
    }
}
