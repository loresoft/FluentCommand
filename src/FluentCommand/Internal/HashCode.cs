namespace FluentCommand.Internal;

/// <summary>
/// An immutable hash code structure
/// </summary>
/// <remarks>
/// Implements the Jon Skeet suggested implementation of GetHashCode(). 
/// </remarks>
public readonly struct HashCode
{
    /// <summary>
    /// The prime multiplier used to combine hash codes.
    /// </summary>
    public const int Multiplier = 31;

    private readonly int _hashCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="HashCode"/> struct.
    /// </summary>
    /// <param name="hashCode">The hash code.</param>
    public HashCode(int hashCode)
    {
        _hashCode = hashCode;
    }

    /// <summary>
    /// Gets a hash code seed value for combine hash codes values.
    /// </summary>
    /// <value>
    /// The hash code seed value.
    /// </value>
    public static HashCode Seed => new(17);

    /// <summary>
    /// Combines this hash code with the hash code of specified <paramref name="value" />.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value to combine hash codes with.</param>
    /// <returns>A new hash code combined with this and the values hash codes.</returns>
    public HashCode Combine<TValue>(TValue value)
    {
        var hashCode = value is null ? 0 : EqualityComparer<TValue>.Default.GetHashCode(value);
        unchecked
        {
            hashCode = _hashCode * Multiplier + hashCode;
        }

        return new HashCode(hashCode);
    }

    /// <summary>
    /// Combines this hash code with the hash code of specified <paramref name="value" />.
    /// </summary>
    /// <param name="value">The value to combine hash codes with.</param>
    /// <returns>A new hash code combined with this and the values hash codes.</returns>
    public HashCode Combine(string value)
    {
        // need to handle string values deterministically 
        var hashCode = HashString(value);
        unchecked
        {
            hashCode = _hashCode * Multiplier + hashCode;
        }

        return new HashCode(hashCode);
    }

    /// <summary>
    /// Combines this hash code with the hash code of specified <paramref name="value" />.
    /// </summary>
    /// <param name="value">The value to combine hash codes with.</param>
    /// <returns>A new hash code combined with this and the values hash codes.</returns>
    public HashCode Combine(object value)
    {
        // need to handle string values deterministically 
        return value switch
        {
            string text => Combine(text),
            _ => Combine(value is null ? 0 : value.GetHashCode()),
        };
    }

    /// <summary>
    /// Combines this hash code with the hash code of each item specified <paramref name="values" />.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="values">The values to combine hash codes with.</param>
    /// <returns>A new hash code combined with this and the values hash codes.</returns>
    public HashCode CombineAll<TValue>(IEnumerable<TValue> values)
    {
        if (values == null)
            return this;

        var comparer = EqualityComparer<TValue>.Default;
        var current = _hashCode;

        foreach (var value in values)
        {
            var hashCode = value is null ? 0 : comparer.GetHashCode(value);
            unchecked
            {
                hashCode = current * Multiplier + hashCode;
            }
            current = hashCode;
        }

        return new HashCode(current);
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
        return _hashCode;
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="HashCode"/> to <see cref="int"/>.
    /// </summary>
    /// <param name="hashCode">The hash code.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator int(HashCode hashCode)
    {
        return hashCode._hashCode;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return _hashCode.ToString();
    }

    /// <summary>
    /// Deterministic string hash
    /// </summary>
    /// <param name="text">The text to hash.</param>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public static int HashString(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        int hash = Seed;

        unchecked
        {
            foreach (char c in text)
                hash = hash * Multiplier + c;
        }

        return hash;
    }
}
