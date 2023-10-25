using FluentCommand.Internal;

namespace FluentCommand;

/// <summary>
/// A structure to hold concurrency token
/// </summary>
public readonly struct ConcurrencyToken : IEquatable<ConcurrencyToken>
{
    /// <summary>
    /// The default empty token
    /// </summary>
    public static readonly ConcurrencyToken None = new(Array.Empty<byte>());

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
        Value = value ?? Array.Empty<byte>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyToken"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public ConcurrencyToken(string value)
    {
#if NET5_0_OR_GREATER
        Value = string.IsNullOrEmpty(value) ? Array.Empty<byte>() : Convert.FromHexString(value);
#else
        Value = string.IsNullOrEmpty(value) ? Array.Empty<byte>() : FromHexString(value);
#endif
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
