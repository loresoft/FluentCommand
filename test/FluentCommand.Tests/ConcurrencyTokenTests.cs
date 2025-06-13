namespace FluentCommand.Tests;

public class ConcurrencyTokenTests
{
    [Fact]
    public void Constructor_WithByteArray_SetsValue()
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        var token = new ConcurrencyToken(bytes);

        token.Value.Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public void Constructor_WithNullByteArray_SetsEmptyValue()
    {
        var token = new ConcurrencyToken((byte[])null);

        token.Value.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithHexString_SetsValue()
    {
        var hex = "01020304";
        var expected = new byte[] { 1, 2, 3, 4 };
        var token = new ConcurrencyToken(hex);

        token.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Constructor_WithNullString_SetsEmptyValue()
    {
        var token = new ConcurrencyToken((string)null);

        token.Value.Should().BeEmpty();
    }

    [Fact]
    public void ToString_ReturnsHexString()
    {
        var bytes = new byte[] { 10, 11, 12, 13 };
        var token = new ConcurrencyToken(bytes);

        token.ToString().Should().Be("0A0B0C0D");
    }

    [Fact]
    public void Equals_ReturnsTrueForSameValue()
    {
        var bytes = new byte[] { 5, 6, 7 };
        var token1 = new ConcurrencyToken(bytes);
        var token2 = new ConcurrencyToken(bytes);

        token1.Equals(token2).Should().BeTrue();
        (token1 == token2).Should().BeTrue();
        (token1 != token2).Should().BeFalse();
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentValue()
    {
        var token1 = new ConcurrencyToken([1, 2]);
        var token2 = new ConcurrencyToken([3, 4]);

        token1.Equals(token2).Should().BeFalse();
        (token1 == token2).Should().BeFalse();
        (token1 != token2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_ToByteArray()
    {
        var bytes = new byte[] { 8, 9 };
        ConcurrencyToken token = bytes;

        byte[] result = token;
        result.Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public void ImplicitConversion_ToString()
    {
        var bytes = new byte[] { 0xAA, 0xBB };
        ConcurrencyToken token = bytes;

        string result = token;
        result.Should().Be("AABB");
    }

    [Fact]
    public void ImplicitConversion_FromString()
    {
        string hex = "A1B2";
        ConcurrencyToken token = hex;

        token.Value.Should().BeEquivalentTo(new byte[] { 0xA1, 0xB2 });
    }

    [Fact]
    public void None_ReturnsEmptyToken()
    {
        var token = ConcurrencyToken.None;

        token.Value.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithLong_SetsValue()
    {
        long value = 0x0102030405060708;
        var token = new ConcurrencyToken(value);

        token.Value.Should().BeEquivalentTo(BitConverter.GetBytes(value));
    }

    [Fact]
    public void Constructor_WithULong_SetsValue()
    {
        ulong value = 0x0102030405060708UL;
        var token = new ConcurrencyToken(value);

        token.Value.Should().BeEquivalentTo(BitConverter.GetBytes(value));
    }

    [Fact]
    public void ImplicitConversion_FromLong()
    {
        long value = 0x1122334455667788;
        ConcurrencyToken token = value;

        token.Value.Should().BeEquivalentTo(BitConverter.GetBytes(value));
    }

    [Fact]
    public void ImplicitConversion_FromULong()
    {
        ulong value = 0x8877665544332211UL;
        ConcurrencyToken token = value;

        token.Value.Should().BeEquivalentTo(BitConverter.GetBytes(value));
    }

    [Fact]
    public void ImplicitConversion_ToLong()
    {
        long value = 0x0F0E0D0C0B0A0908;
        var token = new ConcurrencyToken(value);

        long result = token;
        result.Should().Be(value);
    }

    [Fact]
    public void ImplicitConversion_ToULong()
    {
        ulong value = 0x08090A0B0C0D0E0FUL;
        var token = new ConcurrencyToken(value);

        ulong result = token;
        result.Should().Be(value);
    }

    [Fact]
    public void ImplicitConversion_ToLong_ThrowsIfTooShort()
    {
        var token = new ConcurrencyToken([1, 2, 3]); // less than 8 bytes

        Action act = () => { long _ = token; };
        act.Should().Throw<InvalidCastException>();
    }

    [Fact]
    public void ImplicitConversion_ToULong_ThrowsIfTooShort()
    {
        var token = new ConcurrencyToken([1, 2, 3]); // less than 8 bytes

        Action act = () => { ulong _ = token; };
        act.Should().Throw<InvalidCastException>();
    }
}
