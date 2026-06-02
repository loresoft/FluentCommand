using FluentCommand.Extensions;

namespace FluentCommand.Tests;

public class ConvertExtensionsTests
{
    [Theory]
    [InlineData("true")]
    [InlineData("yes")]
    [InlineData("1")]
    [InlineData("on")]
    [InlineData("y")]
    [InlineData("t")]
    public void ToBooleanShouldConvertTrueValues(string value)
    {
        // Act
        var result = value.ToBoolean();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("false")]
    [InlineData("no")]
    [InlineData("0")]
    [InlineData("off")]
    [InlineData("n")]
    [InlineData("f")]
    [InlineData(null)]
    public void ToBooleanShouldConvertFalseValues(string? value)
    {
        // Act
        var result = value.ToBoolean();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SafeConvertWithEmptyNullableShouldReturnNull()
    {
        // Act
        var result = ConvertExtensions.SafeConvert(typeof(int?), string.Empty);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SafeConvertWithInvalidNullableInt32ShouldReturnNull()
    {
        // Act
        var result = ConvertExtensions.SafeConvert(typeof(int?), "invalid");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SafeConvertWithInvalidInt32ShouldReturnDefaultValue()
    {
        // Act
        var result = ConvertExtensions.SafeConvert(typeof(int), "invalid");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void SafeConvertWithGuidShouldConvertString()
    {
        // Arrange
        var expected = Guid.Parse("c7751251-3267-4a73-bfed-f9ec5c4dba9f");

        // Act
        var result = ConvertExtensions.SafeConvert(typeof(Guid), expected.ToString());

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertValueWithDbNullShouldReturnDefault()
    {
        // Act
        var result = ConvertExtensions.ConvertValue<int?>(DBNull.Value);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ConvertValueWithCustomConverterShouldUseConverter()
    {
        // Act
        var result = ConvertExtensions.ConvertValue("value", static value => value?.ToString()?.Length ?? 0);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void ConvertValueWithStringInt32ShouldConvertValue()
    {
        // Act
        var result = ConvertExtensions.ConvertValue<int?>("42");

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public void ConvertValueWithNullableDateOnlyShouldConvertDateTime()
    {
        // Arrange
        var expected = new DateOnly(2024, 2, 3);
        var value = new DateTime(2024, 2, 3, 4, 5, 6);

        // Act
        var result = ConvertExtensions.ConvertValue<DateOnly?>(value);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertValueWithNullableDateOnlyShouldConvertDateTimeOffset()
    {
        // Arrange
        var expected = new DateOnly(2024, 2, 3);
        var value = new DateTimeOffset(2024, 2, 3, 4, 5, 6, TimeSpan.Zero);

        // Act
        var result = ConvertExtensions.ConvertValue<DateOnly?>(value);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertValueWithNullableDateOnlyShouldConvertString()
    {
        // Arrange
        var expected = new DateOnly(2024, 2, 3);

        // Act
        var result = ConvertExtensions.ConvertValue<DateOnly?>("2024-02-03");

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertValueWithNullableTimeOnlyShouldConvertTimeSpan()
    {
        // Arrange
        var expected = new TimeOnly(4, 5, 6);
        var value = new TimeSpan(4, 5, 6);

        // Act
        var result = ConvertExtensions.ConvertValue<TimeOnly?>(value);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertValueWithNullableTimeOnlyShouldConvertString()
    {
        // Arrange
        var expected = new TimeOnly(4, 5, 6);

        // Act
        var result = ConvertExtensions.ConvertValue<TimeOnly?>("04:05:06");

        // Assert
        result.Should().Be(expected);
    }
}
