using System.Data;

using FluentCommand.Tests.Models;

using HashCode = FluentCommand.Internal.HashCode;

namespace FluentCommand.Tests.Internal;

public class HashCodeTests
{
    [Fact]
    public void HashCommandConsistent()
    {
        var command = new Command
        {
            Text = "SELECT * FROM dbo.Status WHERE Type = @type OR Name = @name",
            CommandType = CommandType.Text,
            Parameters =
            {
                new Parameter{ Name = "@type", Value = 1, DbType = DbType.Int32 },
                new Parameter{ Name = "@name", Value = "test", DbType = DbType.String },
            }
        };

        var commandHash = command.GetHashCode();

        commandHash.Should().Be(92316680);
    }

    [Fact]
    public void HashStringConsistent()
    {
        var stringHash = HashCode.Seed
            .Combine("This is a test")
            .GetHashCode();

        stringHash.Should().Be(1272545829);
    }

    [Fact]
    public void Combine_Int_ProducesExpectedHash()
    {
        var hash = HashCode.Seed.Combine(42);
        hash.GetHashCode().Should().NotBe(0);
    }

    [Fact]
    public void Combine_NullString_ProducesExpectedHash()
    {
        var hash = HashCode.Seed.Combine((string)null);
        hash.GetHashCode().Should().NotBe(0);
    }

    [Fact]
    public void Combine_Object_StringAndInt_Consistent()
    {
        var hash1 = HashCode.Seed.Combine("abc").Combine(123);
        var hash2 = HashCode.Seed.Combine("abc").Combine(123);
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void CombineAll_Enumerable_Consistent()
    {
        var values = new[] { "a", "b", "c" };
        var hash1 = HashCode.Seed.CombineAll(values);
        var hash2 = HashCode.Seed.CombineAll(values);
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashString_NullOrEmpty_ReturnsZero()
    {
        HashCode.HashString(null).Should().Be(0);
        HashCode.HashString(string.Empty).Should().Be(0);
    }

    [Fact]
    public void Equality_And_Operators_Work()
    {
        var hash1 = HashCode.Seed.Combine(1).Combine("a");
        var hash2 = HashCode.Seed.Combine(1).Combine("a");
        var hash3 = HashCode.Seed.Combine(2).Combine("b");

        (hash1 == hash2).Should().BeTrue();
        (hash1 != hash3).Should().BeTrue();
        hash1.Equals(hash2).Should().BeTrue();
        hash1.Equals((object)hash2).Should().BeTrue();
        hash1.Equals(hash3).Should().BeFalse();
    }

    [Fact]
    public void ImplicitConversion_ToInt_Works()
    {
        var hash = HashCode.Seed.Combine(5);
        int intHash = hash;
        intHash.Should().Be(hash.GetHashCode());
    }

    [Fact]
    public void ToString_And_Formatters_Work()
    {
        var hash = HashCode.Seed.Combine(123);
        hash.ToString().Should().Be(hash.GetHashCode().ToString());
        hash.ToString("X").Should().Be(hash.GetHashCode().ToString("X"));
        hash.ToString(System.Globalization.CultureInfo.InvariantCulture).Should().Be(hash.GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture));
        hash.ToString("X", System.Globalization.CultureInfo.InvariantCulture).Should().Be(hash.GetHashCode().ToString("X", System.Globalization.CultureInfo.InvariantCulture));
    }
}
