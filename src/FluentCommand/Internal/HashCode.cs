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
}
