using FluentCommand.Internal;

namespace FluentCommand;

/// <summary>
/// A structure to hold concurrency token
/// </summary>
/// <remarks>
/// This structure is commonly used to represent SQL Server <c>rowversion</c> (also known as <c>timestamp</c>) columns for optimistic concurrency control.
/// </remarks>
public readonly struct ConcurrencyToken : IEquatable<ConcurrencyToken>
{
    /// <summary>
    /// The default empty token
    /// </summary>
    public static readonly ConcurrencyToken None = new([]);

    /// <summary>
    /// Gets the underlying value of the token.
    /// </summary>
    /// <value>
    /// The underlying value of the token.
    /// </value>
    public byte[] Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyToken"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public ConcurrencyToken(byte[] value)
    {
        Value = value ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyToken"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public ConcurrencyToken(string value)
    {
#if NET5_0_OR_GREATER
        Value = string.IsNullOrEmpty(value) ? [] : Convert.FromHexString(value);
#else
        Value = string.IsNullOrEmpty(value) ? [] : FromHexString(value);
#endif
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyToken"/> struct from a <see cref="long"/> value.
    /// </summary>
    /// <param name="value">The long value.</param>
    public ConcurrencyToken(long value)
    {
        Value = BitConverter.GetBytes(value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyToken"/> struct from a <see cref="ulong"/> value.
    /// </summary>
    /// <param name="value">The ulong value.</param>
    public ConcurrencyToken(ulong value)
    {
        Value = BitConverter.GetBytes(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
#if NET5_0_OR_GREATER
        return Value != null ? Convert.ToHexString(Value) : null;
#else
        return Value != null ? ToHexString(Value) : null;
#endif
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return obj is ConcurrencyToken token && Equals(token);
    }

    /// <inheritdoc />
    public bool Equals(ConcurrencyToken other)
    {
        return EqualityComparer<byte[]>.Default.Equals(Value, other.Value);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }


    /// <summary>
    /// Performs an implicit conversion from <see cref="ConcurrencyToken"/> to byte array.
    /// </summary>
    /// <param name="token">The concurrency token.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator byte[](ConcurrencyToken token) => token.Value;

    /// <summary>
    /// Performs an implicit conversion from <see cref="ConcurrencyToken"/> to <see cref="System.String"/>.
    /// </summary>
    /// <param name="token">The concurrency token.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator string(ConcurrencyToken token) => token.ToString();

    /// <summary>
    /// Performs an implicit conversion from <see cref="ConcurrencyToken"/> to <see cref="long"/>.
    /// </summary>
    /// <param name="token">The concurrency token.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator long(ConcurrencyToken token)
    {
        if (token.Value == null || token.Value.Length == 0)
            return 0L;

        if (token.Value.Length < sizeof(long))
            throw new InvalidCastException("The token value is too short to convert to a long.");

        // Use little-endian to match BitConverter default
        return BitConverter.ToInt64(token.Value, 0);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="ConcurrencyToken"/> to <see cref="ulong"/>.
    /// </summary>
    /// <param name="token">The concurrency token.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator ulong(ConcurrencyToken token)
    {
        if (token.Value == null || token.Value.Length == 0)
            return 0UL;

        if (token.Value.Length < sizeof(ulong))
            throw new InvalidCastException("The token value is too short to convert to a ulong.");

        // Use little-endian to match BitConverter default
        return BitConverter.ToUInt64(token.Value, 0);
    }


    /// <summary>
    /// Performs an implicit conversion from byte array to <see cref="ConcurrencyToken"/>.
    /// </summary>
    /// <param name="token">The concurrency token.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator ConcurrencyToken(byte[] token) => new(token);

    /// <summary>
    /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="ConcurrencyToken"/>.
    /// </summary>
    /// <param name="token">The concurrency token.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator ConcurrencyToken(string token) => new(token);

    /// <summary>
    /// Performs an implicit conversion from <see cref="long"/> to <see cref="ConcurrencyToken"/>.
    /// </summary>
    /// <param name="value">The long value.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator ConcurrencyToken(long value)
    {
        var bytes = BitConverter.GetBytes(value);
        return new ConcurrencyToken(bytes);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="ulong"/> to <see cref="ConcurrencyToken"/>.
    /// </summary>
    /// <param name="value">The ulong value.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator ConcurrencyToken(ulong value)
    {
        var bytes = BitConverter.GetBytes(value);
        return new ConcurrencyToken(bytes);
    }


    /// <summary>
    /// Determines whether two <see cref="ConcurrencyToken"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="ConcurrencyToken"/> to compare.</param>
    /// <param name="right">The second <see cref="ConcurrencyToken"/> to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the two <see cref="ConcurrencyToken"/> instances are equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(ConcurrencyToken left, ConcurrencyToken right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="ConcurrencyToken"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="ConcurrencyToken"/> to compare.</param>
    /// <param name="right">The second <see cref="ConcurrencyToken"/> to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the two <see cref="ConcurrencyToken"/> instances are not equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(ConcurrencyToken left, ConcurrencyToken right) => !(left == right);


#if NETSTANDARD2_0
    private static string ToHexString(byte[] bytes)
    {
        var hex = StringBuilderCache.Acquire(bytes.Length * 2);

        foreach (var b in bytes)
            hex.Append(b.ToString("X2"));

        return StringBuilderCache.ToString(hex);
    }

    private static byte[] FromHexString(string hexString)
    {
        var hexLength = hexString.Length;
        var bytes = new byte[hexLength / 2];

        for (var i = 0; i < hexLength; i += 2)
            bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);

        return bytes;
    }
#endif
}
