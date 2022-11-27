using FluentCommand.Internal;

namespace FluentCommand;

public readonly struct ConcurrencyToken : IEquatable<ConcurrencyToken>
{
    public static readonly ConcurrencyToken None = new(Array.Empty<byte>());

    public byte[] Value { get; }

    public ConcurrencyToken(byte[] value)
    {
        Value = value ?? Array.Empty<byte>();
    }

    public ConcurrencyToken(string value)
    {
#if !NETSTANDARD2_0
        Value = string.IsNullOrEmpty(value) ? Array.Empty<byte>() : Convert.FromHexString(value);
#else
        Value = string.IsNullOrEmpty(value) ? Array.Empty<byte>() : FromHexString(value);
#endif
    }

    public override string ToString()
    {
#if !NETSTANDARD2_0
        return Value != null ? Convert.ToHexString(Value) : null;
#else
        return Value != null ? ToHexString(Value) : null;
#endif
    }

    public override bool Equals(object obj)
    {
        return obj is ConcurrencyToken token && Equals(token);
    }

    public bool Equals(ConcurrencyToken other)
    {
        return EqualityComparer<byte[]>.Default.Equals(Value, other.Value);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static implicit operator byte[](ConcurrencyToken token) => token.Value;

    public static implicit operator string(ConcurrencyToken token) => token.ToString();

    public static implicit operator ConcurrencyToken(byte[] token) => new(token);

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
